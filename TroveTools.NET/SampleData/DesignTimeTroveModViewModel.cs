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
    class DesignTimeTroveModViewModel : TroveModViewModel
    {
        public DesignTimeTroveModViewModel() : base(JsonConvert.DeserializeObject<TroveMod>(Resources.DesignTimeTroveMod))
        {
        }
    }
}
