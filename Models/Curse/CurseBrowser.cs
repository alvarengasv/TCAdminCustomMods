using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.Curse
{
    public class CurseBrowser : GenericMod
    {
        public const string BaseUrl = "https://addons-ecs.forgesvc.net/";

        [JsonProperty("id")] public override string Id { get; set; }

        [JsonProperty("name")] public override string Name { get; set; }

        [JsonProperty("authors")] public IList<Author> Authors { get; set; }

        [JsonProperty("attachments")] public IList<Attachment> Attachments { get; set; }

        [JsonProperty("websiteUrl")] public string WebsiteUrl { get; set; }

        [JsonProperty("gameId")] public int GameId { get; set; }

        [JsonProperty("summary")] public string Summary { get; set; }

        [JsonProperty("defaultFileId")] public int DefaultFileId { get; set; }

        [JsonProperty("downloadCount")] public double DownloadCount { get; set; }

        [JsonProperty("latestFiles")] public IList<LatestFile> LatestFiles { get; set; }

        [JsonProperty("categories")] public IList<Category> Categories { get; set; }

        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("primaryCategoryId")] public int PrimaryCategoryId { get; set; }

        [JsonProperty("categorySection")] public CategorySection CategorySection { get; set; }

        [JsonProperty("slug")] public string Slug { get; set; }

        [JsonProperty("gameVersionLatestFiles")]
        public IList<GameVersionLatestFile> GameVersionLatestFiles { get; set; }

        [JsonProperty("isFeatured")] public bool IsFeatured { get; set; }

        [JsonProperty("popularityScore")] public double PopularityScore { get; set; }

        [JsonProperty("gamePopularityRank")] public int GamePopularityRank { get; set; }

        [JsonProperty("primaryLanguage")] public string PrimaryLanguage { get; set; }

        [JsonProperty("gameSlug")] public string GameSlug { get; set; }

        [JsonProperty("gameName")] public string GameName { get; set; }

        [JsonProperty("portalName")] public string PortalName { get; set; }

        [JsonProperty("dateModified")] public DateTime DateModified { get; set; }

        [JsonProperty("dateCreated")] public DateTime DateCreated { get; set; }

        [JsonProperty("dateReleased")] public DateTime DateReleased { get; set; }

        [JsonProperty("isAvailable")] public bool IsAvailable { get; set; }

        [JsonProperty("isExperiemental")] public bool IsExperiemental { get; set; }

        public static CurseBrowser GetMod(int id)
        {
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("/api/v2/addon/" + id);
            var restResponse = restClient.Get<CurseBrowser>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }

        public static List<CurseBrowser> Search(string query = "", int page = 0, int pageSize = 20, string category = "", string gameVersion = "")
        {
            page--; //Index starts at 0.
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("/api/v2/addon/search");
            restRequest.AddQueryParameter("categoryId", "0");
            restRequest.AddQueryParameter("searchFilter", query);
            restRequest.AddQueryParameter("index", (page * pageSize).ToString());
            restRequest.AddQueryParameter("sort", "name");
            restRequest.AddQueryParameter("gameId", "432");
            restRequest.AddQueryParameter("gameVersion", gameVersion);
            restRequest.AddQueryParameter("pageSize", pageSize.ToString());
            restRequest.AddQueryParameter("sectionId", "6");
            
            var restResponse = restClient.Get<List<CurseBrowser>>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}