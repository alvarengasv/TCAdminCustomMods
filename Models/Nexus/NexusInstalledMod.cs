using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Nexus
{
    public class NexusInstalledMod
    {
        [JsonProperty("mod_id")] public string ModId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("summary")] public string Summary { get; set; }

        [JsonProperty("picture_url")] public string PictureUrl { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("domain_name")] public string DomainName { get; set; }

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("author")] public string Author { get; set; }

        [JsonProperty("mod_downloads")] public long ModDownloads { get; set; }

        [JsonProperty("file_id")] public int FileId { get; set; }

        [JsonProperty("installed_files")] public List<string> InstalledFiles { get; set; } = new List<string>();

        [JsonProperty("installed_at")] public DateTime InstalledAt { get; set; }

        public static NexusInstalledMod FromMod(NexusMod mod, int fileId, List<string> installedFiles)
        {
            return new NexusInstalledMod
            {
                ModId = mod.ModId.ToString(),
                Name = mod.Name,
                Summary = mod.Summary,
                PictureUrl = mod.PictureUrl,
                Url = mod.Url,
                DomainName = mod.DomainName,
                Version = mod.Version,
                Author = mod.Author,
                ModDownloads = mod.ModDownloads,
                FileId = fileId,
                InstalledFiles = installedFiles ?? new List<string>(),
                InstalledAt = DateTime.UtcNow
            };
        }
    }
}
