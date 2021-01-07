using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class Icon
    {
        [JsonProperty("filename")] public string Filename { get; set; }

        [JsonProperty("original")] public string Original { get; set; }

        [JsonProperty("thumb_64x64")] public string Thumb64X64 { get; set; }

        [JsonProperty("thumb_128x128")] public string Thumb128X128 { get; set; }

        [JsonProperty("thumb_256x256")] public string Thumb256X256 { get; set; }
    }
}