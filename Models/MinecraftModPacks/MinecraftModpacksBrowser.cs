using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.MinecraftModPacks
{
    public class MinecraftModpacksBrowser : GenericMod
    {
        private class ModPackSearchResult
        {
            public int[] Packs { get; set; } = new int[] { };
            public int[] Curseforge { get; set; } = new int[] { };
            public int Total { get; set; }
            public int Limit { get; set; }
            public long Refreshed { get; set; }

            public static ModPackSearchResult Search(string sortBy = "modpack/updated", string query = "")
            {
                var restClient = new RestClient(BaseUrl);
                restClient.UseNewtonsoftJson();
                var restRequest = new RestRequest(sortBy + "/5000");
                if (!string.IsNullOrEmpty(query))
                {
                    restRequest.AddQueryParameter("term", query);
                }

                //Console.WriteLine("Search URL: " + restClient.BuildUri(restRequest));

                var restResponse = restClient.Get<ModPackSearchResult>(restRequest);
                //Console.WriteLine("restResponse: {0}", restResponse.Data.packs.Length);

                if (restResponse.IsSuccessful)
                {
                    //Console.WriteLine("Search returned count: {0}", restResponse.Data.Packs.Length);
                    return restResponse.Data;
                }

                return null;
            }
        }

        //https://modpacksch.docs.apiary.io/#/reference
        public const string BaseUrl = "https://api.modpacks.ch/public/";
        public const string BaseCurseforgeUrl = "https://api.modpacks.ch/public/curseforge/";

        public override string Id { get; set; }
        public override string Name { get; set; }
        public string Synopsis { get; set; }
        public string Description { get; set; }
        public long Installs { get; set; }
        public long Plays { get; set; }
        public bool Featured { get; set; }
        public long Refreshed { get; set; }
        public string Notification { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public long Updated { get; set; }
        public MinecraftModpacksArt[] Art { get; set; }
        public MinecraftModpacksVersion[] Versions {get;set;}
        public static MinecraftModpacksBrowser GetPack(int packId)
        {
            var restClient = new RestClient(BaseUrl + "modpack/");
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest(packId.ToString());

            //Console.WriteLine("URL: " + restClient.BuildUri(restRequest));

            var restResponse = restClient.Get<MinecraftModpacksBrowser>(restRequest);
            //Console.WriteLine("restResponse: {0}", restResponse.Data.packs.Length);

            if (restResponse.IsSuccessful && restResponse.Data.Status == "success" && !string.IsNullOrEmpty(restResponse.Data.Name))
            {
                return restResponse.Data;
            }

            //Console.WriteLine("Failure pack id: {0}", packId);

            return new MinecraftModpacksBrowser()
            {
                Id = packId.ToString(),
                Name = "Unknown Modpack",
                Status="error"
            };

        }
        public static MinecraftModpacksBrowser GetCurseforgePack(int packId)
        {
            var restClient = new RestClient(BaseCurseforgeUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest(packId.ToString());

            //Console.WriteLine("URL: " + restClient.BuildUri(restRequest));

            var restResponse = restClient.Get<MinecraftModpacksBrowser>(restRequest);
            //Console.WriteLine("restResponse: {0}", restResponse.Data.packs.Length);

            if (restResponse.IsSuccessful && restResponse.Data.Status == "success" && !string.IsNullOrEmpty(restResponse.Data.Name))
            {
                return restResponse.Data;
            }

            //Console.WriteLine("Failure pack id: {0}", packId);

            return new MinecraftModpacksBrowser()
            {
                Id = packId.ToString(),
                Name = "Unknown Modpack",
                Status = "error"
            };

        }

        public static List<MinecraftModpacksBrowser> Search(string sortBy= "modpack/updated", string query = "", int page = 1, int pageSize = 10)
        {
            if (sortBy.StartsWith("curseforge/"))
            {
                var packs = new List<MinecraftModpacksBrowser>();
                var sort = sortBy.Split('/')[1];
                var cursepacks = Curse.CurseBrowser.Search(query: query, page: page, pageSize: pageSize, sectionId: "4471", sort: sort);
                foreach (var cursepack in cursepacks)
                {
                    packs.Add(FromCurseModpack(cursepack));
                }
                return packs;
            }


            var search = ModPackSearchResult.Search(sortBy, query);

            if (search == null)
            {
                return null;
            }

            var modinfos = new List<MinecraftModpacksBrowser>();
            var skip = (page - 1) * pageSize;
            var currentpacks = search.Packs.Concat(search.Curseforge).ToArray().Skip(skip).Take(pageSize).ToArray();

            var listOfActions = new List<Action>();
            var lockobj = new object();
            currentpacks.ToList().ForEach(p => { 
                if (search.Packs.Contains(p))
                {
                    listOfActions.Add(() =>
                    {
                        var modpack = MinecraftModpacksBrowser.GetPack(p);
                        lock (lockobj)
                        {
                            modinfos.Add(modpack);
                        }
                    });
                }
                else
                {
                    listOfActions.Add(() =>
                    {
                        var modpack = MinecraftModpacksBrowser.GetCurseforgePack(p);
                        lock (lockobj)
                        {
                            modinfos.Add(modpack);
                        }
                    });
                }
            }
            );

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
            Parallel.Invoke(options, listOfActions.ToArray());

            //Items could have wrong order. Order them again.
            var orderedmodinfos = new List<MinecraftModpacksBrowser>();
            for (int i = 0; i < currentpacks.Length; i++)
            {
                var packid = currentpacks[i].ToString();
                orderedmodinfos.Add(modinfos.SingleOrDefault(m => m.Id == packid));
            }
            return orderedmodinfos;
        }

        private static MinecraftModpacksBrowser FromCurseModpack(Curse.CurseBrowser curseModpack)
        {
            var pack = new MinecraftModpacksBrowser()
            {
                Id = curseModpack.Id,
                Name = curseModpack.Name,
                Description = curseModpack.Summary,
                Status = "success",
                Synopsis = curseModpack.Summary,
                Type = "Curseforge",
                Updated = ((DateTimeOffset)curseModpack.DateReleased.ToUniversalTime()).ToUnixTimeMilliseconds(),
                Installs = 0,
                Featured = false,
                Plays = 0,
                Refreshed = ((DateTimeOffset)curseModpack.DateModified.ToUniversalTime()).ToUnixTimeMilliseconds(),
                Notification = string.Empty,
                Art = new MinecraftModpacksArt[] {
                    new MinecraftModpacksArt() {
                     Url=curseModpack.Attachments[0].Url}
                }
            };

            var versions = new List<MinecraftModpacksVersion>();
            foreach(var version in curseModpack.GameVersionLatestFiles)
            {
                versions.Add(new MinecraftModpacksVersion()
                {
                    Id = version.ProjectFileId,
                    Name = string.Format("{0} {1}", pack.Name, version.GameVersion),
                    Type = "Release"
                });
            }
            pack.Versions = versions.ToArray();

            return pack;
        }

        public static string[] GetTags()
        {
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("https://api.modpacks.ch/public/tag/popular/100");

            var restResponse = restClient.Get<MinecraftModpacksTags>(restRequest);

            if (restResponse.IsSuccessful)
            {
                return restResponse.Data.Tags;
            }

            return new string[] { };
        }
    }
}
