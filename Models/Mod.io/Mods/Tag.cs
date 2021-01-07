using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("date_added")]
        public int DateAdded { get; set; }
    }
}