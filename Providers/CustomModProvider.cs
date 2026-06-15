using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        static CustomModProvider()
        {
            // The mod APIs (CurseForge, Nexus, mod.io, uMod, ...) sit behind CDNs that
            // have dropped TLS 1.0/1.1. On Windows Server 2012 the system-default
            // SChannel negotiation can still attempt those protocols, producing
            // "The request was aborted: Could not create SSL/TLS secure channel".
            // Force TLS 1.2 (the strongest this OS + .NET 4.7.2 target supports)
            // process-wide. This runs once, before any provider issues a request.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

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

        public virtual DataSourceResult GetInstalledMods(Service service, [DataSourceRequest] DataSourceRequest request)
        {
            return new DataSourceResult { Data = new List<object>(), Total = 0 };
        }

        public virtual void RecordInstalled(Service service, GenericMod gameMod, string versionId)
        {
        }

        public virtual void RecordUninstalled(Service service, string modId)
        {
        }

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
            if (string.IsNullOrEmpty(slug)) return;

            var installedPlugins = GetInstalledPlugins(service);

            // Installed entries are stored either as a bare mod id ("12604") or as
            // "modId:versionId" ("12604:35407"). Uninstall is always keyed by mod id, so
            // match on the mod-id portion (before the first ':') and drop every entry for
            // that mod, whatever version was recorded. Comparing the bare mod id against a
            // version-qualified entry was the bug that left Nexus mods marked installed
            // after an uninstall.
            var targetModId = slug.Split(':')[0];
            var removed = installedPlugins.RemoveAll(p =>
                !string.IsNullOrEmpty(p) &&
                string.Equals(p.Split(':')[0], targetModId, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
            {
                SetInstalledPlugins(service, installedPlugins);
            }
        }
    }

    public enum ModSearchType
    {
        Id,
        Name,
        NameNonCaseSensitive
    }
}