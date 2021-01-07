using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.Mod.io.Games;
using TCAdminCustomMods.Models.Mod.io.Generic;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class MMod : GenericMod
    {
        [JsonProperty("id")] public override string Id { get; set; }

        [JsonProperty("game_id")] public int GameId { get; set; }

        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("visible")] public int Visible { get; set; }

        [JsonProperty("submitted_by")] public SubmittedBy SubmittedBy { get; set; }

        [JsonProperty("date_added")] public int DateAdded { get; set; }

        [JsonProperty("date_updated")] public int DateUpdated { get; set; }

        [JsonProperty("date_live")] public int DateLive { get; set; }

        [JsonProperty("maturity_option")] public int MaturityOption { get; set; }

        [JsonProperty("logo")] public Logo Logo { get; set; }

        [JsonProperty("homepage_url")] public string HomepageUrl { get; set; }

        [JsonProperty("name")] public override string Name { get; set; }

        [JsonProperty("name_id")] public string NameId { get; set; }

        [JsonProperty("summary")] public string Summary { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("description_plaintext")]
        public string DescriptionPlaintext { get; set; }

        [JsonProperty("metadata_blob")] public string MetadataBlob { get; set; }

        [JsonProperty("profile_url")] public string ProfileUrl { get; set; }

        [JsonProperty("media")] public Media Media { get; set; }

        [JsonProperty("modfile")] public Modfile Modfile { get; set; }

        [JsonProperty("metadata_kvp")] public IList<MetadataKvp> MetadataKvp { get; set; }

        [JsonProperty("tags")] public IList<Tag> Tags { get; set; }

        [JsonProperty("stats")] public Stats Stats { get; set; }

        public static MMod GetModForGame(int gameId, int modId)
        {
            var restClient = new RestClient(ModIoBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            var restRequest = new RestRequest($"/v1/games/{gameId}/mods/{modId}");
            restRequest.AddQueryParameter("api_key", modIoConfiguration.ApiKey);

            var restResponse = restClient.Get<MMod>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}