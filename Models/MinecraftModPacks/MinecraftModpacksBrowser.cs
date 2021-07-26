using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.MinecraftModPacks
{
    public class MinecraftModpacksBrowser : GenericMod
    {
        private class ModPackSearchResult
        {
            public int[] Packs { get; set; }
            public int Total { get; set; }
            public int Limit { get; set; }
            public long Refreshed { get; set; }

            public static ModPackSearchResult Search(string sortBy = "updated", string query = "")
            {
                var restClient = new RestClient(BaseUrl);
                restClient.UseNewtonsoftJson();
                var restRequest = new RestRequest(sortBy + "/5000");
                if (!string.IsNullOrEmpty(query))
                {
                    restRequest.AddQueryParameter("term", query);
                }

                //Console.WriteLine("Search URL: " + restClient.BuildUri(restRequest));

                var restResponse = restClient.Get<ModPackSearchResult>(restRequest);
                //Console.WriteLine("restResponse: {0}", restResponse.Data.packs.Length);

                if (restResponse.IsSuccessful)
                {
                    //Console.WriteLine("Search returned count: {0}", restResponse.Data.Packs.Length);
                    return restResponse.Data;
                }

                return null;
            }
        }

        public const string BaseUrl = "https://api.modpacks.ch/public/modpack/";

        public override string Id { get; set; }
        public override string Name { get; set; }
        public string Synopsis { get; set; }
        public string Description { get; set; }
        public long Installs { get; set; }
        public long Plays { get; set; }
        public bool Featured { get; set; }
        public long Refreshed { get; set; }
        public string Notification { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public long Updated { get; set; }
        public MinecraftModpacksArt[] Art { get; set; }
        public MinecraftModpacksVersion[] Versions {get;set;}
        public static MinecraftModpacksBrowser GetPack(int packId)
        {
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest(packId.ToString());

            //Console.WriteLine("URL: " + restClient.BuildUri(restRequest));

            var restResponse = restClient.Get<MinecraftModpacksBrowser>(restRequest);
            //Console.WriteLine("restResponse: {0}", restResponse.Data.packs.Length);

            if (restResponse.IsSuccessful && restResponse.Data.Status == "success" && !string.IsNullOrEmpty(restResponse.Data.Name))
            {
                return restResponse.Data;
            }

            //Console.WriteLine("Failure pack id: {0}", packId);

            return new MinecraftModpacksBrowser()
            {
                Id = packId.ToString(),
                Name = "Unknown Modpack",
                Status="error"
            };

        }

        public static List<MinecraftModpacksBrowser> Search(string sortBy= "updated", string query = "", int page = 1, int pageSize = 10)
        {
            var search = ModPackSearchResult.Search(sortBy, query);

            if (search == null)
            {
                return null;
            }

            var modinfos = new List<MinecraftModpacksBrowser>();
            var skip = (page - 1) * pageSize;
            var currentpacks = search.Packs.Skip(skip).Take(pageSize).ToArray();

            var listOfActions = new List<Action>();
            var lockobj = new object();
            currentpacks.ToList().ForEach(p => listOfActions.Add(() => {
                var modpack = MinecraftModpacksBrowser.GetPack(p);
                lock (lockobj)
                {
                    modinfos.Add(modpack);
                }
            }));

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
            Parallel.Invoke(options, listOfActions.ToArray());

            //Items could have wrong order. Order them again.
            var orderedmodinfos = new List<MinecraftModpacksBrowser>();
            for (int i = 0; i < currentpacks.Length; i++)
            {
                var packid = currentpacks[i].ToString();
                orderedmodinfos.Add(modinfos.SingleOrDefault(m => m.Id == packid));
            }
            return orderedmodinfos;
        }

        public static string[] GetTags()
        {
            var restClient = new RestClient(BaseUrl);
            restClient.UseNewtonsoftJson();
            var restRequest = new RestRequest("https://api.modpacks.ch/public/tag/popular/100");

            var restResponse = restClient.Get<MinecraftModpacksTags>(restRequest);

            if (restResponse.IsSuccessful)
            {
                return restResponse.Data.Tags;
            }

            return new string[] { };
        }
    }
}
