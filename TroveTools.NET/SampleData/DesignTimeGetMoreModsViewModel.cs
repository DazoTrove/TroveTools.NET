using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;
using TroveTools.NET.ViewModel;

namespace TroveTools.NET.SampleData
{
    class DesignTimeGetMoreModsViewModel : GetMoreModsViewModel
    {
        public DesignTimeGetMoreModsViewModel() : base()
        {
            foreach (TroveMod mod in JsonConvert.DeserializeObject<List<TroveMod>>(Resources.DesignTimeTroveMods))
            {
                TrovesaurusMods.Add(new TroveModViewModel(mod));
            }
            BuildTypesCollections();
        }
    }
}
