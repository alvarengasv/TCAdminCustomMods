using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Alexr03.Common.TCAdmin.Extensions;
using Alexr03.Common.TCAdmin.Permissions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using TCAdmin.SDK.Objects;
using TCAdminCustomMods.Models.Generic;
using TCAdminCustomMods.Models.Mod.io.Mods;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

namespace TCAdminCustomMods.Providers
{
    public abstract class CustomModProvider
    {
        public virtual void PreInstallMod(Service service, GenericMod gameMod)
        {
        }
        
        public virtual void PostInstallMod(Service service, GenericMod gameMod)
        {
        }
        
        public virtual void PreUninstallMod(Service service, GenericMod gameMod)
        {
        }
        
        public virtual void PostUninstallMod(Service service, GenericMod gameMod)
        {
        }
        
        public abstract bool InstallMod(Service service, GenericMod gameMod);
        public abstract bool InstallMod(Service service, GenericMod gameMod, string versionId);
        public abstract int InstallModWithTask(Service service, GenericMod gameMod);

        public abstract bool UnInstallMod(Service service, GenericMod gameMod);
        public abstract int UninstallModWithTask(Service service, GenericMod gameMod);

        public abstract DataSourceResult GetMods([DataSourceRequest] DataSourceRequest request);

        public abstract GenericMod GetMod(string s, ModSearchType modSearchType);

        public List<string> GetInstalledPlugins(Service service)
        {
            var hasValueAndSet = service.Variables.HasValueAndSet("CM:InstalledPlugins");
            if (!hasValueAndSet) return new List<string>();
            
            var strings = service.Variables["CM:InstalledPlugins"].ToString().Split(',');
            return strings.ToList();
        }
        
        private void SetInstalledPlugins(Service service, IEnumerable<string> plugins)
        {
            using (var securityBypass = new SecurityBypass(service))
            {
                service.Variables["CM:InstalledPlugins"] = string.Join(",", plugins);
                service.Save();
            }
        }
        
        public void AddInstalledPlugin(Service service, string slug)
        {
            var installedPlugins = GetInstalledPlugins(service);
            installedPlugins.Add(slug);
            SetInstalledPlugins(service, installedPlugins);
        }
        
        public void RemoveInstalledPlugin(Service service, string slug)
        {
            var installedPlugins = GetInstalledPlugins(service);
            installedPlugins.Remove(slug);
            if (slug.IndexOf(":") != -1)
            {
                var slugstart = slug.Split(':')[0];
                for (int i = installedPlugins.Count - 1; i >= 0; i--)
                {
                    if (installedPlugins[i].StartsWith(slugstart))
                    {
                        installedPlugins.RemoveAt(i);
                    }
                }
            }
            SetInstalledPlugins(service, installedPlugins);
        }
    }

    public enum ModSearchType
    {
        Id,
        Name,
        NameNonCaseSensitive
    }
}