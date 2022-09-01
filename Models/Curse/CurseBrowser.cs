using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.Curse
{
    public class CurseBrowserData
    {
        [JsonProperty("data")] public List<CurseBrowser> Data { get; set; }
    }
    public class CurseBrowserSingleData
    {
        [JsonProperty("data")] public CurseBrowser Data { get; set; }
    }
    public class CurseBrowserFileData
    {
        [JsonProperty("data")] public List<LatestFile> Data { get; set; }
    }
    public class CurseBrowser : GenericMod
    {
        public const string CURSE_API_KEY = "$2a$10$.QcC5bd.hAkR1OMKMp771uEv.Ygt7ralelC2EubRMT9lpUAGCBON2";
        public const string BaseUrl = "https://api.curseforge.com/";
        
        [JsonProperty("id")] public override string Id { get; set; }

        [JsonProperty("allowModDistribution")] public bool AllowModDistribution { get; set; }

        [JsonProperty("name")] public override string Name { get; set; }

        [JsonProperty("authors")] public IList<Author> Authors { get; set; }

        [JsonProperty("logo")] public Logo Logo { get; set; }
        [JsonProperty("links")] public Links Links { get; set; }

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
            var restRequest = new RestRequest("/v1/mods/" + id);
            restRequest.AddHeader("x-api-key", CURSE_API_KEY);

            var restResponse = restClient.Get<CurseBrowserSingleData>(restRequest);
            return restResponse.IsSuccessful && restResponse.Data.Data.AllowModDistribution ? restResponse.Data.Data : null;
        }

        public static List<CurseBrowser> Search(string query = "", int page = 0, int pageSize = 20, string category = "", string gameVersion = "", string sectionId = "6", string sort = "2", string sortOrder = "desc")
        {
            page--; //Index starts at 0.
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("/v1/mods/search");
            restRequest.AddHeader("x-api-key", CURSE_API_KEY);
            restRequest.AddQueryParameter("categoryId", "0");
            restRequest.AddQueryParameter("searchFilter", query);
            restRequest.AddQueryParameter("index", (page * pageSize).ToString());
            restRequest.AddQueryParameter("sortField", sort); //2=Popularity,4=Name
            restRequest.AddQueryParameter("sortOrder", sortOrder);
            restRequest.AddQueryParameter("gameId", "432");
            restRequest.AddQueryParameter("gameVersion", gameVersion);
            restRequest.AddQueryParameter("pageSize", pageSize.ToString());
            restRequest.AddQueryParameter("classId", sectionId); //6=Mods, 4471=ModPacks

            //Console.WriteLine("URL: " + restClient.BuildUri(restRequest));

            var restResponse = restClient.Get<CurseBrowserData>(restRequest);
            //Console.WriteLine(restResponse.Content);
            return restResponse.IsSuccessful ? restResponse.Data.Data.FindAll(c=>c.AllowModDistribution) : null;
        }
        public static List<LatestFile> GetFiles(int id)
        {
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest(string.Format("/v1/mods/{0}/files", id));
            restRequest.AddHeader("x-api-key", CURSE_API_KEY);
            restRequest.AddQueryParameter("pageSize", "100");
            var restResponse = restClient.Get<CurseBrowserFileData>(restRequest);
            return restResponse.Data.Data;
        }
    }
}