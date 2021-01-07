using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class Logo
    {
        [JsonProperty("filename")] public string Filename { get; set; }

        [JsonProperty("original")] public string Original { get; set; }

        [JsonProperty("thumb_320x180")] public string Thumb320x180 { get; set; }

        [JsonProperty("thumb_640x360")] public string Thumb640x360 { get; set; }

        [JsonProperty("thumb_1280x720")] public string Thumb1280x720 { get; set; }
    }
}