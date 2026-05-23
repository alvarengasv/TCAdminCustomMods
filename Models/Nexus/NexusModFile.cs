using System.Collections.Generic;
using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Nexus
{
    public class NexusModFile
    {
        [JsonProperty("id")] public List<int> Id { get; set; }

        [JsonProperty("uid")] public long Uid { get; set; }

        [JsonProperty("file_id")] public int FileId { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("category_id")] public int CategoryId { get; set; }

        [JsonProperty("category_name")] public string CategoryName { get; set; }

        [JsonProperty("is_primary")] public bool IsPrimary { get; set; }

        [JsonProperty("size")] public long Size { get; set; }

        [JsonProperty("file_name")] public string FileName { get; set; }

        [JsonProperty("uploaded_timestamp")] public long UploadedTimestamp { get; set; }

        [JsonProperty("uploaded_time")] public string UploadedTime { get; set; }

        [JsonProperty("mod_version")] public string ModVersion { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("size_kb")] public long SizeKb { get; set; }

        [JsonProperty("size_in_bytes")] public long SizeInBytes { get; set; }

        [JsonProperty("changelog_html")] public string ChangelogHtml { get; set; }

        [JsonProperty("content_preview_link")] public string ContentPreviewLink { get; set; }
    }

    public class NexusFilesResponse
    {
        [JsonProperty("files")] public List<NexusModFile> Files { get; set; }
    }

    public class NexusDownloadLink
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("short_name")] public string ShortName { get; set; }

        [JsonProperty("URI")] public string Uri { get; set; }
    }

    public class NexusContentPreviewNode
    {
        [JsonProperty("path")] public string Path { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("size")] public string Size { get; set; }

        [JsonProperty("children")] public List<NexusContentPreviewNode> Children { get; set; }
    }
}
