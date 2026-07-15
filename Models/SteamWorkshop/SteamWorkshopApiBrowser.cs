using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TCAdmin.Helper.Steam;

namespace TCAdminCustomMods.Models.SteamWorkshop
{
    /// <summary>
    /// Browses the Steam Workshop through the official Steam Web API
    /// (IPublishedFileService/QueryFiles) instead of the steamcommunity.com website.
    /// Produces the same WorkshopItem list as TCAdmin.Helper.Steam.WorkshopBrowser so the
    /// rest of the provider and the views work unchanged. Requires a Steam Web API key.
    /// </summary>
    public class SteamWorkshopApiBrowser
    {
        private readonly int _storeAppId;
        private readonly string _apiKey;

        public string Sort { get; set; } = "trend";
        public string Section { get; set; } = "readytouseitems";
        public string RequiredTags { get; set; } = string.Empty;
        public int TrendDays { get; set; } = 7;
        public string SearchText { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public bool ResolveAuthors { get; set; } = true;

        /// <summary>Total matching items reported by the API on the last Parse call.</summary>
        public int Total { get; private set; }

        public SteamWorkshopApiBrowser(int storeAppId, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("A Steam Web API key is required to browse the workshop with the Steam API.", "apiKey");
            }

            _storeAppId = storeAppId;
            _apiKey = apiKey;
        }

        public void LoadValuesFromQueryString()
        {
            var httpRequest = System.Web.HttpContext.Current.Request;
            var values = new NameValueCollection();
            foreach (string key in httpRequest.QueryString.Keys)
            {
                if (key != null)
                {
                    values[key] = httpRequest.QueryString[key];
                }
            }

            if (httpRequest.Form != null)
            {
                foreach (string key in httpRequest.Form.Keys)
                {
                    if (key == null)
                    {
                        continue;
                    }

                    if (key == "page")
                    {
                        values["p"] = httpRequest.Form[key];
                    }
                    else if (values[key] == null)
                    {
                        values[key] = httpRequest.Form[key];
                    }
                }
            }

            Sort = values["browsesort"] ?? "trend";
            Section = values["section"] ?? "readytouseitems";
            CurrentPage = ParseIntOrDefault(values["p"], 1);
            TrendDays = ParseIntOrDefault(values["days"], 7);
            PageSize = ParseIntOrDefault(values["num_per_page"] ?? values["numperpage"] ?? values["pageSize"], 30);
            SearchText = values["searchtext"] ?? string.Empty;

            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }

            // GetPlayerSummaries resolves at most 100 ids per call, so keep one page within that.
            if (PageSize < 1 || PageSize > 100)
            {
                PageSize = 30;
            }
        }

        public List<WorkshopItem> Parse(bool allowUnlisted, bool searchingCollections)
        {
            Total = 0;
            var items = new List<WorkshopItem>();
            var response = QueryFiles();

            if (response != null)
            {
                Total = response.Total;
                if (response.PublishedFileDetails != null)
                {
                    var players = ResolveAuthors
                        ? GetPlayers(response.PublishedFileDetails)
                        : new Dictionary<string, SteamPlayer>();

                    foreach (var publishedFile in response.PublishedFileDetails)
                    {
                        ulong fileId;
                        if (publishedFile == null || !ulong.TryParse(publishedFile.PublishedFileId, out fileId))
                        {
                            continue;
                        }

                        var item = new WorkshopItem
                        {
                            FileId = fileId,
                            Url = string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}&searchtext=", publishedFile.PublishedFileId),
                            PreviewImage = publishedFile.PreviewUrl ?? string.Empty,
                            RatingImage = GetRatingImage(publishedFile.VoteData),
                            Title = publishedFile.Title ?? string.Empty
                        };

                        SteamPlayer player;
                        if (publishedFile.Creator != null && players.TryGetValue(publishedFile.Creator, out player))
                        {
                            item.Author = player.PersonName ?? string.Empty;
                            item.AuthorUrl = BuildAuthorUrl(player);
                        }

                        item.ExtendedInfo.Description = publishedFile.ShortDescription ?? string.Empty;
                        item.ExtendedInfo.Subscriptions = (ulong)Math.Max(0, publishedFile.LifetimeSubscriptions);
                        items.Add(item);
                    }
                }
            }

            // Same semantics as WorkshopBrowser: a numeric search can match an unlisted file id.
            if (!string.IsNullOrEmpty(SearchText))
            {
                ulong searchId;
                if (ulong.TryParse(SearchText, out searchId))
                {
                    if (allowUnlisted)
                    {
                        var unlisted = GetUnlistedItem(searchId, searchingCollections);
                        if (unlisted != null)
                        {
                            if (items.Count == 0)
                            {
                                items.Add(unlisted);
                                Total = Math.Max(Total, 1);
                            }
                            else if (items[0].FileId != searchId)
                            {
                                items.Insert(0, unlisted);
                                Total++;
                            }
                        }
                    }

                    if (items.Count > 1 && items[0].FileId == searchId)
                    {
                        items.RemoveRange(1, items.Count - 1);
                        Total = 1;
                    }
                }
            }

