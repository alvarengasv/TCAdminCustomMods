using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class Avatar
    {
        [JsonProperty("filename")] public string Filename { get; set; }

        [JsonProperty("original")] public string Original { get; set; }

        [JsonProperty("thumb_50x50")] public string Thumb50X50 { get; set; }

        [JsonProperty("thumb_100x100")] public string Thumb100X100 { get; set; }
    }
}