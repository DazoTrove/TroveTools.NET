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
            /*
            Add(new TroveModViewModel(new TroveMod("Display No Clouds", @"%AppData%\TroveTools.NET\mods\Display No Clouds+1431713613.zip",
                "images/uploads/205.jpg", Strings.TroveMod_Status_UpToDate, true, 1431713613)));

            Add(new TroveModViewModel(new TroveMod("Neon Rezonance", @"%AppData%\TroveTools.NET\mods\Neon Rezonance+1450555472.zip",
                "images/uploads/2576.png", Strings.TroveMod_Status_NewVersionAvailable, true, 1450555472)));

            Add(new TroveModViewModel(new TroveMod("Dragon Slayer", @"%AppData%\TroveTools.NET\mods\Dragon Slayer+1431713613.zip",
                "images/uploads/1230.png", Strings.TroveMod_Status_UpToDate, false, 1433364840)));
            */

            foreach (TroveMod mod in JsonConvert.DeserializeObject<List<TroveMod>>(Resources.DesignTimeTroveMods))
            {
                MyMods.Add(new TroveModViewModel(mod));
            }
        }
    }
}
