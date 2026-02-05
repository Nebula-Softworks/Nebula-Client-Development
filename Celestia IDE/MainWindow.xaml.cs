// System
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.IO.Compression;
using System.Diagnostics;
using Drawing = System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Web;
using System.Management;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Controls;
using Primitives = System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Input;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Globalization;
// Microsoft
using Microsoft;
using Microsoft.Win32;
// Local Project
using Celestia_IDE.Core;
using Celestia_IDE.Core.Editor;
using Celestia_IDE.Core.ExplorerSystem;
using Celestia_IDE.Controls;
using Celestia_IDE.Controls.ScriptCloud;
using Celestia_IDE.Controls.InstanceManager;
using Celestia_IDE.Controls.Settings;
using Celestia_IDE.Controls.Console;
// Packages
using WK.Libraries.BetterFolderBrowserNS;
using CefSharp.Core;
using CefSharp;
using CefSharp.Wpf;
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ColorPicker;
using RPC = DiscordRPC;
using NullSoftware.ToolKit;

namespace Celestia_IDE
{
    /*
    Classes for um the Tab System save
    */
    public class tabclass
    {
        public string Title; public object Tag; public string Content;
    }
    public class TabSession
    {
        public int ActiveTabIndex { get; set; }
        public List<tabclass> Tabs { get; set; } = new();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")] // this is so it can be dragged from aero snap maximise, from some stack overflow post https://stackoverflow.com/questions/7417739/make-wpf-window-draggable-no-matter-what-element-is-clicked
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        // Window Maximsie Respector thing
        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
        [StructLayout(LayoutKind.Sequential)] public struct MINMAXINFO { public POINT ptReserved; public POINT ptMaxSize; public POINT ptMaxPosition; public POINT ptMinTrackSize; public POINT ptMaxTrackSize; }
        protected override void OnSourceInitialized(EventArgs e) { base.OnSourceInitialized(e); var hwnd = new WindowInteropHelper(this).Handle; HwndSource.FromHwnd(hwnd).AddHook(WndProc); }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) { const int WM_GETMINMAXINFO = 0x0024; if (msg == WM_GETMINMAXINFO) { handled = true; WmGetMinMaxInfo(hwnd, lParam); } return IntPtr.Zero; }
        private double GetDpiScaleX() { var source = PresentationSource.FromVisual(this); return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0; }
        private double GetDpiScaleY() { var source = PresentationSource.FromVisual(this); return source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0; }
        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) { var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam); var screen = System.Windows.Forms.Screen.FromHandle(hwnd); mmi.ptMaxPosition.X = screen.WorkingArea.Left - screen.Bounds.Left; mmi.ptMaxPosition.Y = screen.WorkingArea.Top - screen.Bounds.Top; mmi.ptMaxSize.X = screen.WorkingArea.Width; mmi.ptMaxSize.Y = screen.WorkingArea.Height; mmi.ptMinTrackSize.X = (int)(MinWidth * GetDpiScaleX()); mmi.ptMinTrackSize.Y = (int)(MinHeight * GetDpiScaleY()); Marshal.StructureToPtr(mmi, lParam, true); }

        /// <summary>
        /// Gets whether the system is Windows 10
        /// </summary>
        /// <returns>If is windows 10, true. else false</returns>
        bool IsWindows10() { var v = Environment.OSVersion.Version; return v.Major == 10 && v.Build < 22000; }

        /// <summary>
        /// Returns the closest or equivalent value to *value* that is greater or equal to *min* and is smaller or equal to *max*
        /// </summary>
        /// <param name="value">Value you wish to Clamp</param>
        /// <param name="min">Minimum returned value</param>
        /// <param name="max">Maximum returned value</param>
        /// <returns></returns>
        static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /*
         Static/Constant Pages/User Controls
         */
        Explorer explorerPanel = null;
        SourceControl sourceControl = null;
        OutputConsole output = null;
        TerminalConsole terminal = null;
        ScriptCloud scriptCloud = null;
        InstanceManager instanceManager = null;
        SettingsPage settings = null;

        /*
         Cool Variables
         */
        public static string NebulaClientPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Nebula Softworks\Nebula Client\Data\Celestia IDE";
        static string Website = "nebulasoftworks.xyz/nebulaclient";
        private void discordjoin()
        {
            Redirect("https://dsc.gg/nebulasoftworks");
        }
        bool _contextmenuover = false;
        TaskCompletionSource<MessageBoxResult> _promptDialogWaiter;
        public string _WorkspaceFolder = "";
        public string WorkspaceFolder
        {
            get => _WorkspaceFolder;
            set
            {
                _WorkspaceFolder = value;
                explorerPanel.InitializeFileWatcher(WorkspaceFolder);
                explorerPanel.Reload();

                if (TabSystemz.maintabs.SelectedIndex == -1)
                {
                    SetBaseRichPresence();
                }
            }
        }
        public List<TabItem> tabitemcache = new List<TabItem>();
        bool isabletoclose = false;
