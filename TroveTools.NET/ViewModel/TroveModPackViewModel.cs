using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Model;

namespace TroveTools.NET.ViewModel
{
    class TroveModPackViewModel : ViewModelBase<TroveModPack>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TroveModPackViewModel(TroveModPack dataObject) : base(dataObject)
        {
            if (dataObject != null && dataObject.Mods != null)
            {
                var mods = MainWindowViewModel.Instance.MyMods.MyMods;
                foreach (var mod in dataObject.Mods)
                {
                    // Only set mod pack grouping for mods saved with the pack name (saved or loaded from a pack previously)
                    var modVm = mods.FirstOrDefault(m => m.DataObject.Id == mod.Id && m.DataObject.PackName == dataObject.Name);
                    if (modVm != null)
                    {
                        modVm.ModPack = this;
                        modVm.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == "Enabled") RaisePropertyChanged("ModsEnabled");
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Three state boolean to represent whether mods in this pack are enabled: true if all mods are enabled,
        /// false if all mods are disabled, and null if only some are enabled
        /// </summary>
        public bool? ModsEnabled
        {
            get
            {
                bool? enabled = null;
                var mods = MainWindowViewModel.Instance.MyMods.MyMods;
                if (DataObject.Mods.All(m => IsModEnabled(m.Id, mods))) enabled = true;
                if (DataObject.Mods.All(m => !IsModEnabled(m.Id, mods))) enabled = false;
                return enabled;
            }
            set
            {
                bool? oldValue = ModsEnabled;
                bool? newValue = value;

                // Do not set to null: instead toggle based on previous value
                if (!newValue.HasValue)
                {
                    if (oldValue.HasValue)
                        newValue = !oldValue.Value;
                    else
                        newValue = true;
                }

                log.InfoFormat("Setting Mods Enabled to [{0}]", newValue);
                var mods = MainWindowViewModel.Instance.MyMods.MyMods;
                foreach (var mod in DataObject.Mods)
                {
                    dynamic modVm = GetMod(mod.Id, mods);
                    if (modVm != null) modVm.Enabled = newValue;
                }
            }
        }

        public override string DisplayName
        {
            get { return DataObject.Name; }
        }

        public override string ToString()
        {
            return DataObject.Name;
        }

        private static bool IsModEnabled(string id, ObservableCollection<TroveModViewModel> mods)
        {
            return mods.Any(m => m.DataObject.Id == id && m.DataObject.Enabled);
        }

        private static TroveModViewModel GetMod(string id, ObservableCollection<TroveModViewModel> mods)
        {
            return mods.FirstOrDefault(m => m.DataObject.Id == id);
        }
    }
}
