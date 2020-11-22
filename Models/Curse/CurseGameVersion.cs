using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace TCAdminCustomMods.Models.Curse
{
    public class CurseGameVersion
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("gameVersionId")]
        public int GameVersionId { get; set; }

        [JsonProperty("versionString")]
        public string VersionString { get; set; }

        [JsonProperty("jarDownloadUrl")]
        public string JarDownloadUrl { get; set; }

        [JsonProperty("jsonDownloadUrl")]
        public string JsonDownloadUrl { get; set; }

        [JsonProperty("approved")]
        public bool Approved { get; set; }

        [JsonProperty("dateModified")]
        public DateTime DateModified { get; set; }

        [JsonProperty("gameVersionTypeId")]
        public int GameVersionTypeId { get; set; }

        [JsonProperty("gameVersionStatus")]
        public int GameVersionStatus { get; set; }

        [JsonProperty("gameVersionTypeStatus")]
        public int GameVersionTypeStatus { get; set; }

        public static List<CurseGameVersion> GetVersions()
        {
            var restClient = new RestClient(CurseBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("/api/v2/minecraft/version");
            var restResponse = restClient.Get<List<CurseGameVersion>>(restRequest);
            return restResponse.IsSuccessful ? restResponse.Data : null;
        }
    }
}