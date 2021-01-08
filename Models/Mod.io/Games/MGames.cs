using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace TCAdminCustomMods.Models.Mod.io.Games
{
    public class MGames : MListBase
    {
        [JsonProperty("data")] public IList<MGame> Data { get; set; }

        public static MGames GetGames()
        {
            var restClient = new RestClient(ModIoBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            var restRequest = new RestRequest("/v1/games");
            restRequest.AddQueryParameter("api_key", modIoConfiguration.ApiKey);
            
            var restResponse = restClient.Get<MGames>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}