<<<<<<< HEAD
=======
        QuorumAPI.QuorumModule quorum = new QuorumAPI.QuorumModule();
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e

        /// <summary>
        /// Returns the content of a provided webpage
        /// </summary>
        /// <param name="weburl">The link of the webpage to fetch from</param>
        /// <returns>String, The content of the page</returns>
        public string HttpGet(string weburl)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (compatible; MSIE 6.0; Windows NT 10.0; .NET CLR 1.0.3705; Win64; x64; rv:146.0) Gecko/20100101 Firefox/146.0");
                try
                {
                    return webClient.DownloadString(weburl);
                }
                catch { }
                return "";
            }
        }

        /// <summary>
        /// Redirects the user to a webpage via their default browser.
        /// If an error occurs, it will copy the url to their clipboard instead
        /// </summary>
        /// <param name="url">The webpage to be redirected to</param>
        public async void Redirect(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                ApplicationPrint(2, "Failed to redirect user. Manually Prompting To Copy Link To Clipboard...");
                if ((await Prompt($"We Couldn't Redirect You To The Page {url}.\nWould You Like Us To Copy The Link To Your Clipboard Instead?", "Failed to Open Link", "No Thanks", "Okay") == MessageBoxResult.OK))
                    Clipboard.SetText(url); // source: https://stackoverflow.com/questions/899350/how-do-i-copy-the-contents-of-a-string-to-the-clipboard-in-c
            }
        }

        /// <summary>
        /// Print in the app Output
        /// </summary>
        /// <param name="OUTPUT_TYPE">DEBUG, ERROR, WARNING, SUCCESS, INFO, nil (no content in type)</param>
        /// <param name="OUTPUT_CONTENT"></param>
        public void ApplicationPrint(int OUTPUT_TYPE, string OUTPUT_CONTENT)
        {
            string typestring = null;
            Brush brush = null;
            switch (OUTPUT_TYPE)
            {
                case 1:
                    typestring = "[DEBUG] ";
                    brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#CF9FFF");
                    break;
                case 2:
                    typestring = "[ERROR] ";
                    brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#e59d9d");
                    break;
                case 3:
                    typestring = "[WARNING] ";
                    brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#f0d686");
                    break;
                case 4:
                    typestring = "[SUCCESS] ";
                    brush = Brushes.LightGreen;
                    break;
                case 5:
                    typestring = "[INFO ] ";
                    brush = Brushes.CornflowerBlue;
                    break;
                default:
                    typestring = "";
                    brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#d5d3d7");
                    break;
            }

            Dispatcher.Invoke(delegate
            {
                output.Main.Items.Add(new
                {
                    Time = "[" + DateTime.Now.ToString(new CultureInfo("fr-FR")) + "." + DateTime.Now.Millisecond.ToString("D3") + "]",
                    Type = typestring,
                    Color = brush,
                    Message = "> " + OUTPUT_CONTENT
                });
                output.Main.ScrollIntoView(output.Main.Items.GetItemAt(output.Main.Items.Count - 1));
            });
        }

        /// <summary>
        /// Prints an array of strings to the Debug Console of the application, mimicing the Roblox Luau Output
        /// Deprecated And replaced by ApplicationPrint
        /// </summary>
        /// <param name="STRING_PRINT">An array of strings to be printed, each on seperate lines.</param>
        [Obsolete("Not Used, Deprecated by ApplicationPrint")]
        public void print(params string[] STRING_PRINT)
        {
            if (STRING_PRINT.Length == 0) return;
            for (int i = 1; i < STRING_PRINT.Length; i++)
            {
                Console.WriteLine(i);
            }
        }

        /// <summary>
        /// Prompts the user with a custom message box, yielding the thread until a response is submitted.
        /// </summary>
        /// <param name="content">The content/paragraph text</param>
        /// <param name="title">The header of the message box</param>
        /// <param name="secondaryText">NULLABLE ? the grey button's text : Grey button won't be shown</param>
        /// <param name="primaryText">NULLABLE ? the accent button's text : Accent button won't be shown</param>
        /// <returns></returns>
        public async Task<MessageBoxResult> Prompt(string content, string title, string secondaryText = null, string primaryText = null)
        {
            _promptDialogWaiter = new TaskCompletionSource<MessageBoxResult>();

            Popup popup = new Popup();

            popup.TitleBlock.Text = title;
            popup.ContentBlock.Text = content;
            if (string.IsNullOrEmpty(secondaryText))
                popup.MainButton.Visibility = Visibility.Collapsed;
            else
                popup.MainButton.Content = secondaryText;

            if (string.IsNullOrEmpty(primaryText))
                popup.AccentButton.Visibility = Visibility.Collapsed;
            else
                popup.AccentButton.Content = primaryText;

            DialogHost.Children.Add(popup);
            DialogBackground.Visibility = Visibility.Visible;
            Fade(DialogBackground, tenthsecond, 1).Begin();

            popup.MainButton.Click += (_, __) =>
                _promptDialogWaiter.TrySetResult(MessageBoxResult.Cancel);

            popup.AccentButton.Click += (_, __) =>
                _promptDialogWaiter.TrySetResult(MessageBoxResult.OK);

            var result = await _promptDialogWaiter.Task;
            DialogBackground_MouseLeftButtonDown(new object(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
            return result;
        }

        /// <summary>
        /// Notifies the user with a uninteractable popup at the top right of the interface
        /// </summary>
        /// <param name="Notification_Type">The Type of notification which will determine the color and icon</param>
        /// <param name="Title">The header</param>
        /// <param name="Content">The paragraph text</param>
        /// <param name="Duration">How long the notification will be visible</param>
        public async void Notify(int Notification_Type, string Title, string Content, int Duration)
        {
            Notification notification = new Notification();
            Brush brush = null;
            switch (Notification_Type)
            {
                case 1: //info (checking key, updated, update available, etc)
                    brush = Brushes.CornflowerBlue;
                    notification.Icon.Text = "";
                    break;

                case 2: // error (failed to inject, missing files, etc)
                    brush = Brushes.IndianRed;
                    notification.Icon.Text = "";
                    break;

                case 3: // success (succesfully injected etc)
                    brush = Brushes.LightGreen;
                    notification.Icon.Text = "";
                    break;

                case 4: //warning (use alt, beta, etc)
                    brush = Brushes.Orange;
                    notification.Icon.Text = "";
                    break;
            }

            notification.DurationBar.Background = brush;
            notification.Icon.Foreground = brush;
            notification.Title.Text = Title;
            notification.Description.Text = Content;

            NotificationPanel.Children.Insert(0, notification);
            await Task.Delay(500);
            notification.Main.Width = 300;
            double ActlHeight = notification.ActualHeight;
            notification.Main.Height = ActlHeight;
            notification.Main.Width = 0;
            notification.DurationBar.Margin = new Thickness(0, 0, 0, 0);
            Resize(notification.Main, second, 0, 300, null, false);
            notification.DurationBar.Margin = new Thickness(-12, 0, -12, -7);

            var duration = TimeSpan.FromSeconds(Duration);
            notification.DurationBar.Width = notification.DurationBar.ActualWidth;
            notification.DurationBar.HorizontalAlignment = HorizontalAlignment.Left;
            Resize(notification.DurationBar, duration, 0, 0, new PowerEase() { Power=1 }, false);
            await Task.Delay(duration);
            Resize(notification.Main, TimeSpan.FromMilliseconds(440), 0, 0, null, false);
            await Task.Delay(halfsecond);
            Resize(notification.Main, halfsecond, 0, 0, null, true);
            await Task.Delay(halfsecond);
            NotificationPanel.Children.Remove(notification);
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

            /*
             Initialise Components And bind Events
             */
            explorerPanel = new Explorer(this);
            sourceControl = new SourceControl(this);
            output = new OutputConsole();
            terminal = new TerminalConsole();
            scriptCloud = new ScriptCloud(this);
            instanceManager = new InstanceManager();
            settings = new SettingsPage();
            SidebarFrame.Content = explorerPanel;
            ConsoleFrame.Content = output;
            OutputCheck.IsChecked = true;
            PanelBar.MouseMove += PanelBar_MouseMove;
            PanelBar.MouseLeftButtonDown += PanelBar_MouseDown;
            PanelBar.MouseLeave += PanelBar_MouseLeave;
            Sidebar.MouseMove += Sidebar_MouseMove;
            Sidebar.MouseLeave += Sidebar_MouseLeave;
            Sidebar.PreviewMouseLeftButtonDown += Sidebar_MouseDown;
            MouseMove += Window_MouseMove;
            MouseMove += Dragger_MouseMove;
            MouseUp += Window_MouseUp;
            MouseEnter += delegate { _mouseInside = true; };
            MouseLeave += delegate { _mouseInside = false; };
            PreviewMouseUp += (_, _) => { isDragging = false; explorerPanel._isDragDropActive = false; EngineDraggerButton.ReleaseMouseCapture(); };
            PreviewMouseDown += async (_, e) =>
            {
<<<<<<< HEAD
                await Task.Delay(5);
=======
                await Task.Delay(10);
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                if (_contextmenuover) return;
                HideMenus();
            };
            Loaded += async delegate
            {
                InitializeEngine();
                StartProcessCheck();
                BindRPCSettings();
                CreateSettingsObjects();

                if (!Directory.Exists(NebulaClientPath)) Directory.CreateDirectory(NebulaClientPath);
                ApplicationPrint(1, "Celestia IDE is up to date.");
                ApplicationPrint(1, "Checking files, this may take awhile...");
                await Task.Delay(2300);
                ApplicationPrint(1, "All Files Available, check closed.");
                ApplicationPrint(5, "Celestia Fully Initialised, Welcome to Nebula Client!");
                Notify(4, "Nebula Client is in Alpha!", "Please take note that Celestia and the rest of Nebula Client is currently in its Alpha Stage.\nExpect Bugs, and help report them in the discord server. Thank you.", 5);
                Notify(1, "Checking for updates", "Celestia is currently checking for Nebula Client updates.\nYou may continue to use it while it does.", 3);
                ApplicationPrint(3, "Please Make Sure to Use On An ALT Account to ensure maximum security of your account, we will not be responsible for bans!");
            };
            SizeChanged += delegate
            {
                // Makes sure the Engine buttons are within window bounds
                DraggableEngineButtons.Margin = new Thickness(
                 Clamp(DraggableEngineButtons.Margin.Left, 15, ActualWidth - 42 - DraggableEngineButtons.ActualWidth),
                 Clamp(DraggableEngineButtons.Margin.Top, 15, ActualHeight - 45 - DraggableEngineButtons.ActualHeight),
                 0, 0);
            };
            Loaded += async delegate
            {
                await Task.Delay(0);
                // Loads previous session saved tabs
                if (File.Exists(NebulaClientPath + @"\cache\tabs.celestia"))
                {
                    var json = File.ReadAllText(NebulaClientPath + @"\cache\tabs.celestia");
                    var session = JsonConvert.DeserializeObject<TabSession>(json);

                    foreach (tabclass tab in session.Tabs)
                    {
                        TabSystemz.maintabs.Items.Add(TabSystemz.CreateTab(tab.Content, tab.Title, tab.Tag != null, (string?)tab.Tag));
                    }
                    TabSystemz.maintabs.UpdateLayout();

                    for (int i = 0; i < TabSystemz.maintabs.Items.Count; i++)
                    {
                        TabSystemz.maintabs.SelectedIndex = i;
                        await Dispatcher.Yield(DispatcherPriority.Background);
                    }
                    TabSystemz.maintabs.SelectedIndex = session.ActiveTabIndex;
                    if (TabSystemz.maintabs.SelectedIndex != -1)
                        SetRpcFile(((TextBox)((TabItem)TabSystemz.maintabs.SelectedItem).Header).Text);

                    File.Delete(NebulaClientPath + @"\cache\tabs.celestia");
                }
                
                //loads previous session's workspace
                if (File.Exists(NebulaClientPath + @"\cache\workspace.celestia"))
                {
                    WorkspaceFolder = File.ReadAllText(NebulaClientPath + @"\cache\workspace.celestia");
                    File.Delete(NebulaClientPath + @"\cache\workspace.celestia");
                }

                // loads previous session's window properties
                if (File.Exists(NebulaClientPath + @"\cache\window.celestia"))
                {
                    var json = File.ReadAllText(NebulaClientPath + @"\cache\window.celestia");
                    dynamic session = JsonConvert.DeserializeObject(json);

                    Height = session.Height != null ? session.Height : Height;
                    Width = session.Width != null ? session.Width : Width;
                    Left = session.PositionX != null ? Convert.ToDouble(session.PositionX) : (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
                    Top = session.PositionY != null ? Convert.ToDouble(session.PositionY) : (SystemParameters.PrimaryScreenWidth - ActualHeight) / 2;
                    SideBarColumn.Width = new GridLength(session.SidebarWidth != null ? Convert.ToDouble(session.SidebarWidth) : 220, GridUnitType.Pixel);
                    PanelBar.Height = session.PanelSize != null ? Convert.ToDouble(session.PanelSize) : 240;
                    if (session.ActiveSidebar != null) if (session.ActiveSidebar == 1) Activitybar_SourceControl(new object(), new RoutedEventArgs());
                    if (session.ActivePanel != null) if (session.ActivePanel == 0) TerminalCheck.IsChecked = true;
                    if (session.IsPanelOpen != null) if (session.IsPanelOpen == false) PanelBar.Visibility = Visibility.Collapsed;
                    if (PanelBar.IsVisible) TabSystemz.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height); else TabSystemz.Margin = new Thickness(0, 0, 0, 0);
<<<<<<< HEAD
                    if (PanelBar.IsVisible) HomeTabPage.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height); else TabSystemz.Margin = new Thickness(0, 0, 0, 0);
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                    if (session.IsSidebarOpen != null) if (session.IsSidebarOpen == false)
                        {
                            SideBarColumn.Width = new GridLength(0);
                            currentSideBarSize = SideBarColumn.Width.Value;
                        }
                    if (session.EngineButtonX != null && session.EngineButtonY != null)
                    {
                        DraggableEngineButtons.Margin = new Thickness(
                            Clamp(Convert.ToDouble(session.EngineButtonX), 15, ActualWidth - 42 - DraggableEngineButtons.ActualWidth),
                            Clamp(Convert.ToDouble(session.EngineButtonY), 15, ActualHeight - 45 - DraggableEngineButtons.ActualHeight),
                        0, 0);
                    }
                    else
                    {
                        DraggableEngineButtons.Margin = new Thickness(
                            Clamp(DraggableEngineButtons.Margin.Left, 15, ActualWidth - 42 - DraggableEngineButtons.ActualWidth),
                            Clamp(DraggableEngineButtons.Margin.Top, 15, ActualHeight - 45 - DraggableEngineButtons.ActualHeight),
                        0, 0);
                    }

                    File.Delete(NebulaClientPath + @"\cache\window.celestia");
                }

                if (IsWindows10())
                {
                    windowstate(0, WindowState == WindowState.Maximized);
                }
            };
            Closing += async (_, e) =>
            {
                if (isabletoclose == false) e.Cancel = true;

<<<<<<< HEAD
=======
                quorum.StopCommunication();
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                try
                {
                    TerminateConnection();
                } catch { }

                var tabTasks = tabitemcache.Cast<TabItem>().Select(async t => new tabclass
                {
                    Title = ((TextBox)t.Header).Text,
                    Tag = t.Tag,
                    Content = await ((monaco_api)t.Content).GetText()
                });

                var tabs = (await Task.WhenAll(tabTasks)).ToList();

                var session = new TabSession
                {
                    ActiveTabIndex = TabSystemz.maintabs.SelectedIndex,
                    Tabs = tabs
                };

                var json = JsonConvert.SerializeObject(session, Newtonsoft.Json.Formatting.Indented);

                File.WriteAllText(NebulaClientPath + @"\cache\tabs.celestia", json);

                var windowjson = JsonConvert.SerializeObject(new
                {
                    Height = Height,
                    Width = Width,
                    PositionX = Left,
                    PositionY = Top,
                    ActiveSidebar = SidebarFrame.Content == explorerPanel ? 0 : 1,
                    ActivePanel = ConsoleFrame.Content == output ? 1 : 0,
                    PanelSize = PanelBar.ActualHeight,
                    SidebarWidth = SideBarColumn.ActualWidth,
                    IsSidebarOpen = SideBarColumn.ActualWidth != 0,
                    IsPanelOpen = PanelBar.Visibility != Visibility.Collapsed,
                    EngineButtonX = DraggableEngineButtons.Margin.Left,
                    EngineButtonY = DraggableEngineButtons.Margin.Top
                }, Newtonsoft.Json.Formatting.Indented) ;
                File.WriteAllText(NebulaClientPath + @"\cache\window.celestia", windowjson);

                File.WriteAllText(NebulaClientPath + @"\cache\workspace.celestia", WorkspaceFolder);
                isabletoclose = true;
<<<<<<< HEAD
                await Task.Delay(5);
=======
                await Task.Delay(10);
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                listener.Abort();
                Close();

            };
            PreviewKeyDown += (sender, e) =>
            {
                /*
                 Binding Keys
                */
                if (e.Key == Settings.KeyBinds["SideBarToggle"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ViewMenuButtons(new Button() { Name = "Menus_View_ToggleSidebar" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["PanelToggle"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ViewMenuButtons(new Button() { Name = "Menus_View_TogglePanel" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["NewTextFile"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    FileMenuButtons(new Button() { Name = "Menus_File_NewTxtFile" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["OpenFile"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    FileMenuButtons(new Button() { Name = "Menus_File_OpenFile" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["OpenFolder"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    FileMenuButtons(new Button() { Name = "Menus_File_OpenFolder" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["SaveFile"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    FileMenuButtons(new Button() { Name = "Menus_File_SaveFile" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["Panel_ViewTerminal"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ViewMenuButtons(new Button() { Name = "Menus_View_ViewTerminal" }, new RoutedEventArgs());
                    e.Handled = true;
                }
                if (e.Key == Settings.KeyBinds["Panel_ViewOutput"] && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ViewMenuButtons(new Button() { Name = "Menus_View_ViewOutput" }, new RoutedEventArgs());
                    e.Handled = true;
                }
            };
            //Notify(1, "test!", "Please take note that Celestia and the rest of Nebula Client is currently in its Alpha Stage.\nExpect Bugs, and help report them in the discord server. Thank you.", 5);
            //Notify(2, "test!", "Please take note that Celestia and the rest of Nebula Client is currently in its Alpha Stage.\nExpect Bugs, and help report them in the discord server. Thank you.", 2);
            //Notify(3, "test!", "Please take note that Celestia and the rest of Nebula Client is currently in its Alpha Stage.\nExpect Bugs, and help report them in the discord server. Thank you.", 3);
        }

        #region Window Functionality

        public async void CloseWindow()
        {
            ObjectShift(MainBackgroundBorder, TimeSpan.FromMilliseconds(300), new Thickness(30)).Begin();
            Fade(this, TimeSpan.FromMilliseconds(300), 0).Begin();
            await Task.Delay(1000);
            Close();
        }

        /// <summary>
        /// Sets variable/properties of the customly designed window to mimic actual window functionality/design
        /// </summary>
        /// <param name="os">The OS. 0 == Windows10 1 == Windows11</param>
        /// <param name="isMaximised"></param>
        public void windowstate(int os, bool isMaximised)
        {
            if (isMaximised)
            {
                MainBackgroundBorder.Margin = new Thickness(0);
                MaximiseButton.Content = "\ue923";
                MainBackgroundBorder.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                Topbar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                Activitybar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                PanelBar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                MainBackgroundBorder.BorderThickness = new Thickness(0);
            }
            else
            {
                MaximiseButton.Content = "\ue922";
                MainBackgroundBorder.Margin = new Thickness(15);
                MainBackgroundBorder.BorderThickness = new Thickness(1);
                if (os == 0)
                {
                    MainBackgroundBorder.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                    Topbar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                    Activitybar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                    PanelBar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                }
                else
                {
                    MainBackgroundBorder.CornerRadius = new CornerRadius() { BottomLeft = 8, BottomRight = 8, TopLeft = 8, TopRight = 8 };
                    Topbar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 8, TopRight = 8 };
                    Activitybar.CornerRadius = new CornerRadius() { BottomLeft = 8, BottomRight = 0, TopLeft = 0, TopRight = 0 };
                    PanelBar.CornerRadius = new CornerRadius() { BottomLeft = 0, BottomRight = 8, TopLeft = 0, TopRight = 0 };
                }
            }
        }

        public void drag(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper h = new WindowInteropHelper(this);
            SendMessage(h.Handle, 161, 2, 0);
            e.Handled = true;
            windowstate(IsWindows10() ? 0 : 1, WindowState == WindowState.Maximized);
        }

        private void MaximiseButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                windowstate(IsWindows10() ? 0 : 1, WindowState == WindowState.Maximized);
            }
            else
            {
                WindowState = WindowState.Normal;
                windowstate(IsWindows10() ? 0 : 1, WindowState == WindowState.Maximized);
            }
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => CloseWindow();

        private void Activitybar_Explorer(object sender, RoutedEventArgs e)
        {
            SidebarFrame.Content = explorerPanel;
            Storyboard anim = ObjectShift(NavIndicator, tenthsecond, new Thickness(.5, 10, 0, 0));
            ActivitybarButtons_Explorer.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(tenthsecond, (Color)ColorConverter.ConvertFromString("#d5d3d7")));
            ActivitybarButtons_SourceControl.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(hunsecond, (Color)ColorConverter.ConvertFromString("#A4A6AA")));
            anim.Begin();
        }
        private void Activitybar_SourceControl(object sender, RoutedEventArgs e)
        {
            SidebarFrame.Content = sourceControl;
            Storyboard anim = ObjectShift(NavIndicator, tenthsecond, new Thickness(.5, 60, 0, 0));
            ActivitybarButtons_SourceControl.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(tenthsecond, (Color)ColorConverter.ConvertFromString("#d5d3d7")));
            ActivitybarButtons_Explorer.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(hunsecond, (Color)ColorConverter.ConvertFromString("#A4A6AA")));
            anim.Begin();
        }
        private void Activitybar_ScriptCloud(object sender, RoutedEventArgs e)
        {
            ActivitybarButtons_ScriptCloud.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(tenthsecond, (Color)ColorConverter.ConvertFromString("#d5d3d7")));
            DialogHost.Children.Add(scriptCloud);
            DialogBackground.Visibility = Visibility.Visible;
            Fade(DialogBackground, tenthsecond, 1).Begin();
        }
        private void Activitybar_InstanceManager(object sender, RoutedEventArgs e)
        {
            ActivitybarButtons_InstanceManager.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(tenthsecond, (Color)ColorConverter.ConvertFromString("#d5d3d7")));
            DialogHost.Children.Add(instanceManager);
            ClientManagerNotification.Visibility = Visibility.Collapsed;
            DialogBackground.Visibility = Visibility.Visible;
            Fade(DialogBackground, tenthsecond, 1).Begin();
        }
        private void Activitybar_ExtensionMenu(object sender, RoutedEventArgs e)
        {

        }
        private void Activitybar_Settings(object sender, RoutedEventArgs e)
        {
            ActivitybarButtons_Settings.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(tenthsecond, (Color)ColorConverter.ConvertFromString("#d5d3d7")));
            DialogHost.Children.Add(settings);
            DialogBackground.Visibility = Visibility.Visible;
            Fade(DialogBackground, tenthsecond, 1).Begin();
        }

        private void TerminalToggle(object sender, RoutedEventArgs e)
        {
            ConsoleFrame.Content = terminal;
        }

        private void OutputToggle(object sender, RoutedEventArgs e)
        {
            ConsoleFrame.Content = output;
        }

        private void ClearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            output.Main.Items.Clear();
        }

        private async void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            string CurrentLog = "";
            foreach (dynamic item in output.Main.Items.Cast<dynamic>().ToList())
            {
                string OutputLine = item.Time + " " + item.Type + " " + item.Message + "\n";
                CurrentLog = CurrentLog + OutputLine;
            }
            try
            {
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                {
                    FileName = "Celestia Output Log",
                    Title = "Nebula Client - Celestia IDE | File Manager - Save Output to...",
                    Filter = "Log/Text Files (*.txt;*.celestia)|*.txt;*celestia|All files (*.*)|*.*",
                    RestoreDirectory = true,
                };

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, CurrentLog);
                }
            }
            catch (Exception ex)
            {
                ApplicationPrint(2, "Error: " + ex.Message);
                await Prompt("Error Saving Output Log to File", "File Manager");
            }
        }

<<<<<<< HEAD
        private async void CopyOutputButton_Click(object sender, RoutedEventArgs e)
        {
            string CurrentLog = "";
            foreach (dynamic item in output.Main.Items.Cast<dynamic>().ToList())
            {
                string OutputLine = item.Time + " " + item.Type + " " + item.Message + "\n";
                CurrentLog = CurrentLog + OutputLine;
            }
            try
            {
                Clipboard.SetText(CurrentLog);
            }
            catch (Exception ex)
            {
                ApplicationPrint(2, "Error: " + ex.Message);
                await Prompt("Error Saving Output Log to Clipboard", "File Manager");
            }
        }

=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
        // SECTION : Layout Resizers

        /*
         Variables
        */
        bool _resizing;
        double _startScreenY;
        double _startHeight;
        const double ResizeGrip = 4;
        double? _snapLock = null;
        const double SnapThreshold = 1;   // how close to snap before locking
<<<<<<< HEAD
        const double SnapRelease = 40;     // how far to pull before unlocking
=======
        const double SnapRelease = 60;     // how far to pull before unlocking
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e

        /// <summary>
        /// If value is within *SnapRelease* +/- to any number within *SnapPoints*, value will not change
        /// </summary>
        /// <param name="value"></param>
        /// <param name="SnapPoints"></param>
        /// <returns></returns>
        double MagneticSnap(double value, double[] SnapPoints)
        {
            double nearest = SnapPoints
                .OrderBy(p => Math.Abs(p - value))
                .First();

            if (_snapLock.HasValue)
            {
                if (Math.Abs(value - _snapLock.Value) >= SnapRelease)
                    _snapLock = null;
                else
                    return _snapLock.Value;
            }

            if (Math.Abs(value - nearest) <= SnapThreshold)
            {
                _snapLock = nearest;
                return nearest;
            }

            return value;
        }

        void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_resizing) return;

            double currentY = e.GetPosition(this).Y;
            double delta = _startScreenY - currentY;

            double newHeight = _startHeight + delta;
            PanelBar.Height = Clamp(MagneticSnap(newHeight, new double[] { 240 }), 60, 350) ;

            TabSystemz.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height);
<<<<<<< HEAD
            HomeTabPage.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height);
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
        }

        // element-level hover cursor
        void PanelBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing) return;

            var pos = e.GetPosition(PanelBar);

            if (pos.Y <= ResizeGrip)
                PanelBar.Cursor = Cursors.SizeNS;
            else
                PanelBar.Cursor = null;
        }

        void PanelBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PanelBar);

            if (pos.Y <= ResizeGrip)
            {
                _resizing = true;
                _startScreenY = e.GetPosition(this).Y;
                _startHeight = PanelBar.ActualHeight;

                PanelBar.CaptureMouse();
                Mouse.OverrideCursor = Cursors.SizeNS;

                // attach window-level handlers
                PreviewMouseMove += Window_MouseMove;
                PreviewMouseLeftButtonUp += Window_MouseUp;

                e.Handled = true;
            }
        }

        void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_resizing) return;

            _snapLock = null;
            _resizing = false;

            PanelBar.ReleaseMouseCapture();
            Mouse.OverrideCursor = null;

            PreviewMouseMove -= Window_MouseMove;
            PreviewMouseLeftButtonUp -= Window_MouseUp;
        }

        void PanelBar_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_resizing)
                Mouse.OverrideCursor = null;
        }

        bool _resizingCol;
        double _startMouseX;
        double _startColWidth;

        void Sidebar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizingCol) return;

            var pos = e.GetPosition(Sidebar);

            if (pos.X >= Sidebar.ActualWidth - ResizeGrip)
                Mouse.OverrideCursor = Cursors.SizeWE;
            else
                Mouse.OverrideCursor = null;
        }

        void Sidebar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Sidebar);

            if (pos.X < Sidebar.ActualWidth - ResizeGrip)
                return;

            if (pos.X >= Sidebar.ActualWidth - ResizeGrip || pos.X <= Sidebar.ActualWidth + ResizeGrip)
            {
                _resizingCol = true;
                _startMouseX = e.GetPosition(this).X;
                _startColWidth = SideBarColumn.ActualWidth;

                Mouse.OverrideCursor = Cursors.SizeWE;

                SidebarFrame.IsHitTestVisible = false;
                PreviewMouseMove += SidebarResize_MouseMove;
                PreviewMouseLeftButtonUp += SidebarResize_MouseUp;

                e.Handled = true;
            }
        }

        void SidebarResize_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_resizingCol) return;

            double currentX = e.GetPosition(this).X;
            double delta = currentX - _startMouseX;

            double newWidth = _startColWidth + delta;

            SideBarColumn.Width = new GridLength(Clamp(MagneticSnap(newWidth, new double[] { 220 }), 120, 600), GridUnitType.Pixel);
        }

        void SidebarResize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_resizingCol) return;

            _resizingCol = false;
            _snapLock = null;

            Mouse.OverrideCursor = null;

            SidebarFrame.IsHitTestVisible = true;
            PreviewMouseMove -= SidebarResize_MouseMove;
            PreviewMouseLeftButtonUp -= SidebarResize_MouseUp;
        }

        void Sidebar_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_resizingCol)
                Mouse.OverrideCursor = null;
        }
        // ENDSECTION

        public void DialogBackground_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard fadeout = Fade(DialogBackground, tenthsecond);
            fadeout.Completed += delegate
            {
                DialogBackground.Visibility = Visibility.Collapsed;
            };
            fadeout.Begin();
            foreach (object popup in DialogHost.Children.Cast<Visual>().ToList())
            {
                if (popup is Popup)
                {
                    Storyboard fadeout2 = Fade((Popup)popup, tenthsecond);
                    fadeout2.Begin();
                }
                if (popup is ScriptCloud)
                {
                    ActivitybarButtons_ScriptCloud.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(hunsecond, (Color)ColorConverter.ConvertFromString("#A4A6AA")));
                    DialogHost.Children.Remove((ScriptCloud)popup);
                }
                if (popup is InstanceManager)
                {
                    ActivitybarButtons_InstanceManager.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(hunsecond, (Color)ColorConverter.ConvertFromString("#A4A6AA")));
                    DialogHost.Children.Remove((InstanceManager)popup);
                }
                if (popup is SettingsPage)
                {
                    ActivitybarButtons_Settings.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ColorShift(hunsecond, (Color)ColorConverter.ConvertFromString("#A4A6AA")));
                    DialogHost.Children.Remove((SettingsPage)popup);
                }
            }
        }

        void HideMenus(object a = null, object b = null)
        {
            foreach (Border contextmenu in WindowMenus.Children)
            {
                contextmenu.Visibility = Visibility.Collapsed;
            }
        }

        private void MenuButtons(object sender, RoutedEventArgs e)
        {
            HideMenus();
            switch (((Button)sender).Name)
            {
                case "FileMenuButton":
                    FileContextMenu.Visibility = Visibility.Visible;
                    FileContextMenu.Margin = new Thickness(TranslatePoint(Mouse.GetPosition(this), this).X - 30, TranslatePoint(Mouse.GetPosition(this), this).Y - 10, 0, 0);
                    break;
                case "EditMenuButton":
                    EditContextMenu.Visibility = Visibility.Visible;
                    EditContextMenu.Margin = new Thickness(TranslatePoint(Mouse.GetPosition(this), this).X - 30, TranslatePoint(Mouse.GetPosition(this), this).Y - 10, 0, 0);
                    break;
                case "ViewMenuButton":
                    ViewContextMenu.Visibility = Visibility.Visible;
                    ViewContextMenu.Margin = new Thickness(TranslatePoint(Mouse.GetPosition(this), this).X - 30, TranslatePoint(Mouse.GetPosition(this), this).Y - 10, 0, 0);
                    break;
                case "TerminalMenuButton":
                    PanelBar.Visibility = Visibility.Visible;
                    ViewMenuButtons(new Button() { Name = "Menus_View_ViewTerminal" }, new RoutedEventArgs());
                    TabSystemz.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height);
<<<<<<< HEAD
                    HomeTabPage.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height);
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                    break;
                case "HelpMenuButton":
                    HelpContextMenu.Visibility = Visibility.Visible;
                    HelpContextMenu.Margin = new Thickness(TranslatePoint(Mouse.GetPosition(this), this).X - 30, TranslatePoint(Mouse.GetPosition(this), this).Y - 10, 0, 0);
                    break;
            }
        }

        private async void FileMenuButtons(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "Menus_File_NewTxtFile":
                    TabSystemz.ButtonTabs(new Button() { Name = "AddTabButton" }, new RoutedEventArgs());
                    break;
                case "Menus_File_NewFile":
                    explorerPanel.NewFile_Click(new Button(), new RoutedEventArgs());
                    break;
                case "Menus_File_OpenFile":
                    try
                    {
                        var openFileDialog = new System.Windows.Forms.OpenFileDialog
                        {
                            Title = "Nebula Client - Celestia IDE | File Manager - Import Into Editor",
                            Filter = "All files (*.*)|*.*|Text Files (*.txt)|*.txt;|Lua Files (*.lua;*.luau)|*.lua;*.luau",
                            Multiselect = false,
                            RestoreDirectory = true,
                        };
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            TabSystemz.add_tab_from_file(openFileDialog.FileName);
                        //ApplicationPrint(1, openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        ApplicationPrint(2, "Error: " + ex.Message);
                        await Prompt("Error Importing File Contents Into Editor", "File Manager");
                    }
                    break;
                case "Menus_File_OpenFolder":
                    try
                    {
                        var openFolderDialog = new BetterFolderBrowser
                        {
                            Title = "Nebula Client - Celestia IDE | File Manager - Open Workspace",
                            Multiselect = false,
                        };
                        if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            WorkspaceFolder = openFolderDialog.SelectedFolder;
                        //ApplicationPrint(1, openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        ApplicationPrint(2, "Error: " + ex.Message);
                        await Prompt("Error Opening Folder", "File Manager");
                    }
                    break;
                case "Menus_File_ClearFolder":
                    try
                    {
                        WorkspaceFolder = "";
                    }
                    catch (Exception ex)
                    {
                        ApplicationPrint(2, "Error: " + ex.Message);
                        await Prompt("Error Clearing Folder", "File Manager");
                    }
                    break;
                case "Menus_File_SaveFile":
                    try
                    {
                        TabItem currentTab = (TabItem)TabSystemz.maintabs.SelectedItem;
                        if (currentTab == null) return;
                        if (currentTab.Tag == null) { FileMenuButtons(new Button() { Name = "Menus_File_SaveFileAs" }, new RoutedEventArgs()); return; }

                        File.WriteAllText((string)currentTab.Tag, await TabSystemz.current_monaco().GetText());
                    }
                    catch (Exception ex)
                    {
                        ApplicationPrint(2, "Error: " + ex.Message);
                        await Prompt("Error Saving Editor Contents To File", "File Manager");
                    }
                    break;
                case "Menus_File_SaveFileAs":
                    try
                    {
                        var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                        {
                            FileName = await TabSystemz.GetCurrentTabTitle(),
                            Title = "Nebula Client - Celestia IDE | File Manager - Save File As...",
                            Filter = "Script Files (*.txt;*.lua;*.luau)|*.txt;*.lua;*.luau|All files (*.*)|*.*",
                            RestoreDirectory = true,
                        };

                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            File.WriteAllText(saveFileDialog.FileName, await TabSystemz.current_monaco().GetText());
                            TabItem currentTab = (TabItem)TabSystemz.maintabs.SelectedItem;
                            currentTab.Tag = saveFileDialog.FileName;
                            ((TextBox)currentTab.Header).Text = Path.GetFileName(saveFileDialog.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationPrint(2, "Error: " + ex.Message);
                        await Prompt("Error Saving Editor Contents To File", "File Manager");
                    }
                    break;
                case "Menus_File_SaveAllFiles":
                    try
                    {
                        foreach (TabItem currentTab in TabSystemz.maintabs.Items)
                        {
                            if (currentTab.Tag != null)
                            {
                                File.WriteAllText((string)currentTab.Tag, await ((monaco_api)currentTab.Content).GetText());
                            }
                            else
                            {
                                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                                {
                                    FileName = ((TextBox)currentTab.Header).Text,
                                    Title = "Nebula Client - Celestia IDE | File Manager - Save File As...",
                                    Filter = "Script Files (*.txt;*.lua;*.luau)|*.txt;*.lua;*.luau|All files (*.*)|*.*",
                                    RestoreDirectory = true,
                                };

                                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    File.WriteAllText(saveFileDialog.FileName, await ((monaco_api)currentTab.Content).GetText());
                                    currentTab.Tag = saveFileDialog.FileName;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationPrint(2, "Error: " + ex.Message);
                        await Prompt("Error Saving Editor Contents To File(s)" + ex.Message, "File Manager");
                    }
                    break;
            }
            HideMenus();
        }

        private void EditMenuButtons(object sender, RoutedEventArgs e)
        {
            HideMenus();
            if (TabSystemz.current_monaco() == null) return;
            switch (((Button)sender).Name)
            {
                case "Menus_Edit_Cut":
                    TabSystemz.current_monaco().Cut();
                    break;
                case "Menus_Edit_Copy":
                    TabSystemz.current_monaco().Copy();
                    break;
                case "Menus_Edit_Paste":
                    TabSystemz.current_monaco().Paste();
                    break;
                case "Menus_Edit_Undo":
                    TabSystemz.current_monaco().Undo();
                    break;
                case "Menus_Edit_Redo":
                    TabSystemz.current_monaco().Redo();
                    break;
                case "Menus_Edit_Clear":
                    TabSystemz.Clear_Editor(new object(), new RoutedEventArgs());
                    break;
                case "Menus_Edit_Find":
                    TabSystemz.current_monaco().Find();
                    break;
                case "Menus_Edit_Replace":
                    TabSystemz.current_monaco().Replace();
                    break;
                case "Menus_Edit_ToggleBlockComment":
                    TabSystemz.current_monaco().BlockC();
                    break;
                case "Menus_Edit_ToggleLineComment":
                    TabSystemz.current_monaco().LineC();
                    break;
            }
        }

        public double currentSideBarSize = 220;
        private void ViewMenuButtons(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "Menus_View_TogglePanel":
                    PanelBar.Visibility = PanelBar.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                    if (PanelBar.IsVisible) TabSystemz.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height); else TabSystemz.Margin = new Thickness(0, 0, 0, 0);
<<<<<<< HEAD
                    if (PanelBar.IsVisible) HomeTabPage.Margin = new Thickness(0, 0, 0, 0 + PanelBar.Height); else TabSystemz.Margin = new Thickness(0, 0, 0, 0);
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                    break;
                case "Menus_View_ToggleSidebar":
                    if (SideBarColumn.ActualWidth == 0)
                    {
                        SideBarColumn.Width = new GridLength(currentSideBarSize);
                    }
                    else
                    {
                        currentSideBarSize = SideBarColumn.Width.Value;
                        SideBarColumn.Width = new GridLength(0);
                    }
                    break;
                case "Menus_View_ViewExplorer":
                    Activitybar_Explorer(new object(), new RoutedEventArgs());
                    break;
                case "Menus_View_ViewSourceControl":
                    Activitybar_SourceControl(new object(), new RoutedEventArgs());
                    break;
                case "Menus_View_ViewTerminal":
                    TerminalCheck.IsChecked = true;
                    TerminalToggle(new object(), new RoutedEventArgs());
                    break;
                case "Menus_View_ViewOutput":
                    OutputCheck.IsChecked = true;
                    OutputToggle(new object(), new RoutedEventArgs());
                    break;
            }
            HideMenus();
        }

        private void HelpMenuButtons(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "Menus_Help_Discord":
                    discordjoin();
                    break;
                case "Menus_Help_Documentation":
                    Redirect("https://docs." + Website);
                    break;
            }
            HideMenus();
        }

        private void ContextMenuDown(object sender, MouseEventArgs e) => _contextmenuover = true;
        private void ContextMenuUp(object sender, MouseEventArgs e) => _contextmenuover = false;

        /*
         Dragging Mechanic of the Engine Buttons
         */
        public bool isDragging;
        private bool _mouseInside;
        private Point _mouseDownPos;
        private Point _lastMousePos;
        private bool _xLocked;
        private bool _yLocked;
        private void EngineDragger_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            _mouseInside = true;

            _mouseDownPos = e.GetPosition(this);
            _lastMousePos = _mouseDownPos;

            _xLocked = false;
            _yLocked = false;

            EngineDraggerButton.CaptureMouse();
        }
        private void Dragger_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            if (!_mouseInside)
            {
                return;
            }

            Point mouse = e.GetPosition(this);
            Vector delta = mouse - _lastMousePos;


            var border = DraggableEngineButtons;
            double left = border.Margin.Left;
            double top = border.Margin.Top;

            if (!_xLocked)
            {
                double nextX = left + delta.X;
                double clampedX = Clamp(nextX, 15, ActualWidth - 42 - border.ActualWidth);

                if (nextX != clampedX)
                    _xLocked = true;
                else
                    left = clampedX;
            }
            else
            {
                if ((delta.X < 0 && mouse.X <= _mouseDownPos.X) ||
                    (delta.X > 0 && mouse.X >= _mouseDownPos.X))
                {
                    _xLocked = false;
                }
            }
            if (!_yLocked)
            {
                double nextY = top + delta.Y;
                double clampedY = Clamp(nextY, 15, ActualHeight - 45 - border.ActualHeight);

                if (nextY != clampedY)
                    _yLocked = true;
                else
                    top = clampedY;
            }
            else
            {
                if ((delta.Y < 0 && mouse.Y <= _mouseDownPos.Y) ||
                    (delta.Y > 0 && mouse.Y >= _mouseDownPos.Y))
                {
                    _yLocked = false;
                }
            }

            border.Margin = new Thickness(left, top, 0, 0);
            _lastMousePos = mouse;
        }

        #endregion

        #region Execution/NBT Linkup

        Dictionary<int, int> PortToProcess = new();
        Dictionary<int, InstanceControl> ProcessToUI = new();
        Dictionary<int, WebSocket> PortToSocket = new();
        HttpListener listener;
