using System.Collections.Generic;
using Alexr03.Common.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class MMods : MListBase
    {
        private static readonly Logger Logger = new Logger("MMods_logger");
        [JsonProperty("data")] public IList<MMod> Data { get; set; }

        public static MMods GetModsForGame(int gameId, string query = "", int page = 1, int pageSize = 16, List<string> tags = null)
        {
            var restClient = new RestClient(ModIoBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            var restRequest = new RestRequest($"/v1/games/{gameId}/mods");
            restRequest.AddQueryParameter("_limit", pageSize.ToString());
            if (page >= 0)
            {
                restRequest.AddQueryParameter("_offset", (pageSize * page).ToString());
            }

            restRequest.AddQueryParameter("api_key", modIoConfiguration.ApiKey);
            restRequest.AddQueryParameter("_sort", "name");
            restRequest.AddQueryParameter("_q", query);
            if (tags != null)
            {
                restRequest.AddQueryParameter("tags", string.Join(",", tags));
            }
            
            Logger.Debug(restClient.BuildUri(restRequest).ToString());

            var restResponse = restClient.Get<MMods>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}