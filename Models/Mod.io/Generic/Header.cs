using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class Header
    {
        [JsonProperty("filename")] public string Filename { get; set; }

        [JsonProperty("original")] public string Original { get; set; }
    }
}