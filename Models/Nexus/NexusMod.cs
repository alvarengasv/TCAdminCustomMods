using Newtonsoft.Json;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.Nexus
{
    public class NexusMod : GenericMod
    {
        public override string Id { get; set; } = "-1";

        [JsonProperty("mod_id")] public int ModId { get; set; }

        [JsonProperty("name")] public override string Name { get; set; }

        [JsonProperty("summary")] public string Summary { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("picture_url")] public string PictureUrl { get; set; }

        [JsonProperty("mod_downloads")] public long ModDownloads { get; set; }

        [JsonProperty("mod_unique_downloads")] public long ModUniqueDownloads { get; set; }

        [JsonProperty("endorsement_count")] public long EndorsementCount { get; set; }

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("author")] public string Author { get; set; }

        [JsonProperty("uploaded_by")] public string UploadedBy { get; set; }

        [JsonProperty("uploaded_users_profile_url")] public string UploadedUsersProfileUrl { get; set; }

        [JsonProperty("domain_name")] public string DomainName { get; set; }

        [JsonProperty("game_id")] public int GameId { get; set; }

        [JsonProperty("category_id")] public int CategoryId { get; set; }

        public string Category { get; set; }

        [JsonProperty("contains_adult_content")] public bool ContainsAdultContent { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        [JsonProperty("available")] public bool Available { get; set; }

        [JsonProperty("created_timestamp")] public long CreatedTimestamp { get; set; }

        [JsonProperty("updated_timestamp")] public long UpdatedTimestamp { get; set; }

        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(DomainName) || ModId <= 0) return null;
                return string.Format("https://www.nexusmods.com/{0}/mods/{1}", DomainName, ModId);
            }
        }
    }
}
