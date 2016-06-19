using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class TrovesaurusViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand _LaunchTrovesaurusCommand;

        public TrovesaurusViewModel()
        {
            DisplayName = Strings.TrovesaurusViewModel_DisplayName;
        }

        public DelegateCommand LaunchTrovesaurusCommand
        {
            get
            {
                if (_LaunchTrovesaurusCommand == null) _LaunchTrovesaurusCommand = new DelegateCommand(LaunchTrovesaurus);
                return _LaunchTrovesaurusCommand;
            }
        }

        private void LaunchTrovesaurus(object param = null)
        {
            try
            {
                TrovesaurusApi.LaunchTrovesaurus();
            }
            catch (Exception ex)
            {
                log.Error("Error launching Trovesaurus", ex);
            }
        }
    }
}
