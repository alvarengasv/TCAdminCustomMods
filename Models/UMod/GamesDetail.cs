using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.UMod
{
    public class GamesDetail
    {
        [JsonProperty("icon_url")] public string IconUrl { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("slug")] public string Slug { get; set; }
    }
}