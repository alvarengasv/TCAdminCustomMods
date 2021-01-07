using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class Download
    {

        [JsonProperty("binary_url")]
        public string BinaryUrl { get; set; }

        [JsonProperty("date_expires")]
        public int DateExpires { get; set; }
    }
}