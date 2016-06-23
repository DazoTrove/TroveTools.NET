using log4net;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TroveTools.NET.Model
{
    static class ApplicationDetails
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetCurrentVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed) return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static string LaunchPath
        {
            get
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var company = entryAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute)).OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company;
                var product = entryAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute)).OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product;

                if (company != null && product != null)
                {
                    string appShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), company, product + ".appref-ms");
                    if (File.Exists(appShortcut)) return appShortcut;

                    appShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), product + ".appref-ms");
                    if (File.Exists(appShortcut)) return appShortcut;
                }

                return AssemblyLocation;
            }
        }

        public static string AssemblyLocation
        {
            get { return Assembly.GetEntryAssembly().Location; }
        }

        /// <summary>
        /// Parses command line arguments for mod links (ex: trove://6;12) to return a mod ID and file ID for installation
        /// </summary>
        internal static AppArgs GetApplicationArguments()
        {
            // First try parsing traditional command line args
            Regex modParse = new Regex(@"trove:[/\\]{0,2}(?<ModId>\d+);(?<FileId>\d+)", RegexOptions.IgnoreCase);

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                Match m = modParse.Match(args[1]);
                if (m.Success)
                    return new AppArgs() { ModId = m.Groups["ModId"].Value, FileId = m.Groups["FileId"].Value };
                else
                    log.WarnFormat("Unknown command line argument: [{0}]", args[1]);
            }

            // Next try parsing ClickOnce activation arguments from the SetupInformation property of the domain
            string[] activationData = AppDomain.CurrentDomain.SetupInformation?.ActivationArguments?.ActivationData;
            if (activationData != null)
            {
                Match m = modParse.Match(activationData[0]);
                if (m.Success) return new AppArgs() { ModId = m.Groups["ModId"].Value, FileId = m.Groups["FileId"].Value };
            }

            return null;
        }

        internal class AppArgs
        {
            public string ModId { get; set; }
            public string FileId { get; set; }
        }
    }
}
