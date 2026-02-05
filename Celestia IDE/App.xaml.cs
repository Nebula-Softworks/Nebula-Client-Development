using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using CefSharp;
using CefSharp.Wpf;

namespace Celestia_IDE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var settings = new CefSettings
            {
                LogFile = "ceflog.log",
                LogSeverity = LogSeverity.Verbose,
                BackgroundColor = Cef.ColorSetARGB(0, 0, 0, 0),
                CachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE\monaco_cache"
            };
            settings.CefCommandLineArgs["force-device-scale-factor"] = "1";
            settings.CefCommandLineArgs["high-dpi-support"] = "1";
            settings.CefCommandLineArgs["do-not-de-elevate"] = "1";

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            RenderOptions.ProcessRenderMode = RenderMode.Default;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var others = Process.GetProcessesByName("Celestia IDE");

            if (others.Length <= 1)
            {
                foreach (var p in Process.GetProcessesByName("Nebula Trinity Engine"))
                    p.Kill();
                foreach (var p in Process.GetProcessesByName("Nebula_Trinity_Engine"))
                    p.Kill();
                
            }

            Cef.Shutdown();
            base.OnExit(e);
        }

    }
}
