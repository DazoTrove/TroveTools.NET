using CommonMark;
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
        private DelegateCommand<string> _LaunchFeedbackCommand;

        public AboutViewModel()
        {
            DisplayName = Strings.AboutViewModel_DisplayName;
        }

        public DelegateCommand<string> LaunchFeedbackCommand
        {
            get
            {
                if (_LaunchFeedbackCommand == null) _LaunchFeedbackCommand = new DelegateCommand<string>(LaunchFeedback);
                return _LaunchFeedbackCommand;
            }
        }

        private void LaunchFeedback(string param = null)
        {
            try
            {
                if (param == "Forum")
                    AboutTroveTools.LauchForumPost();
                else
                    AboutTroveTools.LaunchFeedback();
            }
            catch (Exception ex) { log.Error("Error launching feeback", ex); }
        }

        public string VersionHistory
        {
            get
            {
                try { return CommonMarkConverter.Convert(AboutTroveTools.VersionHistory); }
                catch (Exception ex) { log.Error("Error converting version history from Markdown to HTML", ex); }
                return null;
            }
        }

        public string Stylesheet
        {
            get { return AboutTroveTools.Stylesheet; }
        }
    }
}
