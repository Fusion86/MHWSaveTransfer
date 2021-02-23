using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace MHWSaveTransfer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string logFile = Path.Combine(Path.GetTempPath(), "MHWSaveTransfer.log");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), logFile)
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Log.Information("MHWSaveTransfer v" + Assembly.GetExecutingAssembly().GetName().Version);
            Log.Information("Cirilla.Core v" + Assembly.GetAssembly(typeof(Cirilla.Core.Models.SaveData))!.GetName().Version);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                Log.Error(ex, "Unhandled exception.");
        }

        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            // Probably not needed.
            Log.CloseAndFlush();
        }
    }
}
