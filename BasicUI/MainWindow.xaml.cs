using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;

namespace BasicUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        Process engineProcess;
        NamedPipeClientStream pipe;
        bool isPipeConnected = false;
        public MainWindow()
        {
            InitializeComponent();
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(this);
            this.WindowBackdropType = Wpf.Ui.Controls.WindowBackdropType.Mica;
            Loaded += async delegate
            {
                try
                {
                    if (Process.GetProcessesByName("Nebula Trinity Engine").Any())
                        return;
                    if (Process.GetProcessesByName("Nebula_Trinity_Engine").Any())
                        return;

                    engineProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Nebula Trinity Engine\InstallPath.data") + @"\Nebula_Trinity_Engine.exe",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Nebula Trinity Engine\InstallPath.data")
                    });
                }
                catch { }
                try
                {
                    pipe = new NamedPipeClientStream(".", "NebulaClientPipe",
                        PipeDirection.InOut,
                        PipeOptions.Asynchronous);

                    await pipe.ConnectAsync();
                    isPipeConnected = true;

                }
                catch
                {

                }
            };
        }

        async Task Send(string msg)
        {
            try
            {
                while (!isPipeConnected)
                {
                    await Task.Delay(5);
                }
                var writer = new StreamWriter(pipe)
                {
                    AutoFlush = true
                };

                await writer.WriteLineAsync(msg);
            }
            catch (Exception ex)
            {
            }
        }

        private async void ExecuteClick(object sender, RoutedEventArgs e)
        {
            foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                await Send($"EXECUTE;{process.Id};{new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text}");
            }
        }
        private async void InjectClick(object sender, RoutedEventArgs e)
        {
            foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                await Send($"INJECT;{process.Id}");
            }
        }
        private void KillClick(object sender, RoutedEventArgs e)
        {
            foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                process.Kill();
            }
        }
    }
}
