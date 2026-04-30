using System;
using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.UMod
{
    public class UModInstalledMod
    {
        [JsonProperty("slug")] public string Slug { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("icon_url")] public string IconUrl { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("downloads")] public int Downloads { get; set; }
        [JsonProperty("installed_at")] public DateTime InstalledAt { get; set; }

        public static UModInstalledMod FromDetailedModData(DetailedModData mod)
        {
            return new UModInstalledMod
            {
                Slug = mod.Slug,
                Title = mod.Title,
                Description = mod.Description,
                IconUrl = mod.IconUrl,
                Url = mod.Url,
                Downloads = mod.Downloads,
                InstalledAt = DateTime.UtcNow
            };
        }
    }
}