            return items;
        }

        private WorkshopResponseData QueryFiles()
        {
            var url = new StringBuilder("https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/");
            url.Append("?key=").Append(HttpUtility.UrlEncode(_apiKey));
            url.Append("&appid=").Append(_storeAppId);
            url.Append("&query_type=").Append(GetQueryType());
            url.Append("&page=").Append(CurrentPage);
            url.Append("&numperpage=").Append(PageSize);
            if (Sort == "trend" && TrendDays > 0)
            {
                url.Append("&days=").Append(TrendDays);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                url.Append("&search_text=").Append(HttpUtility.UrlEncode(SearchText));
            }

            if (!string.IsNullOrEmpty(RequiredTags))
            {
                url.Append("&requiredtags[0]=").Append(HttpUtility.UrlEncode(RequiredTags));
            }

            url.Append("&filetype=").Append(Section == "collections" ? 1 : 0);
            url.Append("&return_vote_data=true&return_short_description=true&strip_description_bbcode=true&return_details=false");

            try
            {
                using (var wc = CreateWebClient())
                {
                    var json = wc.DownloadString(url.ToString());
                    var root = JsonConvert.DeserializeObject<WorkshopRootObject>(json);
                    return root != null ? root.Response : null;
                }
            }
            catch (WebException ex)
            {
                var httpResponse = ex.Response as HttpWebResponse;
                if (httpResponse != null && (httpResponse.StatusCode == HttpStatusCode.Forbidden || httpResponse.StatusCode == HttpStatusCode.Unauthorized))
                {
                    throw new Exception("The Steam Web API rejected the request. Verify the API key configured in Settings > Default Steam Settings.", ex);
                }

                throw;
            }
        }

        private int GetQueryType()
        {
            switch (Sort)
            {
                case "mostrecent":
                    return 1; // RankedByPublicationDate
                case "totaluniquesubscribers":
                    return 9; // RankedByTotalUniqueSubscriptions
                default:
                    // "All Time" is sent as days=-1, which QueryFiles does not accept for the
                    // trend ranking - fall back to all-time popularity (RankedByVote) instead.
                    return TrendDays > 0 ? 3 : 0;
            }
        }

        private Dictionary<string, SteamPlayer> GetPlayers(List<PublishedFileDetail> publishedFiles)
        {
            var players = new Dictionary<string, SteamPlayer>();
            var creatorIds = publishedFiles
                .Where(f => f != null && !string.IsNullOrEmpty(f.Creator))
                .Select(f => f.Creator)
                .Distinct()
                .ToList();

            if (creatorIds.Count == 0)
            {
                return players;
            }

            try
            {
                using (var wc = CreateWebClient())
                {
                    var url = string.Format("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={0}&steamids={1}",
                        HttpUtility.UrlEncode(_apiKey), string.Join(",", creatorIds));
                    var summaries = JsonConvert.DeserializeObject<SteamPlayerSummariesResponse>(wc.DownloadString(url));
                    if (summaries != null && summaries.Response != null && summaries.Response.Players != null)
                    {
                        foreach (var player in summaries.Response.Players)
                        {
                            if (!string.IsNullOrEmpty(player.SteamId) && !players.ContainsKey(player.SteamId))
                            {
                                players[player.SteamId] = player;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Author names are cosmetic - show the results without them rather than failing the browse.
            }

            return players;
        }

        private WorkshopItem GetUnlistedItem(ulong fileId, bool searchingCollections)
        {
            try
            {
                var json = GetPublishedFileDetailsJson(fileId);
                var data = JsonConvert.DeserializeObject<JObject>(json);
                var details = data == null ? null : data.SelectToken("response.publishedfiledetails") as JArray;
                var file = details != null && details.Count > 0 ? details[0] as JObject : null;
                if (file == null)
                {
                    return null;
                }

                var creatorAppId = (int?)file["creator_app_id"] ?? -1;
                var consumerAppId = (int?)file["consumer_app_id"] ?? -1;
                if (creatorAppId != _storeAppId && !(searchingCollections && consumerAppId == _storeAppId))
                {
                    return null;
                }

                return new WorkshopItem
                {
                    FileId = fileId,
                    Title = (string)file["title"] ?? "No Title",
                    PreviewImage = (string)file["preview_url"] ?? string.Empty,
                    Url = string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}", fileId)
                };
            }
            catch
            {
                // The unlisted lookup is a bonus on top of the search results - never fail the browse for it.
                return null;
            }
        }

        private string GetPublishedFileDetailsJson(ulong fileId)
        {
            using (var wc = CreateWebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var body = string.Format("key={0}&itemcount=1&publishedfileids[0]={1}", HttpUtility.UrlEncode(_apiKey), fileId);
                return Encoding.UTF8.GetString(wc.UploadData("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", Encoding.UTF8.GetBytes(body)));
            }
        }

        private string BuildAuthorUrl(SteamPlayer player)
        {
            if (string.IsNullOrEmpty(player.ProfileUrl))
            {
                return string.Empty;
            }

            return string.Format("{0}/myworkshopfiles/?appid={1}", player.ProfileUrl.TrimEnd('/'), _storeAppId);
        }

        private static string GetRatingImage(VoteData voteData)
        {
            if (voteData == null)
            {
                return string.Empty;
            }

            int rating;
            if (voteData.VotesUp + voteData.VotesDown == 0)
            {
                rating = 0; // unrated - the card template renders 0-star as empty outline stars
            }
            else if (voteData.Score < 0.2)
            {
                rating = 1;
            }
            else if (voteData.Score < 0.4)
            {
                rating = 2;
            }
            else if (voteData.Score < 0.6)
            {
                rating = 3;
            }
            else if (voteData.Score < 0.8)
            {
                rating = 4;
            }
            else
            {
                rating = 5;
            }

            return string.Format("https://community.cloudflare.steamstatic.com/public/images/sharedfiles/{0}-star.png?v=2", rating);
        }

        private static WebClient CreateWebClient()
        {
            var wc = new WebClient();
            // Steam serves UTF-8 but WebClient defaults to the system ANSI code page.
            wc.Encoding = Encoding.UTF8;
            wc.Headers["User-Agent"] = "tcadmin.com";
            return wc;
        }

        private static int ParseIntOrDefault(string value, int defaultValue)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : defaultValue;
        }
    }
}
