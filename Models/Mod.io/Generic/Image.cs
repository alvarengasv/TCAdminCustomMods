using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class Image
    {

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("thumb_320x180")]
        public string Thumb320X180 { get; set; }
    }
}