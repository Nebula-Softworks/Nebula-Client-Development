using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38; private enum DWM_SYSTEMBACKDROP_TYPE {  DWMSBT_AUTO = 0, DWMSBT_NONE = 1,   DWMSBT_MAINWINDOW = 2, DWMSBT_TRANSIENTWINDOW = 3, DWMSBT_TABBEDWINDOW = 4 }
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
        private void blurdisable(object sender, RoutedEventArgs e)
        {  var hwnd = new WindowInteropHelper(this).Handle; int backdrop = (int)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_NONE; DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int)); }


        static string Website = "https://nebulasoftworks.xyz/nebulaclient";
        static string Distrubutor = $"{Website}";
        static string GithubRepo = "https://github.com/Nebula-Softworks/Nebula-Client";

        public string HttpGet(string weburl)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                return webClient.DownloadString(weburl);
            }
        }

        public void Redirect(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception e)
            {
                if (!(MessageBox.Show($"We Couldn't Redirect You To The Page {url}.\nWould You Like Us To Copy The Link To Your Clipboard Instead?", "Failed to Open Link", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
                    return;
                Clipboard.SetText(url); // source: https://stackoverflow.com/questions/899350/how-do-i-copy-the-contents-of-a-string-to-the-clipboard-in-c
            }
        }

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

        public MainWindow()
        {
            InitializeComponent();
            Loaded += blurdisable;

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
        }

        private void Topbar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0;
            MainBorder.Margin = new Thickness(40);
            await Task.Delay(500); 
            ObjectShift(MainBorder, halfsecond, new Thickness(15)).Begin();
            Fade(this, halfsecond, 1).Begin();

            int currentWallpaper = 1;
            new DispatcherTimer(TimeSpan.FromMinutes(2.1), DispatcherPriority.Normal, delegate
            {
                // TODO
                // make this only happen when doing something long (eg. Downloading or installing)
                var currentImage = (Border)FindName($"Wallpaper{currentWallpaper}");

                currentWallpaper++;
                if (currentWallpaper > 7)
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

            } catch { }
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
        }

        private async void Close(object sender, RoutedEventArgs e)
        {
            ObjectShift(MainBorder, TimeSpan.FromMilliseconds(300), new Thickness(30)).Begin();
            Fade(this, TimeSpan.FromMilliseconds(300), 0).Begin();
            await Task.Delay(1000);
            Application.Current.Shutdown();
        }

        private void discordjoin(object sender, RoutedEventArgs e)
        {
            Redirect("https://dsc.gg/nebulasoftworks");
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
            await Task.Delay(200);
            var storyboard = Fade(NBTInstaller, second, 1);
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

        private async void copynebulalibrary(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("loadstring(game:HttpGet('https://raw.nebulasoftworks.xyz/ncscript'))()");
            Fade(nebulalibraryscript, hunsecond).Begin();
            Fade(nebulalibrarynotif, hunsecond, 1).Begin();
            await Task.Delay(1000);
            Fade(nebulalibraryscript, hunsecond,1).Begin();
            Fade(nebulalibrarynotif, hunsecond, 0).Begin();
        }
    }
}