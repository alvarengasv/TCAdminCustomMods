using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace TCAdminCustomMods.Models.UMod
{
    public class UModBrowser
    {
        public const string BaseUrl = "https://umod.org/";

        [JsonProperty("current_page")] public int CurrentPage { get; set; }

        [JsonProperty("data")] public IList<ModData> Mods { get; set; }

        [JsonProperty("first_page_url")] public string FirstPageUrl { get; set; }

        [JsonProperty("from")] public int From { get; set; }

        [JsonProperty("last_page")] public int LastPage { get; set; }

        [JsonProperty("last_page_url")] public string LastPageUrl { get; set; }

        [JsonProperty("next_page_url")] public string NextPageUrl { get; set; }

        [JsonProperty("path")] public string Path { get; set; }

        [JsonProperty("per_page")] public int PerPage { get; set; }

        [JsonProperty("prev_page_url")] public object PrevPageUrl { get; set; }

        [JsonProperty("to")] public int To { get; set; }

        [JsonProperty("total")] public int Total { get; set; }

        public static UModBrowser Search(string query = "", int page = 1, string category = "", string sort = "title",
            string sortDirection = "asc")
        {
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("/plugins/search.json");
            if(!string.IsNullOrEmpty(query))
                restRequest.AddQueryParameter("query", query);
            restRequest.AddQueryParameter("page", page.ToString());
            restRequest.AddQueryParameter("sort", sort);
            restRequest.AddQueryParameter("sortdir", sortDirection);
            if (!string.IsNullOrEmpty(category))
            {
                restRequest.AddQueryParameter("categories[]", category);
            }
            restRequest.AddQueryParameter("categories[]", "universal");

            var restResponse = restClient.Get<UModBrowser>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}