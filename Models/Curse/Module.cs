using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Curse
{
    public class Module
    {
        [JsonProperty("foldername")] public string Foldername { get; set; }

        [JsonProperty("fingerprint")] public string Fingerprint { get; set; }

        [JsonProperty("type")] public int Type { get; set; }
    }
}