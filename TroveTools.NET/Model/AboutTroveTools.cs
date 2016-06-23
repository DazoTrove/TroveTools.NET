using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Properties;

namespace TroveTools.NET.Model
{
    static class AboutTroveTools
    {
        public const string GitHubFeedbackUrl = "https://github.com/DazoTrove/TroveTools.NET/issues";

        public static void LaunchFeedback()
        {
            // Launch site in default browser
            Process.Start(GitHubFeedbackUrl);
        }

        public static string VersionHistory
        {
            get { return Resources.VersionHistory; }
        }

        public static string Stylesheet
        {
            get { return Resources.Stylesheet; }
        }
    }
}
