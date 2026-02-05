using System;
using System.Collections.Generic;
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
using Celestia_IDE.Core.Editor;
using System.IO;

namespace Celestia_IDE.Controls
{
    /// <summary>
    /// Interaction logic for TabSystem.xaml
    /// </summary>
    public partial class TabSystem : UserControl
    {
        public TabSystem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Returns the current Monaco Editor instance
        /// </summary>
        public monaco_api current_monaco()
        {
            return maintabs.SelectedContent as monaco_api;
        }

        /// <summary>
        /// Creates a new tab in the TabSystem control with a separated Monaco Editor instance
        /// </summary>
        public void add_tab_from_file(string filePath)
        {
            var tab = CreateTab(File.ReadAllText(filePath), System.IO.Path.GetFileName(filePath), true, filePath);
            if (tab != null) maintabs.Items.Add(tab);
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                if (maintabs.SelectedIndex != -1) mainWindow.SetRpcFile(((TextBox)((TabItem)maintabs.SelectedItem).Header).Text);
            }
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!(e.Source is TabItem tabItem))
            {
                return;
            }

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            }
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is TabItem tabItemTarget &&
                e.Data.GetData(typeof(TabItem)) is TabItem tabItemSource &&
                !tabItemTarget.Equals(tabItemSource) &&
                tabItemTarget.Parent is TabControl tabControl)
            {
                int targetIndex = tabControl.Items.IndexOf(tabItemTarget);

                tabControl.Items.Remove(tabItemSource);
                tabControl.Items.Insert(targetIndex, tabItemSource);
                tabItemSource.IsSelected = true;
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.tabitemcache.Remove(tabItemSource);
                    mainWindow.tabitemcache.Insert(targetIndex, tabItemSource);
                }
            }
        }


        /// <summary>
        /// Changes the current open Monaco Editor tab title
        /// </summary>
        public void ChangeCurrentTabTitle(string title)
        {
            if (maintabs.SelectedItem is TabItem selectedTab)
            {
                ((TextBox)selectedTab.Header).Text = title;
            }
        }

        public Task<string> GetCurrentTabTitle()
        {
            if (maintabs.SelectedItem is TabItem selectedTab)
            {
                return Task.FromResult(((TextBox)selectedTab.Header).Text);
            }

            return Task.FromResult(string.Empty);
        }
        public static TabItem GetHoveredTabItem()
        {
            DependencyObject current = Mouse.DirectlyOver as DependencyObject;

            while (current != null && current is not TabItem)
                current = VisualTreeHelper.GetParent(current);

            return current as TabItem;
        }

        public async void ButtonTabs(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "AddTabButton":
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        var tab = CreateTab("");
                        if (tab != null) maintabs.Items.Add(tab);
                        if (Core.Settings.Minimap) current_monaco().enable_minimap();
                        if (maintabs.SelectedIndex != -1) mainWindow.SetRpcFile(((TextBox)((TabItem)maintabs.SelectedItem).Header).Text);
                    }
                    break;

                case "CloseButton":
                    try
                    {
                        if (Application.Current.MainWindow is MainWindow mainWindow2)
                        {
                            var tab = GetHoveredTabItem();
                            string EditorContent = await ((monaco_api)tab.Content).GetText();
                            if (tab.Tag != null)
                            {
                                if (EditorContent.Replace("\r\n", "\n").Replace("\r", "\n") != File.ReadAllText((string)tab.Tag).Replace("\r\n", "\n").Replace("\r", "\n"))
                                {
                                    if (await mainWindow2.Prompt("You have unsaved changes. Are you sure you wish to close this tab?", "Unsaved Changes", "Cancel", "Yes") != MessageBoxResult.OK) return;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(EditorContent))
                                {
                                    if (await mainWindow2.Prompt("If you close your text file unsaved, your changes will be lost! Are you sure you wish to close this tab?", "Unsaved Text File", "Cancel", "Yes") != MessageBoxResult.OK) return;
                                }
                            }
                            mainWindow2.tabitemcache.Remove(tab);
                            maintabs.Items.Remove(tab);

                            if (maintabs.SelectedIndex == -1)
                            {
                                mainWindow2.SetBaseRichPresence();
                            }
                            else
                            {
                                mainWindow2.SetRpcFile(((TextBox)((TabItem)maintabs.SelectedItem).Header).Text);
                            }
                        }
                    }
                    catch { }
                    break;
            }
        }

        public monaco_api CreateEditor(string Start) 
        {
            //if (CefSharp.Cef.IsInitialized == true)
            {
                return new monaco_api(Start);
            }
            //return null;
        }

        public TabItem CreateTab(string content, string Title = null, bool isFile = false, string? filePath = null)
        {
            if (isFile)
            {
                bool flag = false;
                int index = -1;
                foreach (TabItem other_tab in maintabs.Items)
                {
                    if (other_tab.Tag != null && ((string)other_tab.Tag) == filePath) { flag = true; index = maintabs.Items.IndexOf(other_tab); }
                }
                if (flag) 
                {
                    maintabs.SelectedIndex = index;
                    return null;
                }
            }

            var m = (MainWindow)Application.Current.MainWindow;
            if (Title == null) Title = Core.Settings.TextFileHeader;

            TextBox textBox = new TextBox();
            textBox.Text = Title;
            textBox.IsHitTestVisible = false;
            textBox.IsEnabled = false;
            textBox.TextWrapping = TextWrapping.NoWrap;
            textBox.Style = TryFindResource("InvisibleTextBox") as Style;
            var tab = new TabItem
            {
                Header = textBox,
                Style = TryFindResource("Tab") as Style,
                Foreground = Brushes.White,
                FontSize = 12,
                Content = CreateEditor(content),
                IsSelected = true,
                AllowDrop = true,
            };
            if (isFile) tab.Tag = filePath;
            tab.MouseDown += (sender, e) =>
            {
                if (!(e.OriginalSource is Border))
                    return;

                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    ButtonTabs(new Button() { Name = "CloseButton" }, new RoutedEventArgs());
                }

                else
                {
                    if (tab.Tag != null) return;
                    if (e.RightButton != MouseButtonState.Pressed)
                        return;

                    textBox.IsEnabled = true;
                    if (textBox.Text.LastIndexOf(".") > 0)
                    {
                        textBox.Focus();
                        textBox.SelectionStart = 0;
                        textBox.SelectionLength = textBox.Text.LastIndexOf(".");
                    }
                    else
                    {
                        textBox.Focus();
                        textBox.SelectionStart = 0;
                        textBox.SelectAll();
                    }
                }
            };
            tab.KeyDown += (sender, e) =>
            {

                if (e.Key == Core.Settings.KeyBinds["Explorer_RenameFile"])
                {
                    if (tab.Tag != null) return;
                    textBox.IsEnabled = true;
                    if (textBox.Text.LastIndexOf(".") > 0)
                    {
                        textBox.Focus();
                        textBox.SelectionStart = 0;
                        textBox.SelectionLength = textBox.Text.LastIndexOf(".");
                    }
                    else
                    {
                        textBox.Focus();
                        textBox.SelectionStart = 0;
                        textBox.SelectAll();
                    }
                }
                if (e.Key == Core.Settings.KeyBinds["Explorer_DeleteFile"])
                {
                    ButtonTabs(new Button() { Name = "CloseButton" }, new RoutedEventArgs());
                }
            };
            string oldHeader = Title;
            textBox.GotFocus += (sender, e) =>
            {
                oldHeader = textBox.Text;
                textBox.CaretIndex = textBox.Text.Length - 1;
            };
            textBox.KeyDown += (s, e) =>
            {
                if (textBox.Text == "")
                {
                    textBox.Text = oldHeader;
                    textBox.IsEnabled = false;
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.Return:
                            textBox.IsEnabled = false;
                            if (Application.Current.MainWindow is MainWindow mainWindow2)
                            {
                                if (maintabs.SelectedIndex == -1)
                                {
                                    mainWindow2.SetBaseRichPresence();
                                }
                                else
                                {
                                    mainWindow2.SetRpcFile(((TextBox)((TabItem)maintabs.SelectedItem).Header).Text);
                                }
                            }
                            break;
                        case Key.Escape:
                            textBox.Text = oldHeader;
                            goto case Key.Return;
                    }
                }
            };
            textBox.LostFocus += (sender, e) => textBox.IsEnabled = false;
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.tabitemcache.Add(tab);
            }
            return tab;
        }

        /// <summary>
        /// Clears the Monaco Editor content and resets the title to its index.
        /// </summary>
        public async void Clear_Editor(object sender, RoutedEventArgs e)
        {
            var x = maintabs.SelectedContent as monaco_api;
            try
            {
                await x.SetText("");
            }
            catch { }
        }

        static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        private void maintabs_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer == null) return;

            if (e.Delta < 0)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 130);
            else
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 130);

            e.Handled = true;
        }

        void maintabs_SelectionChanged(object _, SelectionChangedEventArgs e)
        {
            if (e.Source != maintabs) return;
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                if (maintabs.SelectedIndex == -1)
                {
                    mainWindow.SetBaseRichPresence();
                    mainWindow.HomeTabPage.Visibility = Visibility.Visible;
                }
                else
                {
                    mainWindow.SetRpcFile(((TextBox)((TabItem)maintabs.SelectedItem).Header).Text);
                    mainWindow.HomeTabPage.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
