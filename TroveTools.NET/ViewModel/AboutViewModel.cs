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
    class AboutViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand _LaunchFeedbackCommand;

        public AboutViewModel()
        {
            DisplayName = Strings.AboutViewModel_DisplayName;
        }

        public DelegateCommand LaunchFeedbackCommand
        {
            get
            {
                if (_LaunchFeedbackCommand == null) _LaunchFeedbackCommand = new DelegateCommand(LaunchFeedback);
                return _LaunchFeedbackCommand;
            }
        }

        private void LaunchFeedback(object param = null)
        {
            try
            {
                AboutTroveTools.LaunchFeedback();
            }
            catch (Exception ex)
            {
                log.Error("Error launching feeback", ex);
            }
        }

        public string VersionHistory
        {
            get { return AboutTroveTools.VersionHistory; }
        }
    }
}
