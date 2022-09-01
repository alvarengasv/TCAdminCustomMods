using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Curse
{
    public class Links
    {
        [JsonProperty("websiteUrl")] public string WebsiteUrl { get; set; }

        [JsonProperty("wikiUrl")] public string WikiUrl { get; set; }

        [JsonProperty("issuesUrl")] public string IssuesUrl { get; set; }

        [JsonProperty("sourceUrl")] public string SourceUrl { get; set; }
    }
}
