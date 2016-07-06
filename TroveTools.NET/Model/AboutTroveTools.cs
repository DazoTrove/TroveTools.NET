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
        public const string ForumPostUrl = "http://forums.trovegame.com/showthread.php?103168-Mod-Loader-TroveTools.NET-created-by-Dazo-(for-Windows-PCs)";

        public static void LaunchFeedback()
        {
            // Launch site in default browser
            Process.Start(GitHubFeedbackUrl);
        }

        public static void LauchForumPost()
        {
            // Launch site in default browser
            Process.Start(ForumPostUrl);
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
