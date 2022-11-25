using System.Collections.Generic;
using System.Linq;
using Alexr03.Common.Misc.Strings;
using Alexr03.Common.TCAdmin.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TCAdmin.Interfaces.Database;

namespace TCAdminCustomMods.Providers
{
    public class CustomModBase : DynamicTypeBase
    {
        public CustomModBase() : base("tcmodule_custom_mod_providers")
        {
        }
        
        public CustomModBase(int id) : this()
        {
            this.SetValue("id", id);
            this.ValidateKeys();
            if (!this.Find())
            {
                throw new KeyNotFoundException("Cannot find Custom Mod Provider with ID: " + id);
            }
        }
        
        public string Name => this.GetStringValue("name");

        public string ViewName => this.GetStringValue("view");

        public JObject GetConfigurationForGame(TCAdmin.GameHosting.SDK.Objects.Game game)
        {
            var value = game.AppData.GetValue($"__CustomModsModule::{this.Name.ReplaceWhitespace()}_Config", this.Id == 4 ? $"{{\"Enabled\":{game.Steam.WorkshopEnabled.ToString().ToLower()}, \"CustomName\":\"Steam Workshop\"}}" : "{}").ToString();
            return JsonConvert.DeserializeObject<JObject>(value);
        }
        
        public bool SetConfigurationForGame(TCAdmin.GameHosting.SDK.Objects.Game game, object obj)
        {
            game.AppData[$"__CustomModsModule::{this.Name.ReplaceWhitespace()}_Config"] = JsonConvert.SerializeObject(obj);
            return game.Save();
        }

        public static List<CustomModBase> GetCustomModBases()
        {
            return new CustomModBase().GetObjectList(new WhereList()).Cast<CustomModBase>().ToList();
        }
    }
}