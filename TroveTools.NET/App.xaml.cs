using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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

        protected override void OnStartup(StartupEventArgs se)
        {
            log4net.Config.XmlConfigurator.Configure();

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
