using System.Collections.Generic;
using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Generic
{
    public class Media
    {

        [JsonProperty("youtube")]
        public IList<string> Youtube { get; set; }

        [JsonProperty("sketchfab")]
        public IList<string> Sketchfab { get; set; }

        [JsonProperty("images")]
        public IList<Image> Images { get; set; }
    }
}