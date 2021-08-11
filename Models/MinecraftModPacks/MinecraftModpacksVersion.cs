using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCAdminCustomMods.Models.MinecraftModPacks
{
    public class MinecraftModpacksVersion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Updated { get; set; }
    }
}
