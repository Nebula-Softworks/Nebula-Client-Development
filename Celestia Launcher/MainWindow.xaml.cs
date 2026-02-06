using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Interop;

namespace Celestia_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebClient downloadhandler = new WebClient();
        public string HttpGet(string weburl)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                return webClient.DownloadString(weburl);
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
                MessageBox.Show(
                    "I'm sorry, we encountered an error, I'm sorry about this.\n" +
                    "If you see this message box, please show this to Support.\n\n" +
                    "[Celestia Launcher Extract Process]\n" + e,
                    "Celestia Launcher");

                return false;
            }
        }


        async void DownloadFile(string file, string destination)
        {
            downloadhandler.DownloadFileAsync(new Uri(file), destination);
            while (downloadhandler.IsBusy)
                await Task.Delay(1000);
        }

        // Source - https://stackoverflow.com/a/22282428
        // Posted by Jone Polvora, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-02-05, License - CC BY-SA 4.0
        public static void ClearFolder(DirectoryInfo baseDir, bool isBase = false)
        {
            if (!baseDir.Exists)
                return;
            foreach (var file in baseDir.GetFiles())
            {
                File.Delete(file.FullName);
            }
            foreach (var dir in baseDir.EnumerateDirectories())
            {
                ClearFolder(dir, false);
            }
            if (!isBase) baseDir.Delete(true);
        }

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
        public ColorAnimation ColorShift(TimeSpan dur, System.Windows.Media.Color color)
        {
            if (dur == null) dur = second;

            ColorAnimation colorAnimation = new ColorAnimation()
            {
                To = color,
                Duration = second,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };
            return colorAnimation;
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
            Opacity = 0;
            Loaded += async delegate
            {
                MainBackgroundBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#000D0C0F");
                mainTitle.Opacity = 0;
                mainTitle.Width = 0;
                mainLogo.Opacity = 0;

                mainLogo.Height = 200;
                mainLogo.Width = 200;
                Opacity = 1;
                await Task.Delay(100);
                Fade(mainLogo, second, opac: 1, easingStyle: new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }).Begin();
                await Task.Delay(1400);
                MainBackgroundBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(hunsecond, (Color)ColorConverter.ConvertFromString("#0D0C0F")));
                await Task.Delay(800);
                Resize(mainLogo, halfsecond, 100, 100, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut });
                Resize(mainTitle, halfsecond, 0, 168, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }, false);
                Fade(mainTitle, second, opac: 1, easingStyle: new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }).Begin();

                if (!File.Exists(".\\autoexec.lnk"))
                {
                    string shortcutPath = ".\\autoexec.lnk";
                    var shell = new IWshRuntimeLibrary.WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.Description = "Celestia IDE";
                    shortcut.TargetPath = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Nebula Trinity Engine\InstallPath.data") + "\\autoexec";
                    shortcut.Save();
                }
                if (!File.Exists(".\\workspace.lnk"))
                {
                    string shortcutPath = ".\\workspace.lnk";
                    var shell = new IWshRuntimeLibrary.WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.Description = "Celestia IDE";
                    shortcut.TargetPath = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Nebula Trinity Engine\InstallPath.data") + "\\workspace";
                    shortcut.Save();
                }

                await Task.Delay(900);
                string latest = HttpGet("https://github.com/Nebula-Softworks/Nebula-Client-Development/raw/refs/heads/master/Redistrutables/latest_release.txt")?.Trim();

                string installDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Nebula Softworks\Nebula Client\Data\Celestia IDE\InstallPath.data"
                );

                string localRelease = "";

                string installFolder = File.ReadAllText(installDataPath).Trim();

                if (Directory.Exists(installFolder))
                {
                    string releaseFile = Path.Combine(installFolder, "current_release");

                    if (File.Exists(releaseFile))
                    {
                        localRelease = File.ReadAllText(releaseFile).Trim();
                    }
                }

                if (latest == localRelease)
                {
                    CloseWindow();
                    Process.Start(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE\InstallPath.data")
                        + "\\Celestia IDE.exe");
                }
                else
                {
                    // TODO: read settings file and set accent if a custom one exists

                    await Task.Delay(300);
                    ObjectShift(BrandStack, halfsecond, new Thickness(-8, 0, 0, 85), new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }).Begin();
                    await Task.Delay(100);
                    Fade(ProgressBarMain, halfsecond, 1, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }).Begin();
                    Fade(statusText, halfsecond, 1, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }).Begin();
                    Fade(WindowButtons, halfsecond, 1, new PowerEase() { Power = 5, EasingMode = EasingMode.EaseInOut }).Begin();
                    statusText.Text = "Checking Versions...";
                    await Task.Delay(200);
                    statusText.Text = "Downloading Celestia Files...";
                    Resize(ProgressBarFill, tenthsecond, 0, 32, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }, false);
                    var FolderPath = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE\InstallPath.data");
                    Console.WriteLine(FolderPath);
                    ClearFolder(Directory.CreateDirectory(FolderPath), true);
                    await Task.Delay(800);
                    DownloadFile("https://github.com/Nebula-Softworks/Nebula-Client-Development/raw/refs/heads/master/Redistrutables/Celestia.zip",
                         FolderPath + "\\Celestia.zip");
                    while (downloadhandler.IsBusy)
                        await Task.Delay(1000);
                    await Task.Delay(500);
                    statusText.Text = "Extracting Celestia Files...";
                    Resize(ProgressBarFill, tenthsecond, 0, 196, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }, false);
                    if (File.Exists(FolderPath + "\\Celestia.zip"))
                    {
                        var success = await ExtractFile(FolderPath + "\\Celestia.zip", FolderPath);
                        if (success)
                            try { File.Delete(FolderPath + "\\Celestia.zip"); } catch { };
                    }
                    await Task.Delay(600);
                    statusText.Text = "Finishing Up...";
                    Resize(ProgressBarFill, tenthsecond, 0, 287, new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut }, false);
                    await Task.Delay(300);
                    CloseWindow();
                    Process.Start(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE\InstallPath.data")
                        + "\\Celestia IDE.exe");
                }
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        public async void CloseWindow()
        {
            ObjectShift(MainBackgroundBorder, TimeSpan.FromMilliseconds(160), new Thickness(80)).Begin();
            Fade(this, TimeSpan.FromMilliseconds(300), 0).Begin();
            await Task.Delay(1000);
            Close();
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => CloseWindow();
    }
}
