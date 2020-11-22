using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.UMod
{
    public class StatusDetail
    {
        [JsonProperty("icon")] public string Icon { get; set; }

        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("value")] public int Value { get; set; }

        [JsonProperty("class")] public string Class { get; set; }
    }
}