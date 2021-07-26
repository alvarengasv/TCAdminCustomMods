using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.UMod
{
    public class DetailedModData : GenericMod
    {
        public override string Id { get; set; } = "-1";
        [JsonProperty("name")] public override string Name { get; set; }

        [JsonProperty("category")] public int Category { get; set; }

        [JsonProperty("license")] public string License { get; set; }

        [JsonProperty("license_url")] public object LicenseUrl { get; set; }

        [JsonProperty("icon_url")] public string IconUrl { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("slug")] public string Slug { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("downloads")] public int Downloads { get; set; }

        [JsonProperty("rating")] public string Rating { get; set; }

        [JsonProperty("created_at")] public string CreatedAt { get; set; }

        [JsonProperty("published_at")] public string PublishedAt { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("json_url")] public string JsonUrl { get; set; }
        [JsonProperty("download_url")] public string DownloadUrl { get; set; }

        [JsonProperty("status_detail")] public StatusDetail StatusDetail { get; set; }

        [JsonProperty("downloads_shortened")] public string DownloadsShortened { get; set; }

        [JsonProperty("games_detail")] public IList<GamesDetail> GamesDetail { get; set; }

        [JsonProperty("donate_url")] public string DonateUrl { get; set; }

        [JsonProperty("latest_release_version")]
        public string LatestReleaseVersion { get; set; }

        [JsonProperty("latest_release_version_formatted")]
        public string LatestReleaseVersionFormatted { get; set; }

        [JsonProperty("latest_release_version_checksum")]
        public string LatestReleaseVersionChecksum { get; set; }

        [JsonProperty("latest_release_at")] public string LatestReleaseAt { get; set; }

        [JsonProperty("latest_release_at_atom")]
        public DateTime LatestReleaseAtAtom { get; set; }

        [JsonProperty("author")] public string Author { get; set; }

        [JsonProperty("author_icon_url")] public string AuthorIconUrl { get; set; }

        [JsonProperty("author_id")] public string AuthorId { get; set; }

        [JsonProperty("tags_all")] public string TagsAll { get; set; }

        [JsonProperty("created_at_atom")] public DateTime CreatedAtAtom { get; set; }

        [JsonProperty("updated_at_atom")] public DateTime UpdatedAtAtom { get; set; }

        [JsonProperty("watchers")] public int Watchers { get; set; }

        [JsonProperty("watchers_shortened")] public string WatchersShortened { get; set; }

        [JsonProperty("badges")] public string Badges { get; set; }

        public static DetailedModData GetDetailedModData(string id)
        {
            var restClient = new RestClient(UModBrowser.BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest($"/plugins/{id}.json");
            //Console.WriteLine($"/plugins/{id}.json");
            var restResponse = restClient.Get<DetailedModData>(restRequest);
            if (!restResponse.IsSuccessful) return null;
            restResponse.Data.Id = restResponse.Data.Name;
            return restResponse.Data;

        }
    }
}