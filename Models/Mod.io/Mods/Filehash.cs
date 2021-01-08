using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class Filehash
    {

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }
}