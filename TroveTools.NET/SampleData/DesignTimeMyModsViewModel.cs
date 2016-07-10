using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Properties;
using TroveTools.NET.Model;
using TroveTools.NET.ViewModel;
using Newtonsoft.Json;

namespace TroveTools.NET.SampleData
{
    class DesignTimeMyModsViewModel : MyModsViewModel
    {
        public DesignTimeMyModsViewModel() : base()
        {
            TroveModPackViewModel pack = new TroveModPackViewModel(new TroveModPack { Name = "Design Time Pack", Author = "Dazo" });

            foreach (TroveMod mod in JsonConvert.DeserializeObject<List<TroveMod>>(Resources.DesignTimeTroveMods))
            {
                MyMods.Add(new TroveModViewModel(mod));
                pack.DataObject.Mods.Add(mod);
            }

            ModPacks.Add(pack);
        }
    }
}
