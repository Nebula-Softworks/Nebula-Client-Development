using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Input;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using WK.Libraries.BetterFolderBrowserNS;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38; private enum DWM_SYSTEMBACKDROP_TYPE { DWMSBT_AUTO = 0, DWMSBT_NONE = 1, DWMSBT_MAINWINDOW = 2, DWMSBT_TRANSIENTWINDOW = 3, DWMSBT_TABBEDWINDOW = 4 }
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
        private void blurdisable(object sender, RoutedEventArgs e)
        { var hwnd = new WindowInteropHelper(this).Handle; int backdrop = (int)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_NONE; DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int)); }

        [DllImport("Shell32.dll")]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        #region Functions And Prerequisites
        WebClient downloadhandler = new WebClient();

        static string Website = "https://nebulasoftworks.xyz/nebulaclient";
        /// <summary>
        /// Returns the link of the raw EULA of the provided product
        /// </summary>
        /// <param name="product">The Provided Product to get the EULA from</param>
        /// <returns>String, The link of the EULA file</returns>
        static string EULALink(string product)
        {
            return ($"https://raw.nebulasoftworks.xyz/EULAs/{product}.eula");
        }
        static string GithubRepo = "https://github.com/Nebula-Softworks/Nebula-Client-Development";
        static string DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data";

        /// <summary>
        /// Returns the content of a provided webpage
        /// </summary>
        /// <param name="weburl">The link of the webpage to fetch from</param>
        /// <returns>String, The content of the page</returns>
        public string HttpGet(string weburl)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                return webClient.DownloadString(weburl);
            }
        }

        public async void SetFolderIcon(string folderPath)
        {
            using var client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync("https://raw.githubusercontent.com/Nebula-Softworks/Nebula-Client-Development/refs/heads/master/Assets/Graphics/Nebula%20Client%20Logo.ico");
            File.WriteAllBytes($"{folderPath}\\icofile.ico", data);


            Directory.CreateDirectory(folderPath);

            try
            {
                string desktopIni = Path.Combine(folderPath, "desktop.ini");

                File.WriteAllText(desktopIni,
                    $@"[.ShellClassInfo]
IconResource={"icofile.ico"},0");

                // Make desktop.ini hidden + system
                File.SetAttributes(desktopIni,
                    FileAttributes.Hidden | FileAttributes.System);

                // Mark folder as system so icon applies
                var attr = File.GetAttributes(folderPath);
                File.SetAttributes(folderPath, attr | FileAttributes.System);
            } catch { }

            // Refresh explorer
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Redirects the user to a webpage via their default browser.
        /// If an error occurs, it will copy the url to their clipboard instead
        /// </summary>
        /// <param name="url">The webpage to be redirected to</param>
        public void Redirect(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (!(MessageBox.Show($"We Couldn't Redirect You To The Page {url}.\nWould You Like Us To Copy The Link To Your Clipboard Instead?", "Failed to Open Link", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
                    return;
                Clipboard.SetText(url); // source: https://stackoverflow.com/questions/899350/how-do-i-copy-the-contents-of-a-string-to-the-clipboard-in-c
            }
        }

        async Task<bool> ExtractFile(string file, string destination)
        {
            try
            {
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(file, destination);
                });

                return true;
            }
            catch (Exception e)
            {
                Clipboard.SetText(e.ToString());
                MessageBox.Show("I'm sorry, we encountered an error, I'm sorry about this. \nIf you see this message box, please show this to Support, I haven't seen this actually happen yet. \n\n[Nebula Client Extract Process]\n " + e.ToString(), "Nebula Client Installer");
                return false;
            }
        }

        async void DownloadFile(string file, string destination)
        {
            downloadhandler.DownloadFileAsync(new Uri(file), destination);
            while (downloadhandler.IsBusy)
                await Task.Delay(1000);
        }

        void ExcludeApp(string destination)
        {
            // Skidded from Comet

            try
            {
                using (PowerShell powerShell = PowerShell.Create())
                {
                    powerShell.AddScript("Add-MpPreference -ExclusionPath '" + Directory.GetCurrentDirectory() + "'");
                    powerShell.Invoke();
                    powerShell.AddScript("Add-MpPreference -ExclusionPath '" + System.IO.Path.GetFullPath(destination) + "'");
                    powerShell.Invoke();
                    powerShell.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

        private async void discordjoin(object sender, RoutedEventArgs e)
        {
            Redirect("https://dsc.gg/nebulasoftworks");
        }
        #endregion

        #region animation stuff
        /// <summary>
        /// time span variables
        /// </summary> 

        public TimeSpan second = TimeSpan.FromSeconds(1);
        public TimeSpan halfsecond = TimeSpan.FromMilliseconds(500);
        public TimeSpan tenthsecond = TimeSpan.FromMilliseconds(100);
        public TimeSpan hunsecond = TimeSpan.FromMilliseconds(20);

        /// <summary>
        /// Easing Styles
        /// </summary>
        public static ExponentialEase exponentialEase(EasingMode x = EasingMode.EaseInOut)
        { return new ExponentialEase { EasingMode = x }; }
        public static BackEase backEase(EasingMode x = EasingMode.EaseInOut)
        { return new BackEase { EasingMode = x }; }
        public static QuarticEase smoothEase(EasingMode x = EasingMode.EaseInOut)
        { return new QuarticEase { EasingMode = x }; }

        /// <summary>
        /// Smoothly Transition an Object's Opacity
        /// obj = the object to tween opacity
        /// dur = amount of time to twen | TimeSpan
        /// opac = the opacity to tween to
        /// easingStyle = the style of easing that will be applied
        /// </summary>
        public Storyboard Fade(DependencyObject obj, TimeSpan dur, Double opac = 0, IEasingFunction easingStyle = null)
        {
            if (dur == null) dur = second;
            if (easingStyle == null) easingStyle = exponentialEase();

            Storyboard fadeStoryboard = new Storyboard();
            DoubleAnimation fadeAnimation = new DoubleAnimation()
            {
                To = opac,
                Duration = dur,
                EasingFunction = easingStyle
            };
            fadeStoryboard.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, obj);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));

            return fadeStoryboard;
        }

        /// <summary>
        /// Smoothly Move Or Resize (Using Margins) an Object
        /// obj = the object to tween margin
        /// dur = amount of time to twen | TimeSpan
        /// margin = the margin to tween to.
        /// easingStyle = the style of easing that will be applied
        /// </summary>
        public Storyboard ObjectShift(DependencyObject obj, TimeSpan dur, Thickness margin, IEasingFunction easingStyle = null)
        {
            if (dur == null) dur = second;
            if (easingStyle == null) easingStyle = exponentialEase();

            Storyboard posStoryboard = new Storyboard();
            ThicknessAnimation posAnimation = new ThicknessAnimation()
            {
                To = margin,
                Duration = dur,
                EasingFunction = easingStyle
            };
            posStoryboard.Children.Add(posAnimation);
            Storyboard.SetTarget(posAnimation, obj);
            Storyboard.SetTargetProperty(posAnimation, new PropertyPath(MarginProperty));

            return posStoryboard;
        }

        /// <summary>
        /// Smoothly resize (using absolute numbers) an object
        /// obj = the object to tween size
        /// dur = amount of time to twen | TimeSpan
        /// height = the height to tween to
        /// width = the width to tween to
        /// easingStyle = the style of easing that will be applied
        /// heightbool = whether to not tween the height
        /// </summary>
        public async void Resize(UIElement obj, TimeSpan dur, double height, double width, IEasingFunction easingStyle = null, bool heightbool = true)
        {
            if (dur == null) dur = second; // Default to half a second
            if (easingStyle == null) easingStyle = new ExponentialEase { EasingMode = EasingMode.EaseInOut }; // Default easing

            await Task.Delay(0);

            // Create a new Storyboard
            Storyboard sizeStoryboard = new Storyboard();

            // Height Animation
            DoubleAnimation heightAnimation;
            if (heightbool != false)
            {
                heightAnimation = new DoubleAnimation()
                {
                    To = height,
                    Duration = dur,
                    EasingFunction = easingStyle
                };
                sizeStoryboard.Children.Add(heightAnimation);
                Storyboard.SetTarget(heightAnimation, obj);
                Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            }

            // Width Animation
            DoubleAnimation widthAnimation = new DoubleAnimation()
            {
                To = width,
                Duration = dur,
                EasingFunction = easingStyle
            };

            // Add animations to Storyboard
            sizeStoryboard.Children.Add(widthAnimation);

            // Set the target and target properties for animations
            Storyboard.SetTarget(widthAnimation, obj);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(FrameworkElement.WidthProperty));

            sizeStoryboard.Begin();
        }

        /// <summary>
        /// Smoothly transition the color of an object (brush, shadows etc.)
        /// obj = the obj to tween color
        /// dur = how long to tween | TimeSpan
        /// color = the color to tween too
        /// </summary>
        public Storyboard ColorShift(DependencyObject obj, TimeSpan dur, System.Windows.Media.Color color)
        {
            if (dur == null) dur = second;

            Storyboard colorStoryboard = new Storyboard();
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                To = color,
                Duration = second,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };
            colorStoryboard.Children.Add(colorAnimation);
            Storyboard.SetTarget(colorAnimation, obj);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(MarginProperty));

            return colorStoryboard;
        }

        /// <summary>
        /// enable and disable panels (grids, stack panels, scroll viewers)
        /// </summary>

        public void EnableGrid(UIElement selectedGrid)
        {
            if (selectedGrid != null && selectedGrid is Panel SelectedGrid)
            {
                SelectedGrid.Margin = new Thickness(0.0, 260.0, 0.0, -260.0);
                SelectedGrid.Visibility = Visibility.Visible;
                Fade(SelectedGrid, halfsecond, 1, smoothEase()).Begin();
                ObjectShift(SelectedGrid, halfsecond, new Thickness(0.0, 0.0, 0.0, 0.0), smoothEase()).Begin();
            }
            else if (selectedGrid != null)
            {
                ScrollViewer Selectedgrid = (ScrollViewer)selectedGrid;
                Selectedgrid.Margin = new Thickness(0.0, 260.0, 0.0, -260.0);
                Selectedgrid.Visibility = Visibility.Visible;
                Fade(Selectedgrid, halfsecond, 1, smoothEase()).Begin();
                ObjectShift(Selectedgrid, halfsecond, new Thickness(0.0, 0.0, 0.0, 0.0), smoothEase()).Begin();
            }
        }

        public void EnableGridCustom(UIElement selectedGrid, Thickness thickness)
        {
            if (selectedGrid != null && selectedGrid is Panel SelectedGrid)
            {
                SelectedGrid.Margin = new Thickness(0.0, 260.0, 0.0, -260.0);
                SelectedGrid.Visibility = Visibility.Visible;
                Fade(SelectedGrid, halfsecond, 1, smoothEase()).Begin();
                ObjectShift(SelectedGrid, halfsecond, thickness, smoothEase()).Begin();
            }
            else if (selectedGrid != null)
            {
                ScrollViewer Selectedgrid = (ScrollViewer)selectedGrid;
                Selectedgrid.Margin = new Thickness(0.0, 260.0, 0.0, -260.0);
                Selectedgrid.Visibility = Visibility.Visible;
                Fade(Selectedgrid, halfsecond, 1, smoothEase()).Begin();
                ObjectShift(Selectedgrid, halfsecond, thickness, smoothEase()).Begin();
            }
        }

        public async void DisableGrid(UIElement selectedGrid)
        {
            if (selectedGrid != null && selectedGrid is Panel SelectedGrid)
            {
                ObjectShift(SelectedGrid, halfsecond, new Thickness(0.0, 260.0, 0.0, -260.0), smoothEase()).Begin();
                Fade(SelectedGrid, halfsecond, 0, exponentialEase()).Begin();
                await Task.Delay(1000);
                SelectedGrid.Visibility = Visibility.Collapsed;
            }
            else if (selectedGrid != null)
            {
                ScrollViewer Selectedgrid = (ScrollViewer)selectedGrid;
                ObjectShift(Selectedgrid, halfsecond, new Thickness(0.0, 260.0, 0.0, -260.0), smoothEase()).Begin();
                Fade(Selectedgrid, halfsecond, 0, exponentialEase()).Begin();
                await Task.Delay(1000);
                Selectedgrid.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Loaded += blurdisable;

            Directory.CreateDirectory(DataFolder);
            SetFolderIcon(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client");

            #region Weird Shell/Start Menu shit
            try
            {
                string shortcutPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"), "Nebula Client Installer.lnk"); // common for all users for the installer. apps individually use the normal StartMenu
                var shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "Nebula Client Installer";
                shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                shortcut.IconLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                shortcut.Save();
            }
            catch
            {
                string shortcutPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"), "Nebula Client Installer.lnk"); // if it doesnt work
                var shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "Nebula Client Installer";
                shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                shortcut.IconLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                shortcut.Save();
            }
            try
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "nebulaclientinstallercheck.vbs"), @"
Set shell = CreateObject(""WScript.Shell"")

startMenuPath = shell.SpecialFolders(""StartMenu"") & ""\Programs\Nebula Client Installer.lnk""

Set fso = CreateObject(""Scripting.FileSystemObject"")

If fso.FileExists(startMenuPath) Then
    Set shortcut = shell.CreateShortcut(startMenuPath)
    targetPath = shortcut.TargetPath

    If Not fso.FileExists(targetPath) Then
        fso.DeleteFile startMenuPath
    End If
End If
");

            }
            catch
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "nebulaclientinstallercheck.vbs"), @"
Set shell = CreateObject(""WScript.Shell"")

