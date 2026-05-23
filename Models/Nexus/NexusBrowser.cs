using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace TCAdminCustomMods.Models.Nexus
{
    public static class NexusBrowser
    {
        public const string BaseUrl = "https://api.nexusmods.com/";
        public const string GraphQlUrl = "https://api.nexusmods.com/v2/graphql";
        public const string UserAgent = "TCAdminCustomMods";

        public enum SortField
        {
            Endorsements,
            Downloads,
            UpdatedAt,
            CreatedAt,
            Name,
            Relevance
        }

        public static NexusMod GetMod(string gameDomain, string modId, string apiKey)
        {
            var client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
            var request = new RestRequest(string.Format("/v1/games/{0}/mods/{1}.json", gameDomain, modId));
            AddCommonHeaders(request, apiKey);
            var response = client.Get<NexusMod>(request);
            if (!response.IsSuccessful || response.Data == null) return null;
            var mod = response.Data;
            mod.Id = mod.ModId.ToString();
            return mod;
        }

        public static List<NexusModFile> GetFiles(string gameDomain, string modId, string apiKey)
        {
            var client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
            var request = new RestRequest(string.Format("/v1/games/{0}/mods/{1}/files.json", gameDomain, modId));
            AddCommonHeaders(request, apiKey);
            var response = client.Get<NexusFilesResponse>(request);
            if (!response.IsSuccessful || response.Data == null || response.Data.Files == null)
                return new List<NexusModFile>();
            return response.Data.Files;
        }

        public static NexusDownloadLink GetDownloadLink(string gameDomain, string modId, int fileId, string apiKey)
        {
            var client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
            var request = new RestRequest(string.Format("/v1/games/{0}/mods/{1}/files/{2}/download_link.json",
                gameDomain, modId, fileId));
            AddCommonHeaders(request, apiKey);
            var response = client.Get<List<NexusDownloadLink>>(request);
            if (!response.IsSuccessful || response.Data == null || response.Data.Count == 0) return null;
            return response.Data[0];
        }

        public static List<NexusCategory> GetCategories(string gameDomain, string apiKey)
        {
            var client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
            var request = new RestRequest(string.Format("/v1/games/{0}.json", gameDomain));
            AddCommonHeaders(request, apiKey);
            var response = client.Get<JObject>(request);
            if (!response.IsSuccessful || response.Data == null) return new List<NexusCategory>();
            var arr = response.Data["categories"] as JArray;
            if (arr == null) return new List<NexusCategory>();
            var categories = arr.ToObject<List<NexusCategory>>() ?? new List<NexusCategory>();
            // Drop the root category (parent_category is false) — that's the game's own root entry.
            categories.RemoveAll(c =>
            {
                var p = c.ParentCategory;
                return p == null || (p is bool && !(bool)p);
            });
            return categories;
        }

        public static NexusSearchResult Search(
            string gameDomain,
            string searchText,
            SortField sort,
            string categoryName,
            bool includeAdultContent,
            int offset,
            int count,
            string apiKey)
        {
            var filter = new JObject();

            if (!string.IsNullOrEmpty(gameDomain))
            {
                filter["gameDomainName"] = new JArray(new JObject
                {
                    ["value"] = gameDomain,
                    ["op"] = "EQUALS"
                });
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                filter["name"] = new JArray(new JObject
                {
                    ["value"] = searchText,
                    ["op"] = "WILDCARD"
                });
            }

            if (!string.IsNullOrEmpty(categoryName))
            {
                filter["categoryName"] = new JArray(new JObject
                {
                    ["value"] = categoryName,
                    ["op"] = "EQUALS"
                });
            }

            if (!includeAdultContent)
            {
                filter["adultContent"] = new JArray(new JObject
                {
                    ["value"] = false,
                    ["op"] = "EQUALS"
                });
            }

            var sortNode = BuildSort(sort, !string.IsNullOrEmpty(searchText));

            var variables = new JObject
            {
                ["f"] = filter,
                ["sort"] = new JArray(sortNode),
                ["offset"] = offset,
                ["count"] = count
            };

            const string query =
                "query($f:ModsFilter,$sort:[ModsSort!],$offset:Int,$count:Int){" +
                "mods(filter:$f,sort:$sort,offset:$offset,count:$count){" +
                "totalCount nodesCount " +
                "nodes{modId name summary author downloads endorsements version pictureUrl thumbnailUrl " +
                "adultContent createdAt updatedAt " +
                "modCategory{name} game{domainName id}}}}";

            var body = new JObject
            {
                ["query"] = query,
                ["variables"] = variables
            };

            var client = new RestClient(GraphQlUrl);
            client.UseNewtonsoftJson();
            var request = new RestRequest("", Method.POST);
            AddCommonHeaders(request, apiKey);
            request.AddParameter("application/json", body.ToString(Newtonsoft.Json.Formatting.None), ParameterType.RequestBody);
            var response = client.Execute<JObject>(request);

            var result = new NexusSearchResult();
            if (!response.IsSuccessful || response.Data == null) return result;
            var modsRoot = response.Data["data"]?["mods"];
            if (modsRoot == null) return result;

            result.TotalCount = modsRoot.Value<int?>("totalCount") ?? 0;
            var nodes = modsRoot["nodes"] as JArray ?? new JArray();
            foreach (var n in nodes)
            {
                var mod = new NexusMod
                {
                    ModId = n.Value<int?>("modId") ?? 0,
                    Name = (string)n["name"],
                    Summary = (string)n["summary"],
                    Author = (string)n["author"],
                    Version = (string)n["version"],
                    PictureUrl = (string)(n["pictureUrl"] ?? n["thumbnailUrl"]),
                    ModDownloads = n.Value<long?>("downloads") ?? 0,
                    EndorsementCount = n.Value<long?>("endorsements") ?? 0,
                    ContainsAdultContent = n.Value<bool?>("adultContent") ?? false,
                    Category = (string)n["modCategory"]?["name"],
                    DomainName = (string)n["game"]?["domainName"],
                    GameId = n["game"]?.Value<int?>("id") ?? 0
                };
                mod.Id = mod.ModId.ToString();
                result.Mods.Add(mod);
            }
            return result;
        }

        private static JObject BuildSort(SortField sort, bool hasSearch)
        {
            string field;
            string dir = "DESC";
            switch (sort)
            {
                case SortField.Downloads:
                    field = "downloads";
                    break;
                case SortField.UpdatedAt:
                    field = "updatedAt";
                    break;
                case SortField.CreatedAt:
                    field = "createdAt";
                    break;
                case SortField.Name:
                    field = "name";
                    dir = "ASC";
                    break;
                case SortField.Relevance:
                    if (hasSearch)
                    {
                        field = "relevance";
                    }
                    else
                    {
                        field = "endorsements";
                    }
                    break;
                case SortField.Endorsements:
                default:
                    field = "endorsements";
                    break;
            }
            return new JObject
            {
                [field] = new JObject { ["direction"] = dir }
            };
        }

        public static List<string> GetArchiveFilePaths(string contentPreviewUrl)
        {
            if (string.IsNullOrEmpty(contentPreviewUrl)) return new List<string>();

            var client = new RestClient(contentPreviewUrl);
            client.UseNewtonsoftJson();
            var request = new RestRequest("");
            var response = client.Get<NexusContentPreviewNode>(request);
            if (!response.IsSuccessful || response.Data == null) return new List<string>();

            var paths = new List<string>();
            CollectFilePaths(response.Data, paths);
            return paths;
        }

        private static void CollectFilePaths(NexusContentPreviewNode node, List<string> paths)
        {
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (string.Equals(child.Type, "directory", System.StringComparison.OrdinalIgnoreCase))
                    {
                        CollectFilePaths(child, paths);
                    }
                    else if (!string.IsNullOrEmpty(child.Path))
                    {
                        paths.Add(child.Path);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a path coming from a Nexus archive preview is a safe relative path:
        /// not absolute, no drive letters, no parent-traversal segments, no empty segments.
        /// </summary>
        public static bool IsSafeRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            var first = path[0];
            if (first == '/' || first == '\\') return false;

            var normalized = path.Replace('\\', '/');
            var segments = normalized.Split('/');
            foreach (var seg in segments)
            {
                if (string.IsNullOrEmpty(seg)) return false;
                if (seg == "..") return false;
                if (seg == ".") return false;
                if (seg.IndexOf(':') >= 0) return false;
            }
            return true;
        }

        private static void AddCommonHeaders(RestRequest request, string apiKey)
        {
            request.AddHeader("apikey", apiKey ?? string.Empty);
            request.AddHeader("User-Agent", UserAgent);
            request.AddHeader("Accept", "application/json");
        }
    }
}
