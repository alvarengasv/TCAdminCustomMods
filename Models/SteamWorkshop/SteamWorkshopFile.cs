using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCAdminCustomMods.Models.Generic;

namespace TCAdminCustomMods.Models.SteamWorkshop
{
    public class SteamWorkshopFile : GenericMod
    {
        public override string Id { get; set; }
        public override string Name { get; set; }
        public bool IsCollection { get; set; }
        public dynamic FileData { get; set; }
    }
}
