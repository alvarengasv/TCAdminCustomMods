using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TCAdmin.Interfaces.Server;
using TCAdmin.SDK.Objects;
using TCAdminCustomMods.Configurations;
using TCAdminCustomMods.Models.Mod.io.Generic;
using TCAdminCustomMods.Providers;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

namespace TCAdminCustomMods.Models.Mod.io.Games
{
    public class MGame
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("submitted_by")] public SubmittedBy SubmittedBy { get; set; }

        [JsonProperty("date_added")] public int DateAdded { get; set; }

        [JsonProperty("date_updated")] public int DateUpdated { get; set; }

        [JsonProperty("date_live")] public int DateLive { get; set; }

        [JsonProperty("presentation_option")] public int PresentationOption { get; set; }

        [JsonProperty("submission_option")] public int SubmissionOption { get; set; }

        [JsonProperty("curation_option")] public int CurationOption { get; set; }

        [JsonProperty("community_options")] public int CommunityOptions { get; set; }

        [JsonProperty("revenue_options")] public int RevenueOptions { get; set; }

        [JsonProperty("api_access_options")] public int ApiAccessOptions { get; set; }

        [JsonProperty("maturity_options")] public int MaturityOptions { get; set; }

        [JsonProperty("ugc_name")] public string UgcName { get; set; }

        [JsonProperty("icon")] public Icon Icon { get; set; }

        [JsonProperty("logo")] public Logo Logo { get; set; }

        [JsonProperty("header")] public Header Header { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("name_id")] public string NameId { get; set; }

        [JsonProperty("summary")] public string Summary { get; set; }

        [JsonProperty("instructions")] public string Instructions { get; set; }

        [JsonProperty("instructions_url")] public string InstructionsUrl { get; set; }

        [JsonProperty("profile_url")] public string ProfileUrl { get; set; }

        [JsonProperty("tag_options")] public IList<TagOption> TagOptions { get; set; }
        
        public static MGame GetGame(int gameId)
        {
            var restClient = new RestClient(ModIoBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var modIoConfiguration = ModIoBrowser.GetModIoConfigurationForCurrentGame();
            var restRequest = new RestRequest($"/v1/games/{gameId}");
            restRequest.AddQueryParameter("api_key", modIoConfiguration.ApiKey);

            var restResponse = restClient.Get<MGame>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}