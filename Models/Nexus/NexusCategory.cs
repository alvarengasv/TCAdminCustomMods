using System.Collections.Generic;
using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Nexus
{
    public class NexusCategory
    {
        [JsonProperty("category_id")] public int Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("parent_category")] public object ParentCategory { get; set; }
    }

    public class NexusSearchResult
    {
        public List<NexusMod> Mods { get; set; } = new List<NexusMod>();
        public int TotalCount { get; set; }
    }
}
