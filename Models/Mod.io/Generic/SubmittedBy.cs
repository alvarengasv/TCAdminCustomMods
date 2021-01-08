using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class SubmittedBy
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name_id")] public string NameId { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("date_online")] public int DateOnline { get; set; }

        [JsonProperty("avatar")] public Avatar Avatar { get; set; }

        [JsonProperty("timezone")] public string Timezone { get; set; }

        [JsonProperty("language")] public string Language { get; set; }

        [JsonProperty("profile_url")] public string ProfileUrl { get; set; }
    }
}