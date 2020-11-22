using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Curse
{
    public class Author
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("projectId")] public int ProjectId { get; set; }

        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("projectTitleId")] public int ProjectTitleId { get; set; }

        [JsonProperty("projectTitleTitle")] public string ProjectTitleTitle { get; set; }

        [JsonProperty("userId")] public int UserId { get; set; }

        [JsonProperty("twitchId")] public int TwitchId { get; set; }
    }
}