<<<<<<< HEAD
        Process? engineProcess;
        NamedPipeClientStream pipe;
        bool isPipeConnected = false;
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e

        /// <summary>
        /// Boots up the Engine communication and DLL Systems
        /// </summary>
<<<<<<< HEAD
        public async void InitializeEngine()
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


                try
                {
                    _ = ReadLoop();
                }
                catch { }
            }
            catch {
            
            }
=======
        public void InitializeEngine()
        {
            try
            {
                quorum.StartCommunication();
                QuorumAPI.QuorumModule._AutoUpdateLogs = false;
                QuorumAPI.QuorumModule.ExecutorInfo.Name = "Nebula Client";
                QuorumAPI.QuorumModule.ExecutorInfo.Ver = "v0.1.3a";
                QuorumAPI.QuorumModule.ExecutorInfo.CUA = "Nebula Trinity Engine/v0.1.3a";
            }
            catch { }
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
            StartWebSocketServer();
        }

        /// <summary>
<<<<<<< HEAD
        /// handles whenever messages are received for stuff like success and failure
        /// </summary>
        /// <returns></returns>
        async Task ReadLoop()
        {
            var reader = new StreamReader(pipe);

            while (pipe.IsConnected)
            {
                try
                {
                    var msg = await reader.ReadLineAsync();
                    if (msg == null) continue;

                    var split = msg.Split(new[] { ';' }, 2);
                    if (split.Length < 2) continue;

                    string type = split[0];
                    string data = split[1];
                    Process process = Process.GetProcessById(Convert.ToInt32(data));
                    switch (new string(type.Where(c => char.IsLetter(c) && char.IsUpper(c)).ToArray()))
                    {
                        case "SUCCESSINJECT":
                            try
                            {
                                InstanceControl instanceBtn = null;
                                Dispatcher.Invoke(() =>
                                {
                                    instanceBtn = new InstanceControl();
                                    DraggableEngineButtons.Visibility = Visibility.Visible;
                                    instanceBtn.TitleBlock.Text = "Client " + process.Id.ToString();
                                    instanceBtn.ContentBlock.Text = "Pending Game Join, please join a game first.";
                                    instanceBtn.KillButton.Click += delegate
                                    {
                                        process.Kill();
                                    };
                                    instanceManager.Instances.Children.Add(instanceBtn);
                                    ApplicationPrint(4, $"Sucessfully Injected into Process {process.Id}");
                                    Notify(3, "Injected!", $"Nebula Client has sucessfully injected into Process {process.Id}", 4);
                                    ClientManagerNotification.Visibility = Visibility.Visible;

                                    ProcessToUI[process.Id] = instanceBtn;
                                });

                                [DllImport("user32.dll", SetLastError = true)]
                                static extern bool SetWindowText(IntPtr hWnd, string lpString);

                                process.EnableRaisingEvents = true;
                                process.Exited += (s, e) =>
                                {
                                    int pid;
                                    try
                                    {
                                        pid = process.Id;
                                    }
                                    catch
                                    {
                                        return;
                                    }

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        try
                                        {
                                            instanceManager.Instances.Children.Remove(instanceBtn);
                                            bool robloxAlive = false;
                                            try
                                            {
                                                robloxAlive = Process.GetProcessesByName("RobloxPlayerBeta").Any();
                                            }
                                            catch { }

                                            if (!robloxAlive)
                                            {
                                                DraggableEngineButtons.Visibility = Visibility.Collapsed;
                                                ClientManagerNotification.Visibility = Visibility.Collapsed;
                                            }
                                            ProcessToUI.Remove(pid);
                                        }
                                        catch
                                        {
                                            ApplicationPrint(2, "failed to handle killed roblox process");
                                        }
                                    }));
                                };
                                if (Settings.UseRamLimit) JobLimits.LimitMemory(process, Settings.RamLimit);

                                await Task.Delay(3000);

                                IntPtr hwnd = WindowUtils.GetMainWindowHandle(process.Id);
                                if (hwnd != IntPtr.Zero)
                                {
                                    SetWindowText(hwnd, "Roblox Game Client " + process.Id.ToString());
                                    SetWindowText(hwnd, "Roblox Game Client " + process.Id.ToString());
                                }
                                SetWindowText(hwnd, "Roblox Game Client " + process.Id.ToString());
                            }
                            catch (Exception ex)
                            {
                                Dispatcher.Invoke(delegate
                                {
                                    ApplicationPrint(2, $"error: {ex.Message}\n{ex.StackTrace}");
                                });
                            }
                            break;
                        case "FAILEDINJECT":
                            Dispatcher.Invoke(delegate
                            {
                                ApplicationPrint(3, $"Failed to Inject into {process.Id}");
                                Notify(4, "Failed to Inject", $"Nebula Trinity Engine has failed to inject into {process.Id}!", 4);
                            });
                            break;
                    }


                }

                catch (Exception ex)
                {
                    Dispatcher.Invoke(delegate
                    {
                        ApplicationPrint(2, $"Failed to Inject: " + ex.Message + "\n" + ex.StackTrace);
                        Notify(2, "Failed to Inject", $"Nebula Trinity Engine has failed to inject !", 4);
                    });
                }
            }
        }

        /// <summary>
        /// handles sending messages to the engine for stuff liike injecting and executing
        /// </summary>
        /// <param name="msg">message to send. prefix with the type of the message and semicolon</param>
        /// <returns></returns>
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

                Dispatcher.Invoke(delegate
                {
                    ApplicationPrint(2, $"failed into Process {msg}: {ex.Message}\n{ex.StackTrace}");
                });
            }
        }

        /// <summary>
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
        /// Begins WebSocket listening for the universal/main WebSocket
        /// </summary>
        async void StartWebSocketServer()
        {
            listener = new();
            listener.Prefixes.Add("http://localhost:31425/");
            listener.Start();

            while (true && listener.IsListening)
            {
                try
                {
                    var ctx = await listener.GetContextAsync();
                    var ws = (await ctx.AcceptWebSocketAsync(null)).WebSocket;
                    await HandleSocket(ws);
                } catch { }
            }
        }

        /// <summary>
        /// Unbinds a Roblox Process from the given Web Socket (typically on game leave) to allow for a new
        /// WebSocket, allowing for smooth and reliable communication.
        /// </summary>
        /// <param name="port"></param>
        void HandleInstanceDisconnect(int port)
        {
            if (PortToSocket.TryGetValue(port, out var ws))
            {
                try
                {
                    ws.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Roblox left game",
                        CancellationToken.None
                    ).Wait();
                }
                catch { }
            }
            if (PortToProcess.TryGetValue(port, out int pid)) ProcessToUI.Remove(pid);

            try
            {
                PortToSocket.Remove(port);
                PortToProcess.Remove(port);
            }
            catch {

            }

            ApplicationPrint(1, $"Instance socket {port} disconnected");
        }

        /// <summary>
        /// Checks the received message from *ws* and handles it based on the received command.
        /// </summary>
        /// <param name="ws">The WebSocket to check the most recent message</param>
        /// <returns></returns>
        async Task HandleSocket(WebSocket ws)
        {
            var buf = new byte[1024];

            var res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);

            var raw = Encoding.UTF8.GetString(buf, 0, res.Count);
            var msg = raw.Trim('\0', '\r', '\n', ' ');

            if (msg.StartsWith("CONNECT;"))
            {
                int port = int.Parse(msg.Split(';')[1]);

                int pid = ProcessToUI.Keys
                    .First(p => !PortToProcess.ContainsValue(p));

                PortToProcess[port] = pid;

                StartInstanceServer(port);
            }
            else if (msg.StartsWith("DISCONNECT;"))
            {
                HandleInstanceDisconnect(int.Parse(msg.Split(';')[1]));
                return;
            }
        }

        /// <summary>
        /// Creates a new Instance WebSocket with *port* as the port to be binded to a Roblox Process, allowing easy
        /// and external communication of Process to WebSocket Messages
        /// </summary>
        /// <param name="port"></param>
        async void StartInstanceServer(int port)
        {
            //ApplicationPrint(1, "instance server started" + port.ToString());
            HttpListener listener = new();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            var ctx = await listener.GetContextAsync();
            var ws = (await ctx.AcceptWebSocketAsync(null)).WebSocket;

            var buf = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
                    var msg = Encoding.UTF8.GetString(buf, 0, res.Count);


                    var split = msg.Split(new[] { ';' }, 2);
                    if (split.Length < 2) continue;

                    string type = split[0];
                    string data = split[1];
                    //ApplicationPrint(1, "WebSocket is open");

                    if (!PortToProcess.TryGetValue(port, out int pid)) continue;
                    if (!ProcessToUI.TryGetValue(pid, out var ui)) continue;
                    //ApplicationPrint(1, "ready, finished continue checks");

                    Dispatcher.Invoke(() =>
                    {
                        switch (type)
                        {
                            case "INFO":
                                ui.ContentBlock.Text = data;
                                //ApplicationPrint(1, data);
                                break;
                            case "PRINT":
                                ApplicationPrint(6, "[ROBLOX " + pid.ToString() + "] " + data);
                                break;
                            case "WARN":
                                ApplicationPrint(3, "[ROBLOX " + pid.ToString() + "] " + data);
                                break;
                            case "ERROR":
                                ApplicationPrint(2, "[ROBLOX " + pid.ToString() + "] " + data);
                                break;
                            case "MSGBOX":
                                var split = msg.Split(new[] { ';' }, 3);
                                Prompt(split[1].ToString(), split[2].ToString());
                                break;
                        }
                    });
                }
                catch { }
            }
            //ApplicationPrint(1, "port closed " + port.ToString());

            try { PortToProcess.Remove(port); } catch { }
        }

        /// <summary>
        /// Tells Nebula Trinity Engine to inject into a process with returned states and
        /// Creates a new Instance Control/Execute Selector for that Process in the instance manager.
        /// </summary>
        /// <param name="process"></param>
        public async void newProcess(Process process)
        {
<<<<<<< HEAD

=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
            await Task.Run(async delegate
            {
                Dispatcher.Invoke(delegate
                {
                    ApplicationPrint(6, $"Injecting into Process {process.Id}");
                });
<<<<<<< HEAD
                try
                {
                    await Send($"INJECT;{process.Id}");
                }
                catch (Exception ex)
                {

                    Dispatcher.Invoke(delegate
                    {
                        ApplicationPrint(2, $"failed into Process {process.Id}: {ex.Message}\n{ex.StackTrace}");
                    });
                }

=======
                if (await quorum.Attach(process.Id, true) == QuorumAPI.QuorumStates.Attached)
                {
                    if (quorum.IsPIDAttached(process.Id))
                    {
                        InstanceControl instanceBtn = null;
                        Dispatcher.Invoke(() =>
                        {
                            instanceBtn = new InstanceControl();
                            DraggableEngineButtons.Visibility = Visibility.Visible;
                            instanceBtn.TitleBlock.Text = "Client " + process.Id.ToString();
                            instanceBtn.ContentBlock.Text = "Pending Game Join, please join a game first.";
                            instanceBtn.KillButton.Click += delegate
                            {
                                process.Kill();
                            };
                            instanceManager.Instances.Children.Add(instanceBtn);
                            ApplicationPrint(4, $"Sucessfully Injected into Process {process.Id}");
                            Notify(3, "Injected!", $"Nebula Client has sucessfully injected into Process {process.Id}", 4);
                            ClientManagerNotification.Visibility = Visibility.Visible;

                            ProcessToUI[process.Id] = instanceBtn;
                        });

                        [DllImport("user32.dll", SetLastError = true)]
                        static extern bool SetWindowText(IntPtr hWnd, string lpString);

                        process.EnableRaisingEvents = true;
                        process.Exited += (s, e) =>
                        {
                            int pid;
                            try
                            {
                                pid = process.Id;
                            }
                            catch
                            {
                                return;
                            }

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    instanceManager.Instances.Children.Remove(instanceBtn);
                                    bool robloxAlive = false;
                                    try
                                    {
                                        robloxAlive = Process.GetProcessesByName("RobloxPlayerBeta").Any();
                                    }
                                    catch { }

                                    if (!robloxAlive)
                                    {
                                        DraggableEngineButtons.Visibility = Visibility.Collapsed;
                                        ClientManagerNotification.Visibility = Visibility.Collapsed;
                                    }
                                    ProcessToUI.Remove(pid);
                                }
                                catch
                                {
                                    ApplicationPrint(2, "failed to handle killed roblox process");
                                }
                            }));
                        };
                        if (Settings.UseRamLimit) JobLimits.LimitMemory(process, Settings.RamLimit);


                        IntPtr hwnd = WindowUtils.GetMainWindowHandle(process.Id);
                        if (hwnd != IntPtr.Zero)
                        {
                            SetWindowText(hwnd, process.Id.ToString());
                            SetWindowText(hwnd, process.Id.ToString());
                        }
                        SetWindowText(hwnd, process.Id.ToString());
                    }
                    else
                    {
                        ApplicationPrint(2, $"Failed to Inject into Process {process.Id}");
                    }
                }
                else
                {
                    ApplicationPrint(2, $"Failed to Inject into Process {process.Id}");
                }
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e

                //checkruntimefile();
            });
        }

        /// <summary>
        /// Begins the watcher for new processes, and if it is Roblox,
        /// it will call *newProcess* on it
        /// </summary>
        public async void StartProcessCheck()
        {
            await Task.Run(delegate
            {
                foreach (Process boblox in Process.GetProcessesByName("RobloxPlayerBeta"))
                {
                    newProcess(boblox);
                }
                try
                {
                    var query = new WqlEventQuery(
                        "SELECT * FROM Win32_ProcessStartTrace"
                    );

                    var watcher = new ManagementEventWatcher(query);
                    watcher.EventArrived += (_, e) =>
                    {
                        Dispatcher.Invoke(delegate
                        {
                            //ApplicationPrint(1, "new app");
                            string name = e.NewEvent.Properties["ProcessName"]?.Value?.ToString();
                            int pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                            //ApplicationPrint(1, name);
                            if (name == "RobloxPlayerBeta.exe")
                            {
                                //ApplicationPrint(1, "passed check");
                                newProcess(Process.GetProcessById(pid));
                                //ApplicationPrint(1, pid.ToString());
                                //ApplicationPrint(1, Process.GetProcessById(pid).ToString());
                            }
                        });
                    };

                    watcher.Start();
                }
                catch { }
            });

        }

        /// <summary>
        /// Executes the provided script to selected processes via Nebula Trinity Engine
        /// </summary>
        /// <param name="Script">The script to execute</param>
        public void ExecuteScript(string Script)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\AutoExec\runtimes.txt"))
            {
                ApplicationPrint(2, "deleted runtimes");
                return;
            }

            foreach (InstanceControl instanceBtn in instanceManager.Instances.Children.Cast<Visual>().ToList())
            {
                if (instanceBtn.isSelectedCheckbox.IsChecked == true)
                {
                    try
                    {
                        int pid = int.Parse(instanceBtn.TitleBlock.Text.Substring(7));
<<<<<<< HEAD
                        Send($"EXECUTE;{pid};{Script}");
=======
                        try { if (!quorum.IsPIDAttached(pid)) quorum.Attach(pid, true); } catch { }
                        quorum.Execute(pid, "if not getgenv().IsNebulaTrinityEngineReady then \n\terror('not loaded')\n\treturn\nend \n\n" + Script);
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
                    }
                    catch { }
                }
            }
        }

        public async void ExecuteScriptButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScript(await TabSystemz.current_monaco().GetText());
        }
        public async void ExecuteFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Title = "Nebula Client - Celestia IDE | File Manager - Execute File Content",
                    Filter = "All files (*.*)|*.*|Text Files (*.txt)|*.txt;|Lua Files (*.lua;*.luau)|*.lua;*.luau",
                    Multiselect = false,
                    RestoreDirectory = true,
                };
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ExecuteScript(File.ReadAllText(openFileDialog.FileName));
                }
            }
            catch (Exception ex)
            {
                ApplicationPrint(2, "Error: " + ex.Message);
                await Prompt("Error Executing File Contents", "Nebula Trinity Engine");
            }
        }

        /// <summary>
        /// Checks whether Runtime files are present and not modified.
        /// This is important for reasons.
        /// </summary>
        /// <returns>If all files are correct</returns>
        [Obsolete ("Currently broken and doesnt work")]
        private bool checkruntimefile()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\AutoExec\runtimes.txt"))
            {
                foreach (Process boblox in Process.GetProcessesByName("RobloxPlayerBeta"))
                {
                    boblox.Kill();
                }
                return false;
            }
            else if (File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\AutoExec\runtimes.txt") != HttpGet("https://raw.nebulasoftworks.xyz/nebulaclientruntimes.html"))
            {
                foreach (Process boblox in Process.GetProcessesByName("RobloxPlayerBeta"))
                {
                    boblox.Kill();
                }
                return false;
            }
            return true;
        }
        #endregion

        #region RPC
        public static RPC.DiscordRpcClient client;

        /// <summary>
        /// Default Rich Presence with Constants that wont change
        /// </summary>
        RPC.RichPresence baseRichPresence = new RPC.RichPresence
        {
            Details = "",
            State = "",
            Type = RPC.ActivityType.Playing,

            Assets = new RPC.Assets
            {
                LargeImageKey = "https://raw.nebulasoftworks.xyz/Celestia%20IDE.png",
                LargeImageText = "Celestia IDE",
                SmallImageKey = "https://raw.nebulasoftworks.xyz/Nebula%20Client%20Logo.png",
                SmallImageText = "Nebula Client\nThe Next Generation Of Roblox Exploiting"
            },
            Buttons = new RPC.Button[]
                {
                    new RPC.Button { Label = "Download", Url = $"https://nebulasoftworks.xyz/nc" },
                    new RPC.Button { Label = "GitHub", Url = $"https://github.com/Nebula-Softworks/Nebula-Client" }
                }
        };

        /// <summary>
        /// Initialises the RPC servers and
        /// Adds Event Handlers to Settings.OnUpdate for the RPC Enabled Setting,
        /// Setting it to enabled based on the values.
        /// </summary>
        void BindRPCSettings()
        {
            InitRPC();
            enablerpc();
            Settings.OnUpdate += str =>
            {
                if (str == "RPCEnabled_Setting")
                {
                    if (Settings.DiscordRPCEnabled)
                    {
                        enablerpc();
                    } else
                    {
                        shutdownrpc();
                    }
                }
            };
        }

        /// <summary>
        /// Ends the RPC Connection
        /// </summary>
        public void TerminateConnection()
        {
            client.Dispose();
        }

        /// <summary>
        /// Sets the RPC Details
        /// </summary>
        /// <param name="Details"></param>
        public async void SetRPCDetails(string Details, string state = "Currently Idle")
        {
            if (state == "Currently Idle")
            {
                if (!string.IsNullOrEmpty(WorkspaceFolder))
                    state = "Opened Workspace:\n" + Path.GetFileNameWithoutExtension(WorkspaceFolder);
            }
            baseRichPresence.Details = Details;
            baseRichPresence.State = state;
            if (client.IsInitialized)
            {
                try
                {
                    client.SetPresence(baseRichPresence);
                }
                catch (Exception ex)
                {
                    await Prompt("Celestia could not update your RPC. Error: " + ex.Message + "\nStack Trace: "+ ex.StackTrace, "RPC Error");
                }
            }
        }

        /// <summary>
        /// Initializes the RPC Client
        /// </summary>
        public void InitRPC()
        {
            client = new RPC.DiscordRpcClient("1464843136617681030");
            if (Settings.DiscordRPCEnabled)
            {
                client.Initialize();
            }
            SetRPCDetails($"Using Celestia IDE 0.1.3a");
        }

        /// <summary>
        /// Sets the detail to the string given
        /// </summary>
        /// <param name="currentFile"></param>
        public async void SetRpcFile(string currentFile)
        {
            if (client.IsInitialized)
            {
                try
                {
                    SetRPCDetails($"Using Celestia IDE 0.1.3a", "Editing File: " + currentFile);
                }
                catch (Exception ex)
                {
                    await Prompt("Celestia could not update your RPC. Error: " + ex.Message + "\nStack Trace: " + ex.StackTrace, "RPC Error");
                }
            }
        }

        /// <summary>
        /// Sets the default RPC Strings.
        /// </summary>
        public async void SetBaseRichPresence()
        {
            if (client.IsInitialized)
            {
                try
                {
                    SetRPCDetails($"Using Celestia IDE 0.1.3a");
                }
                catch (Exception ex)
                {
                    await Prompt("Celestia could not update your RPC. Error: " + ex.Message + "\nStack Trace: " + ex.StackTrace, "RPC Error");
                }
            }
        }

        /// <summary>
        /// Enables the Discord RPC for Nebula Client.
        /// </summary>
        public void enablerpc()
        {
            if (!client.IsInitialized)
            {
                InitRPC();
            }
        }

        /// <summary>
        /// Disables the Discord RPC for Nebula Client.
        /// </summary>
        public void shutdownrpc()
        {
            if (client.IsInitialized)
            {
                TerminateConnection();
            }
        }
        #endregion

        #region Settings
        /// <summary>
        /// Sets Initial/Default Settings Values And Creates The Settings Objects
        /// </summary>
        public void CreateSettingsObjects()
        {
            // create the controls, set initial values, set callbacks to change the Settings.cs values

            #region Setting Intitial Values

            BindSettingsToUpdate();
            Settings.TopMost = true;
            Settings.IsOBSHidden = false;
            Settings.UsingTrayIcon = false;
            Settings.Opacity = 1;
            Settings.InterfaceScale = 100/100;
            Settings.InterfaceLanguage = 0;
            Settings.DiscordRPCEnabled = true;
            Settings.StartOnStartup = false;

            Settings.ColorChoices = new Dictionary<string, Color>()
            {
                { "Background", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "Panels", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "Borders", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "InactiveText", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "DarkText", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "ForeText", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "AccentColorOne", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
                { "AccentColorTwo", (Color)new ColorConverter().ConvertFrom("#CF9FFF")},
            };
            Settings.BackgroundPhotoPath = null;

            Settings.FpsUnlock = true;
            Settings.ReplicateFirst = false;
            Settings.UseCpuLimit = false;
            Settings.CpuLimit = 40;
            Settings.UseRamLimit = false;
            Settings.RamLimit = 8192UL * 1024 * 1024;
            Settings.RunAutoExecute = true;

            Settings.Minimap = true;
            Settings.FormatOnSave = true;
            Settings.SaveWorkspaceTabs = true;
            Settings.Ligatures = true;
            Settings.Intellisense = true;
            Settings.AntiSkid = false;
            Settings.FontSize = 14;
            Settings.TextFileHeader = "New Untitled File";
            Settings.AutoFormat = true;
            Settings.InlayHints = true;

            #endregion

            #region Creating Objects
            // Start On Startup and colors and replciate first and cpu limit not done and code default language since only luau atm, ram limits require restarting the process, silently dont implement disable save tabs and font, account js put coming soon

            SettingsToggle TopMostToggle = new SettingsToggle();
            settings.Pages_Interface.Children.Add(TopMostToggle);
            TopMostToggle.TitleBlock.Text = "Topmost";
            TopMostToggle.ContentBlock.Text = "Place Celestia Above All Other Windows Open.\nUseful for constant debugging.";
            TopMostToggle.isSelectedCheckbox.IsChecked = true;
            TopMostToggle.isSelectedCheckbox.Checked += (_, _) =>
                Settings.TopMost = true;
            TopMostToggle.isSelectedCheckbox.Unchecked += (_, _) =>
                Settings.TopMost = false;

            SettingsToggle IsOBSHiddenToggle = new SettingsToggle();
            settings.Pages_Interface.Children.Add(IsOBSHiddenToggle);
            IsOBSHiddenToggle.TitleBlock.Text = "Hide From Capture";
            IsOBSHiddenToggle.ContentBlock.Text = "Hides the Celestia Window from Screen Recording/Capturing Software\nLike OBS. Neat way to look legit.";
            IsOBSHiddenToggle.isSelectedCheckbox.IsChecked = false;
            IsOBSHiddenToggle.isSelectedCheckbox.Checked += (_, _) =>
                Settings.IsOBSHidden = true;
            IsOBSHiddenToggle.isSelectedCheckbox.Unchecked += (_, _) =>
                Settings.IsOBSHidden = false;

            SettingsToggle TrayIconToggle = new SettingsToggle();
            settings.Pages_Interface.Children.Add(TrayIconToggle);
            TrayIconToggle.TitleBlock.Text = "Tray Icon Mode";
            TrayIconToggle.ContentBlock.Text = "Keeps Celestia Running In The Background On Window Close.\nUNIMPLEMENTED.";
            TrayIconToggle.isSelectedCheckbox.IsChecked = false;
            TrayIconToggle.isSelectedCheckbox.Checked += (_, _) =>
                Settings.UsingTrayIcon = true;
            TrayIconToggle.isSelectedCheckbox.Unchecked += (_, _) =>
                Settings.UsingTrayIcon = false;

            SettingsSlider OpacitySlider = new SettingsSlider();
            settings.Pages_Interface.Children.Add(OpacitySlider);
<<<<<<< HEAD
=======
            OpacitySlider.MainSlider.Value = Settings.Opacity*10;
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e
            OpacitySlider.MainSlider.Minimum = 1;
            OpacitySlider.MainSlider.ValueChanged += (_, x) =>
            {
                Settings.Opacity = x.NewValue / 10;
                OpacitySlider.ValueBlock.Text = Settings.Opacity.ToString() + " Opacity";
            };
<<<<<<< HEAD
            OpacitySlider.MainSlider.Value = Settings.Opacity * 10;
=======
>>>>>>> 20fa59d0767e975914b609ee5cbfb94af4d50f6e

            #endregion
        }

        /// <summary>
        /// Binds Functionality to PropertyChanged of the settings
        /// </summary>
        public void BindSettingsToUpdate()
        {
            Settings.OnUpdate += async (name) =>
            {
                await Dispatcher.Yield(DispatcherPriority.Background);
                switch (name)
                {
                    case "TopMost_Setting":
                        Topmost = Settings.TopMost;
                        break;
                    case "IsOBSHidden_Setting":
                        CaptureProtection.SetWindowDisplayAffinity(
                            new WindowInteropHelper(this).Handle,
                            Settings.IsOBSHidden ? CaptureProtection.WDA_EXCLUDEFROMCAPTURE : CaptureProtection.WDA_NONE
                        );
                        break;
                    case "UsingTrayIcon_Setting":
                        
                        break;
                    case "Opacity_Setting":
                        Opacity = Settings.Opacity;
                        break;
                }
            };
        }

        /// <summary>
        /// Loads settings from a saved configuration
        /// </summary>
        /// <param name="settings">Configuration to load from</param>
        public void LoadSettings(dynamic settings)
        {

        }
        #endregion

        #region Extension Cloud
        #endregion
    }
}
