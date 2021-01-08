using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io
{
    public class MListBase
    {
        [JsonProperty("result_count")] public int ResultCount { get; set; }

        [JsonProperty("result_offset")] public int ResultOffset { get; set; }

        [JsonProperty("result_limit")] public int ResultLimit { get; set; }

        [JsonProperty("result_total")] public int ResultTotal { get; set; }
    }
}