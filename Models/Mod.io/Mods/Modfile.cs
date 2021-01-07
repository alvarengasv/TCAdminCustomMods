using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class Modfile
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mod_id")]
        public int ModId { get; set; }

        [JsonProperty("date_added")]
        public int DateAdded { get; set; }

        [JsonProperty("date_scanned")]
        public int DateScanned { get; set; }

        [JsonProperty("virus_status")]
        public int VirusStatus { get; set; }

        [JsonProperty("virus_positive")]
        public int VirusPositive { get; set; }

        [JsonProperty("virustotal_hash")]
        public string VirustotalHash { get; set; }

        [JsonProperty("filesize")]
        public int Filesize { get; set; }

        [JsonProperty("filehash")]
        public Filehash Filehash { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("metadata_blob")]
        public string MetadataBlob { get; set; }

        [JsonProperty("download")]
        public Download Download { get; set; }
    }
}