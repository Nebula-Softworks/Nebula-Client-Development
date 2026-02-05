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
                Fade(mainLogo, second, opac: 1, easingStyle: new PowerEase() { Power = 4, EasingMode = EasingMode.EaseInOut } ).Begin();
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

                if (HttpGet("https://github.com/Nebula-Softworks/Nebula-Client-Development/raw/refs/heads/master/Redistrutables/latest_release.txt") == 
                    File.ReadAllText(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE\InstallPath.data") 
                    + "\\current_release"))
                {
                    CloseWindow();
                    Process.Start(File.ReadAllText(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE\InstallPath.data")
                        + "\\Celestia IDE.exe"));
                }
                
                // else, objectshift up and fade in progress bar and show the close and minimise, then start download
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        public async void CloseWindow()
        {
            ObjectShift(MainBackgroundBorder, TimeSpan.FromMilliseconds(300), new Thickness(30)).Begin();
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
