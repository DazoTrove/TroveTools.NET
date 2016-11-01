using log4net;
using log4net.loggly;
using log4net.Repository.Hierarchy;
using System;
using System.Threading.Tasks;
using System.Windows;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.ViewModel;

namespace TroveTools.NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Simple partial method for updating the Loggly Appender settings such as the inputKey.
        /// This method is to be defined in the partial App class in App.xaml.secret.cs
        /// </summary>
        partial void UpdateLogglyAppenderSettings(LogglyAppender loggly);

        protected override void OnStartup(StartupEventArgs se)
        {
            log4net.Config.XmlConfigurator.Configure();
            foreach (Hierarchy hierarchy in LogManager.GetAllRepositories())
            {
                foreach (var appender in hierarchy.GetAppenders())
                {
                    var loggly = appender as LogglyAppender;
                    if (loggly != null) UpdateLogglyAppenderSettings(loggly);
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

            if (AppInstanceManager.CreateSingleInstance((s, e) => MainWindowViewModel.Instance.ProcessTroveUri(e.Data), ApplicationDetails.GetTroveUri()))
            {
                // Only run startup code for first instance
                base.OnStartup(se);
            }
            else
            {
                // Additional instance of the application: exit application
                Environment.Exit(0);
            }
        }

        private void LogUnhandledException(Exception exceptionObject, string eventName)
        {
            log.Fatal("Unhandled fatal exception: " + eventName, exceptionObject);
        }
    }
}
