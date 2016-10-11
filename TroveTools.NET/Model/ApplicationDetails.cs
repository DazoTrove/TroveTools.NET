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
using System.Windows;

namespace TroveTools.NET.Model
{
    static class ApplicationDetails
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        internal static bool UpdateAvailable()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    var ad = ApplicationDeployment.CurrentDeployment;
                    var info = ad.CheckForDetailedUpdate();
                    return info.UpdateAvailable;
                }
            }
            catch (Exception ex) { log.Error("Error checking for updates", ex); }
            return false;
        }

        internal static void UpdateApplication()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    var ad = ApplicationDeployment.CurrentDeployment;
                    var info = ad.CheckForDetailedUpdate();
                    if (info.UpdateAvailable)
                    {
                        ad.Update();
                        log.InfoFormat("The application will be upgraded when you quit and restart the application.");
                    }
                }
            }
            catch (Exception ex) { log.Error("Error updating application", ex); }
        }

        /// <summary>
        /// Parses the command line arguments and activation data for Trove:// mod links
        /// </summary>
        internal static string GetTroveUri()
        {
            try
            {
                Regex uriParse = new Regex(@"(?<Uri>trove:[/\\]{0,2}.+)$", RegexOptions.IgnoreCase);

                // First try parsing traditional command line args
                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    Match m = uriParse.Match(args[1]);
                    if (m.Success) return m.Groups["Uri"].Value;
                }

                // Next try parsing ClickOnce activation arguments from the SetupInformation property of the domain
                string[] activationData = AppDomain.CurrentDomain.SetupInformation?.ActivationArguments?.ActivationData;
                if (activationData != null)
                {
                    Match m = uriParse.Match(activationData[0]);
                    if (m.Success) return m.Groups["Uri"].Value;
                }
            }
            catch (Exception ex) { log.Error("Error parsing command line arguments for Trove URI", ex); }
            return string.Empty;
        }

        /// <summary>
        /// Parses the given URI for mod links (ex: trove://6;12) to return a mod ID and file ID for installation
        /// </summary>
        internal static AppArgs GetApplicationArguments(string uri)
        {
            try
            {
                if (uri == null) return null;

                Match m = Regex.Match(uri, @"trove:[/\\]{0,2}(?<ModId>\d+);(?<FileId>\d+)", RegexOptions.IgnoreCase);
                if (m.Success) return new AppArgs { LinkType = AppArgs.LinkTypes.Mod, ModId = m.Groups["ModId"].Value, FileId = m.Groups["FileId"].Value, Uri = uri };

                m = Regex.Match(uri, @"trove:[/\\]{0,2}(?<FileName>.+\.(?:zip|tmod))[/\\]?", RegexOptions.IgnoreCase);
                if (m.Success) return new AppArgs
                {
                    LinkType = AppArgs.LinkTypes.LocalMod,
                    FileName = m.Groups["FileName"].Value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                    Uri = uri
                };

                m = Regex.Match(uri, TroveModPack.IdUriRegex, RegexOptions.IgnoreCase);
                if (m.Success) return new AppArgs { LinkType = AppArgs.LinkTypes.ModPack, Uri = uri };

                m = Regex.Match(uri, TroveModPack.AdHocUriRegex, RegexOptions.IgnoreCase);
                if (m.Success) return new AppArgs { LinkType = AppArgs.LinkTypes.ModPack, Uri = uri };

                log.WarnFormat("Unknown Trove:// URI format: [{0}]", uri);
            }
            catch (Exception ex) { log.Error(string.Format("Error parsing Trove URI for mod details: [{0}]", uri), ex); }
            return null;
        }

        internal class AppArgs
        {
            public enum LinkTypes { Mod, ModPack, LocalMod }
            public LinkTypes LinkType { get; set; }
            public string ModId { get; set; }
            public string FileId { get; set; }
            public string FileName { get; set; }
            public string Uri { get; set; }
        }
    }
}
