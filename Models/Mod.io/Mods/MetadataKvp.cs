using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class MetadataKvp
    {

        [JsonProperty("metakey")]
        public string Metakey { get; set; }

        [JsonProperty("metavalue")]
        public string Metavalue { get; set; }
    }
}