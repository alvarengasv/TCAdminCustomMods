using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TCAdminCustomMods.Models.Mod.io.Mods;

namespace TCAdminCustomMods.Models.Mod.io.Games
{
    public class TagOption
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("tags")] public IList<string> Tags { get; set; }

        [JsonProperty("hidden")] public bool Hidden { get; set; }
        
        public static IList<TagOption> GetTagsForGame(int gameId)
        {
            var restClient = new RestClient(ModIoBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            var restRequest = new RestRequest($"/v1/games/{gameId}");
            restRequest.AddQueryParameter("api_key", modIoConfiguration.ApiKey);

            var restResponse = restClient.Get<MGame>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data.TagOptions : null;
        }
    }
}