using Newtonsoft.Json;

namespace TCAdminCustomMods.Models.Mod.io.Mods
{
    public class Stats
    {

        [JsonProperty("mod_id")]
        public int ModId { get; set; }

        [JsonProperty("popularity_rank_position")]
        public int PopularityRankPosition { get; set; }

        [JsonProperty("popularity_rank_total_mods")]
        public int PopularityRankTotalMods { get; set; }

        [JsonProperty("downloads_total")]
        public int DownloadsTotal { get; set; }

        [JsonProperty("subscribers_total")]
        public int SubscribersTotal { get; set; }

        [JsonProperty("ratings_total")]
        public int RatingsTotal { get; set; }

        [JsonProperty("ratings_positive")]
        public int RatingsPositive { get; set; }

        [JsonProperty("ratings_negative")]
        public int RatingsNegative { get; set; }

        [JsonProperty("ratings_percentage_positive")]
        public int RatingsPercentagePositive { get; set; }

        [JsonProperty("ratings_weighted_aggregate")]
        public double RatingsWeightedAggregate { get; set; }

        [JsonProperty("ratings_display_text")]
        public string RatingsDisplayText { get; set; }

        [JsonProperty("date_expires")]
        public int DateExpires { get; set; }
    }
}