startMenuPath = shell.SpecialFolders(""StartMenu"") & ""\Programs\Nebula Client Installer.lnk""

Set fso = CreateObject(""Scripting.FileSystemObject"")

If fso.FileExists(startMenuPath) Then
    Set shortcut = shell.CreateShortcut(startMenuPath)
    targetPath = shortcut.TargetPath

    If Not fso.FileExists(targetPath) Then
        fso.DeleteFile startMenuPath
    End If
End If
");
            }
            #endregion
        }

        #region Window Core Functionality
        async void backtohome(object sender = null, RoutedEventArgs e = null)
        {
            Home.Visibility = Visibility.Visible;
            Fade(Uninstaller, TimeSpan.FromMilliseconds(600)).Begin();
            Fade(CelestiaInstaller, TimeSpan.FromMilliseconds(600)).Begin();
            Fade(NBTInstaller, TimeSpan.FromMilliseconds(600)).Begin();
            Fade(NebulaLibraryCopy, TimeSpan.FromMilliseconds(600)).Begin();
            await Task.Delay(200);
            Fade(Home, second, 1).Begin();
            CelestiaInstaller.Visibility = Visibility.Collapsed;
            NBTInstaller.Visibility = Visibility.Collapsed;
            NebulaLibraryCopy.Visibility = Visibility.Collapsed;
            Uninstaller.Visibility = Visibility.Collapsed;
        }

        private void Topbar_MouseLeftButtonDown(object sender,  MouseButtonEventArgs e) => DragMove();

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;
            MainBorder.Margin = new Thickness(40);
            await Task.Delay(500);
            ObjectShift(MainBorder, halfsecond, new Thickness(15)).Begin();
            Fade(this, halfsecond, 1).Begin();

            int currentWallpaper = 1;
            new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Normal, delegate
            { 
                var currentImage = (Border)FindName($"Wallpaper{currentWallpaper}");

                currentWallpaper++;
                if (currentWallpaper > 8)
                    currentWallpaper = 1;

                var nextImage = (Border)FindName($"Wallpaper{currentWallpaper}");

                Fade(currentImage, halfsecond).Begin();
                Fade(nextImage, halfsecond, 0.451).Begin();
            }, Dispatcher).Start();


            Home.Opacity = 0;
            CelestiaInstaller.Opacity = 0;
            NBTInstaller.Opacity = 0;
            NebulaLibraryCopy.Opacity = 0;
            Uninstaller.Opacity = 0;
            Home.Visibility = Visibility.Collapsed;
            CelestiaInstaller.Visibility = Visibility.Collapsed;
            NBTInstaller.Visibility = Visibility.Collapsed;
            NebulaLibraryCopy.Visibility = Visibility.Collapsed;
            Loader.Visibility = Visibility.Visible;
            await Task.Delay(400);
            Resize(ProgressBarFill, halfsecond, 3.5, 120);
            await Task.Delay(900);
            Resize(ProgressBarFill, halfsecond, 3.5, 300);
            await Task.Delay(1250);
            Home.Visibility = Visibility.Visible;
            Fade(Loader, TimeSpan.FromMilliseconds(300)).Begin();
            try
            {
                // https://stackoverflow.com/questions/26389629/animate-rendertransform-in-wpf

                var animation = new DoubleAnimation { To = 2, Duration = halfsecond };

                loaderstack.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation, HandoffBehavior.Compose);
                loaderstack.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation, HandoffBehavior.Compose);

            }
            catch { }
            await Task.Delay(200);
            Storyboard anim = Fade(Home, halfsecond, 1);
            anim.Completed += delegate
            {
                ProgressBarFill.Width = 20;
                loaderstack.RenderTransform = new ScaleTransform()
                {
                    CenterX = 0,
                    CenterY = 0,
                    ScaleX = 1,
                    ScaleY = 1
                };
            };
            anim.Begin();

            NBT_EulaText.Text = HttpGet(EULALink("Nebula Trinity Engine"));
            Celestia_EulaText.Text = HttpGet(EULALink("Celestia IDE"));
        }

        private async void Close(object sender, RoutedEventArgs e)
        {
            ObjectShift(MainBorder, TimeSpan.FromMilliseconds(300), new Thickness(30)).Begin();
            Fade(this, TimeSpan.FromMilliseconds(300), 0).Begin();
            await Task.Delay(1000);
            Application.Current.Shutdown();
        }

        private async void nebulalibrarystart(object sender, RoutedEventArgs e)
        {
            NebulaLibraryCopy.Visibility = Visibility.Visible;
            Fade(Home, TimeSpan.FromMilliseconds(600)).Begin();
            await Task.Delay(200);
            var storyboard = Fade(NebulaLibraryCopy, second, 1);
            storyboard.Completed += delegate
            {
                Home.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin();
        }

        private async void nebulatrinityenginestart(object sender, RoutedEventArgs e)
        {
            NBTInstaller.Visibility = Visibility.Visible;
            Fade(Home, TimeSpan.FromMilliseconds(600)).Begin();
            foreach (Grid grid in NBTInstaller.Children)
            {
                grid.Width = 818;
            }
            NBT_InstallationPath.Text = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Nebula Softworks\Nebula Client\Applications\Nebula Trinity Engine";
            await Task.Delay(200);
            var storyboard = Fade(NBTInstaller, second, 1);
            storyboard.Completed += delegate
            {
                Home.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin();
            NBTInstaller.UpdateLayout();
        }

        private async void celestiastart(object sender, RoutedEventArgs e)
        {
            CelestiaInstaller.Visibility = Visibility.Visible;
            Fade(Home, TimeSpan.FromMilliseconds(600)).Begin();
            foreach (Grid grid in CelestiaInstaller.Children)
            {
                grid.Width = 818;
            }
            Celestia_InstallationPath.Text = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Nebula Softworks\Nebula Client\Applications\Celestia IDE";
            Celestia_LauncherPath.Text = $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Celestia Launcher";
            await Task.Delay(200);
            var storyboard = Fade(CelestiaInstaller, second, 1);
            storyboard.Completed += delegate
            {
                Home.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin();
        }

        private async void uninstallstart(object sender, RoutedEventArgs e)
        {
            if (Home.Opacity != 1)
            {
                backtohome();
                return;
            }
            Uninstaller.Visibility = Visibility.Visible;
            Fade(Home, TimeSpan.FromMilliseconds(600)).Begin();
            await Task.Delay(200);
            var storyboard = Fade(Uninstaller, second, 1);
            storyboard.Completed += delegate
            {
                Home.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin();
        }
        #endregion

        #region Nebula Library (Suite?)
        private async void copynebulalibrary(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("loadstring(game:HttpGet('https://raw.nebulasoftworks.xyz/ncscript'))()");
            Fade(nebulalibraryscript, hunsecond).Begin();
            Fade(nebulalibrarynotif, hunsecond, 1).Begin();
            await Task.Delay(1000);
            Fade(nebulalibraryscript, hunsecond, 1).Begin();
            Fade(nebulalibrarynotif, hunsecond, 0).Begin();
        }
        #endregion

        #region Nebula Trinity Engine
        private void NBT_ViewOnGithubButton_Click(object sender, RoutedEventArgs e) => Redirect(GithubRepo);

        private void NBT_StartProcess_Click(object sender, RoutedEventArgs e) => Resize(NBT_Start, halfsecond, 0, 0, null, false);

        private void NBT_DeclineEULA_Click(object sender, RoutedEventArgs e) => Resize(NBT_Start, halfsecond, 0, 818, null, false);

        private void NBT_AcceptEULA_Click(object sender, RoutedEventArgs e) => Resize(NBT_Eula, halfsecond, 0, 0, null, false);

        private void NBT_GoBackFromInstallation_Click(object sender, RoutedEventArgs e) => Resize(NBT_Eula, halfsecond, 0, 818, null, false);

        private void NBT_InstallationPathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            using (BetterFolderBrowser ofd = new BetterFolderBrowser())
            {
                ofd.Title = "Select Installation Directory";
                ofd.RootFolder = System.Windows.Forms.Application.StartupPath;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    NBT_InstallationPath.Text = ofd.SelectedFolder + "\\Nebula Trinity Engine";
                }
            }
        }

        private async void NBT_StartInstalling_Click(object sender, RoutedEventArgs e)
        {
            Resize(NBT_Customise, halfsecond, 0, 0, null, false);
            NBT_Progress.Visibility = Visibility.Visible;
            await Task.Delay(1200);
            Directory.CreateDirectory(NBT_InstallationPath.Text);
            Directory.CreateDirectory(DataFolder + @"\Nebula Trinity Engine");
            File.WriteAllText(DataFolder + @"\Nebula Trinity Engine\InstallPath.data", NBT_InstallationPath.Text);
            NBT_ProgressText.Text = "Downloading Files...";
            DownloadFile("https://github.com/Nebula-Softworks/Nebula-Client-Development/raw/refs/heads/master/Redistrutables/Nebula%20Trinity%20Engine.zip", NBT_InstallationPath.Text + "\\NBT.zip");
            while (downloadhandler.IsBusy)
                await Task.Delay(1000);

            await Task.Delay(600);
            NBT_ProgressText.Text = "Extracting Files...";

                if (File.Exists(NBT_InstallationPath.Text + "\\NBT.zip"))
                {
                    var success = await ExtractFile(NBT_InstallationPath.Text + "\\NBT.zip", NBT_InstallationPath.Text);
                    if (success)
                        try { File.Delete(NBT_InstallationPath.Text + "\\NBT.zip"); } catch { };
                }

                await Task.Delay(600);
                NBT_ProgressText.Text = "Installing...";
                try
                {
                    ExcludeApp(Assembly.GetEntryAssembly().Location);
                    ExcludeApp(NBT_InstallationPath.Text);
                }
                catch { }

                //string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                //if (!currentPath.Contains(NBT_InstallationPath.Text) && NBT_Customise_PATHSelector.IsChecked == true)
                //{
                //    string newPath = currentPath + ";" + NBT_InstallationPath.Text;
                //    try { Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine); } catch { }
                //    Console.WriteLine($"Successfully added '{NBT_InstallationPath.Text}' to the system PATH.");
                //}
                //else
                //{
                //    Console.WriteLine($"'{NBT_InstallationPath.Text}' is already in the system PATH.");
                //}

            NBT_ProgressText.Text = "Done! ✓";
            await Task.Delay(600);
            Resize(NBT_Progress, halfsecond, 0, 0, null, false);
        }

        private async void NBT_FinishProcess_Click(object sender, RoutedEventArgs e)
        {
            foreach (Grid grid in NBTInstaller.Children)
            {
                grid.Width = 818;
            }
            NBT_Finish.Width = 0;
            Fade(NBTInstaller, TimeSpan.FromMilliseconds(600)).Begin();
            await Task.Delay(200);
            Home.Visibility = Visibility.Visible;
            var storyboard = Fade(Home, second, 1);
            storyboard.Completed += delegate
            {
                NBTInstaller.Visibility = Visibility.Collapsed;
                NBTInstaller.UpdateLayout();
            };
            storyboard.Begin();
        }
        #endregion

        #region Celestia IDE

        private void Celestia_ViewOnGithubButton_Click(object sender, RoutedEventArgs e) => Redirect(GithubRepo);

        private void Celestia_StartProcess_Click(object sender, RoutedEventArgs e) => Resize(Celestia_Start, halfsecond, 0, 0, null, false);

        private void Celestia_DeclineEULA_Click(object sender, RoutedEventArgs e) => Resize(Celestia_Start, halfsecond, 0, 818, null, false);

        private void Celestia_AcceptEULA_Click(object sender, RoutedEventArgs e) => Resize(Celestia_Eula, halfsecond, 0, 0, null, false);

        private void Celestia_GoBackFromInstallation_Click(object sender, RoutedEventArgs e) => Resize(Celestia_Eula, halfsecond, 0, 818, null, false);

        private void Celestia_InstallationPathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            using (BetterFolderBrowser ofd = new BetterFolderBrowser())
            {
                ofd.Title = "Select Installation Directory";
                ofd.RootFolder = System.Windows.Forms.Application.StartupPath;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Celestia_InstallationPath.Text = ofd.SelectedFolder + "\\Celestia IDE";
                }
            }
        }

        private void Celestia_LauncherPathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            using (BetterFolderBrowser ofd = new BetterFolderBrowser())
            {
                ofd.Title = "Select Installation Directory";
                ofd.RootFolder = System.Windows.Forms.Application.StartupPath;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Celestia_LauncherPath.Text = ofd.SelectedFolder + "\\Celestia Launcher";
                }
            }
        }

        private async void Celestia_Customise_PriorityInstall_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
            {
                await Task.Delay(500);
                Celestia_Customise_PriorityInstall_Checked(new object(), new RoutedEventArgs());
                return;
            }
            if (Celestia_Customise_PriorityInstall.IsChecked == true)
            {
                Celestia_Customise_StartMenu.Opacity = 1;
                Celestia_Customise_DesktopShortcut.Opacity = 1;
                Celestia_Customise_StartMenu.IsHitTestVisible = true;
                Celestia_Customise_DesktopShortcut.IsHitTestVisible = true;
            }
            else
            {
                Celestia_Customise_StartMenu.Opacity = 0.5;
                Celestia_Customise_DesktopShortcut.Opacity = 0.5;
                Celestia_Customise_StartMenu.IsHitTestVisible = false;
                Celestia_Customise_DesktopShortcut.IsHitTestVisible = false;
            }
        }

        private async void Celestia_StartInstalling_Click(object sender, RoutedEventArgs e)
        {
            Resize(Celestia_Customise, halfsecond, 0, 0, null, false);
            Celestia_Progress.Visibility = Visibility.Visible;
            await Task.Delay(1200);
            Directory.CreateDirectory(Celestia_InstallationPath.Text);
            Directory.CreateDirectory(Celestia_LauncherPath.Text);
            Directory.CreateDirectory(DataFolder + @"\Celestia IDE");
            File.WriteAllText(DataFolder + @"\Celestia IDE\InstallPath.data", Celestia_InstallationPath.Text);
            Celestia_ProgressText.Text = "Downloading Files...";
            DownloadFile("https://github.com/Nebula-Softworks/Nebula-Client-Development/raw/refs/heads/master/Redistrutables/Celestia%20Launcher.zip", Celestia_LauncherPath.Text + "\\Launcher.zip");
            while (downloadhandler.IsBusy)
                await Task.Delay(1000);

            if (Celestia_Customise_PriorityInstall.IsChecked == true)
            {
                DownloadFile("https://github.com/Nebula-Softworks/Nebula-Client-Development/raw/refs/heads/master/Redistrutables/Celestia.zip", Celestia_InstallationPath.Text + "\\Celestia.zip");
                while (downloadhandler.IsBusy)
                    await Task.Delay(1000);
            }

                await Task.Delay(600);
            Celestia_ProgressText.Text = "Extracting Files...";

            if (File.Exists(Celestia_LauncherPath.Text + "\\Launcher.zip"))
            {
                var success = await ExtractFile(Celestia_LauncherPath.Text + "\\Launcher.zip", Celestia_LauncherPath.Text);
                if (success)
                    try { File.Delete(Celestia_LauncherPath.Text + "\\Launcher.zip"); } catch { };
            }
            if (File.Exists(Celestia_InstallationPath.Text + "\\Celestia.zip"))
            {
                var success = await ExtractFile(Celestia_InstallationPath.Text + "\\Celestia.zip", Celestia_InstallationPath.Text);
                if (success)
                    try { File.Delete(Celestia_InstallationPath.Text + "\\Celestia.zip"); } catch { };
            }

            await Task.Delay(600);
            Celestia_ProgressText.Text = "Installing...";
            try
            {
                ExcludeApp(Assembly.GetEntryAssembly().Location);
                ExcludeApp(Celestia_LauncherPath.Text);
            }
            catch { }

            if (Celestia_Customise_PriorityInstall.IsChecked == true)
            {
                try
                {
                    ExcludeApp(Celestia_InstallationPath.Text);
                }
                catch { }
                if (Celestia_Customise_StartMenu.IsChecked == true)
                {
                    try
                    {
                        string shortcutPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"), "Celestia IDE.lnk"); // common for all users for the installer. apps individually use the normal StartMenu
                        var shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                        shortcut.Description = "Celestia IDE";
                        shortcut.TargetPath = Celestia_InstallationPath.Text + "\\Celestia IDE.exe";
                        shortcut.IconLocation = Celestia_InstallationPath.Text + "\\Celestia IDE.exe";
                        shortcut.Save();
                    }
                    catch
                    {
                        string shortcutPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"), "Celestia IDE.lnk"); // if it doesnt work
                        var shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                        shortcut.Description = "Celestia IDE";
                        shortcut.TargetPath = Celestia_InstallationPath.Text + "\\Celestia IDE.exe";
                        shortcut.IconLocation = Celestia_InstallationPath.Text + "\\Celestia IDE.exe";
                        shortcut.Save();
                    }
                }
                if (Celestia_Customise_DesktopShortcut.IsChecked == true)
                {
                    try
                    {
                        string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Celestia IDE.lnk"); // if it doesnt work
                        var shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                        shortcut.Description = "Celestia IDE";
                        shortcut.TargetPath = Celestia_InstallationPath.Text + "\\Celestia IDE.exe";
                        shortcut.IconLocation = Celestia_InstallationPath.Text + "\\Celestia IDE.exe";
                        shortcut.Save();
                    }
                    catch (Exception ex)
                    {
                        Clipboard.SetText(ex.ToString());
                        MessageBox.Show("I'm sorry, we encountered an error, I'm sorry about this. \nIf you see this message box, please show this to Support, I haven't seen this actually happen yet. \n\n[Nebula Client Install Process]\n " + e.ToString(), "Nebula Client Installer");
                    }
                }
            }

            //string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            //if (!currentPath.Contains(NBT_InstallationPath.Text) && NBT_Customise_PATHSelector.IsChecked == true)
            //{
            //    string newPath = currentPath + ";" + NBT_InstallationPath.Text;
            //    try { Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine); } catch { }
            //    Console.WriteLine($"Successfully added '{NBT_InstallationPath.Text}' to the system PATH.");
            //}
            //else
            //{
            //    Console.WriteLine($"'{NBT_InstallationPath.Text}' is already in the system PATH.");
            //}

            Celestia_ProgressText.Text = "Done! ✓";
            await Task.Delay(600);
            Resize(Celestia_Progress, halfsecond, 0, 0, null, false);
        }

        private async void Celestia_FinishProcess_Click(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Visible;
            Fade(CelestiaInstaller, TimeSpan.FromMilliseconds(600)).Begin();
            foreach (Grid grid in CelestiaInstaller.Children)
            {
                grid.Width = 818;
            }
            await Task.Delay(200);
            var storyboard = Fade(Home, second, 1);
            storyboard.Completed += delegate
            {
                CelestiaInstaller.Visibility = Visibility.Collapsed;
                CelestiaInstaller.UpdateLayout();
            };
            storyboard.Begin();
        }
        #endregion

        #region Uninstalls

        // Source - https://stackoverflow.com/a/22282428
        // Posted by Jone Polvora, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-02-05, License - CC BY-SA 4.0
        public static void ClearFolder(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var file in baseDir.GetFiles())
            {
                File.Delete(file.FullName);
            }
            foreach (var dir in baseDir.EnumerateDirectories())
            {
                ClearFolder(dir);
            }
            baseDir.Delete(true);
        }

        private void nebulatrinityengine_uninstall(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(
                "WARNING!: THIS WILL CLEAR YOUR AUTOEXEC AND WORKSPACES FOR ALL\nNEBULA TRINITY ENGINE APPS\n\nARE YOU SURE YOU WISH TO PROCEED?", 
                "WARNING!", System.Windows.Forms.MessageBoxButtons.YesNo ,System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    var folder = Directory.CreateDirectory(DataFolder + @"\Nebula Trinity Engine");
                    ClearFolder(new DirectoryInfo(File.ReadAllText(folder.FullName + "\\InstallPath.data")));
                    ClearFolder(folder);
                }
                finally
                {

                    System.Windows.Forms.MessageBox.Show("Uninstalled");

                }
            }
        }

        private void celestiaide_uninstall(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(
                "Heads Up!: This will delete all Celestia Application Files on your system (based on the install).\nAppdata and Cache (settings, saved tabs, account) will not be cleared." +
                "\nLauncher will not be deleted.\n\nAre you sure you wish to proceed?",
                "WARNING!", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    var folder = Directory.CreateDirectory(DataFolder + @"\Celestia IDE");
                    ClearFolder(new DirectoryInfo(File.ReadAllText(folder.FullName + "\\InstallPath.data")));
                }
                finally
                {

                    System.Windows.Forms.MessageBox.Show("Uninstalled");

                }
            }
        }

        #endregion

    }
}