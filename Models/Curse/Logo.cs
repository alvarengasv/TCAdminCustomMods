using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Curse
{
    public class Logo
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("modId")] public int ProjectId { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("thumbnailUrl")] public string ThumbnailUrl { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("url")] public string Url { get; set; }
    }
}