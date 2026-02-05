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
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using Celestia_IDE.Core.ExplorerSystem;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Celestia_IDE.Controls
{
    public abstract class FileSystemItem : INotifyPropertyChanged
    {
        private string _originalHeader;
        private bool _isNew;
        private string _header;
        private bool _isRenaming;

        public string Header
        {
            get => _header;
            set
            {
                if (_header != value)
                {
                    _header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }

        public string OriginalHeader
        {
            get => _originalHeader;
            set
            {
                if (_originalHeader != value)
                {
                    _originalHeader = value;
                    OnPropertyChanged(nameof(OriginalHeader));
                }
            }
        }

        public bool IsNew
        {
            get => _isNew;
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    OnPropertyChanged(nameof(IsNew));
                }
            }
        }

        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                if (_isRenaming != value)
                {
                    _isRenaming = value;
                    OnPropertyChanged(nameof(IsRenaming));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

    }

    public class FolderItem : FileSystemItem
    {
        public ObservableCollection<FileSystemItem> Children { get; set; }
    }

    public class LuaFileItem : FileSystemItem { }
    public class TextFileItem : FileSystemItem { }
    public class OtherFileItem : FileSystemItem { }

    public partial class Explorer : Page, INotifyPropertyChanged
    {
        private Point _dragStart;

        public ObservableCollection<FileSystemItem> RootItems { get; set; }
        public FileSystemWatcher fileWatcher;
        private MainWindow mainWindow;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private HashSet<string> GetExpandedPaths()
        {
            var expanded = new HashSet<string>();
            foreach (var item in RootItems)
                SaveExpanded(item, expanded);
            return expanded;
        }
        private HashSet<string> _expandedStateBeforeSearch;
        private bool _isSearching = false;

        private void SaveExpanded(FileSystemItem item, HashSet<string> set)
        {
            if (item is FolderItem folder && folder.IsExpanded)
                set.Add(GetRelativePath(folder));

            if (item is FolderItem f)
            {
                foreach (var child in f.Children)
                    SaveExpanded(child, set);
            }
        }

        private void RestoreExpanded(HashSet<string> expanded)
        {
            foreach (var item in RootItems)
                ApplyExpanded(item, expanded);
        }

        private void ApplyExpanded(FileSystemItem item, HashSet<string> set)
        {
            if (item is FolderItem folder)
            {
                folder.IsExpanded = set.Contains(GetRelativePath(folder));
                foreach (var child in folder.Children)
                    ApplyExpanded(child, set);
            }
        }

        private DispatcherTimer autoScrollTimer;
        private double scrollSpeed = 6;
        public Explorer(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            RootItems = new ObservableCollection<FileSystemItem>();
            LoadFileTree(window.WorkspaceFolder, RootItems);
            InitializeFileWatcher(window.WorkspaceFolder);
            Main.Items.Clear();
            Main.ItemsSource = RootItems;
            PreviewKeyDown += Explorer_PreviewKeyDown;

            Main.PreviewMouseLeftButtonDown += Tree_PreviewMouseLeftButtonDown;
            Main.MouseMove += Tree_MouseMove;
            Main.Drop += Tree_Drop;
            autoScrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            autoScrollTimer.Tick += AutoScrollTimer_Tick;
        }

        [StructLayout(LayoutKind.Sequential)] public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT lpPoint);
        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        private void AutoScrollTimer_Tick(object sender, EventArgs e)
        {
            var screenPos = GetCursorPosition();
            var treePos = FindVisualChild<ScrollViewer>(Main).PointFromScreen(new Point(screenPos.X, screenPos.Y));
            double offsetY = treePos.Y;

            if (offsetY < 40) 
            {
                ScrollTree(-scrollSpeed);
            }
            else if (offsetY > Main.ActualHeight - 40) 
            {
                ScrollTree(scrollSpeed);
            }
        }

        private void ScrollTree(double delta)
        {
            var sv = FindVisualChild<ScrollViewer>(Main);
            if (sv == null) return;

            sv.ScrollToVerticalOffset(sv.VerticalOffset + delta);
        }


        private void Tree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }
        public bool _isDragDropActive = false;

        private void Tree_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragDropActive) return;
            if (e.LeftButton != MouseButtonState.Pressed) 
            { 
                if (autoScrollTimer.IsEnabled) autoScrollTimer.Stop();
                _isDragDropActive = false;
                return; 
            }

            Point pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            // get TreeViewItem under mouse
            var tvi = FindVisualParent<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (tvi == null) return;

            autoScrollTimer.Start();
            _isDragDropActive = true;
            DragDrop.DoDragDrop(tvi, tvi, DragDropEffects.Move);
        }

        private FileSystemItem GetDropTarget(DependencyObject source)
        {
            while (source != null)
            {
                if (source is TreeViewItem tvi)
                    if (tvi.DataContext is FolderItem)
                        return tvi.DataContext as FolderItem;

                source = VisualTreeHelper.GetParent(source);
            }

            return null;
        }


        private bool IsDescendant(FileSystemItem source, FileSystemItem target)
        {
            if (source is not FolderItem sourceFolder)
                return false;

            FileSystemItem current = target;

            while (current != null)
            {
                if (current == sourceFolder)
                    return true;

                current = FindParent(current);
            }

            return false;
        }

        private async Task MoveFile(string sourcePath, string destPath)
        {
            if (File.Exists(destPath))
            {
                var result = await mainWindow.Prompt(
                    $"A file already exists:\n\n{Path.GetFileName(destPath)}\n\nReplace it?",
                    "Replace file?",
                    "No, Skip this file",
                    "Yes"
                );

                if (result != MessageBoxResult.OK)
                    return;
            }

            File.Copy(sourcePath, destPath, true);
            File.Delete(sourcePath);
            foreach (TabItem tab in mainWindow.TabSystemz.maintabs.Items)
            {
                if ((string)tab.Tag == Path.GetFullPath(sourcePath))
                {
                    tab.Tag = destPath;
                }
            }
        }


        private async Task MoveFolder(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                await MoveFile(file, destFile);
            }
            foreach (var folder in Directory.GetDirectories(sourceDir))
            {
                var destFolder = Path.Combine(destDir, Path.GetFileName(folder));
                await MoveFolder(folder, destFolder); 
            }

            if (Directory.GetFiles(sourceDir).Length == 0 &&
                Directory.GetDirectories(sourceDir).Length == 0)
            {
                Directory.Delete(sourceDir);
            }
        }


        private void Tree_Drop(object sender, DragEventArgs e)
        {
            autoScrollTimer.Stop();

            //mainWindow.ApplicationPrint(1, "drag func/dropped");
            if (!e.Data.GetDataPresent(typeof(TreeViewItem))) return;


            //mainWindow.ApplicationPrint(1, "passed initial check");

            var draggedTvi = (TreeViewItem)e.Data.GetData(typeof(TreeViewItem));
            var draggedItem = draggedTvi.DataContext as FileSystemItem;
            if (draggedItem == null) return;
            //mainWindow.ApplicationPrint(1, "not null");

            // Determine drop target
            var target = GetDropTarget(e.OriginalSource as DependencyObject);

            string sourcePath = Path.Combine(
                mainWindow.WorkspaceFolder,
                GetRelativePath(draggedItem)
            );

            string destinationDir = "";


            if (target is FolderItem folder)
            {
                destinationDir = Path.Combine(
                    mainWindow.WorkspaceFolder,
                    GetRelativePath(folder)
                );
                //mainWindow.ApplicationPrint(1, destinationDir);
            }
            else if (target == null)
            {
                // dropped on empty space → root
                destinationDir = mainWindow.WorkspaceFolder;
                //mainWindow.ApplicationPrint(1, destinationDir);
            }

            string destPath = Path.Combine(destinationDir, draggedItem.Header);

            // prevent self-drop
            if (string.Equals(sourcePath, destPath, StringComparison.OrdinalIgnoreCase))
                return;
            if (draggedItem is FolderItem && target is FolderItem tf)
            {
                if (IsDescendant(draggedItem, tf))
                    return;
            }


            try
            {
                //mainWindow.ApplicationPrint(1, "Starting Move");
                if (draggedItem is FolderItem)
                    MoveFolder(sourcePath, destPath);
                else
                    MoveFile(sourcePath, destPath);

                //Reload();
            }
            catch (Exception ex)
            {
                mainWindow.ApplicationPrint(2, $"Move failed: {ex.Message}");
            }
            _isDragDropActive = false;
        }

        private void Explorer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Main.SelectedItem is not FileSystemItem item) return;

            if (e.Key == Core.Settings.KeyBinds["Explorer_RenameFile"])
            {
                item.OriginalHeader = item.Header;
                item.IsRenaming = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Main.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
                    {
                        tvi.Focus();
                        var tb = FindVisualChild<TextBox>(tvi);
                        tb?.Focus();
                        tb?.SelectAll();
                    }
                }));
                e.Handled = true;
            }

            if (e.Key == Core.Settings.KeyBinds["Explorer_DeleteFile"])
            {
                DeleteItem(item);
                e.Handled = true;
            }
        }

        private void Rename_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox tb || tb.DataContext is not FileSystemItem item) return;

            if (e.Key == Key.Enter)
            {
                CommitRename(tb);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelRename(tb);
                e.Handled = true;
            }
        }

        private void Rename_LostFocus(object sender, RoutedEventArgs e) { }

        private void CommitRename(object sender)
        {
            if (sender is not TextBox tb || tb.DataContext is not FileSystemItem item) return;

            item.IsRenaming = false;

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                if (item.IsNew) RootItems.Remove(item);
                else item.Header = item.OriginalHeader;
                return;
            }

            string baseDir = mainWindow.WorkspaceFolder;

            if (item.IsNew)
            {
                string path = Path.Combine(baseDir, tb.Text);
                if (item is FolderItem)
                {
                    if (!Directory.Exists(path) && !File.Exists(path))
                        Directory.CreateDirectory(path);
                    else RootItems.Remove(item);
                }
                else
                {
                    if (!File.Exists(path))
                        File.WriteAllText(path, "");
                    else RootItems.Remove(item);
                }
                item.IsNew = false;
                Reload();
                return;
            }

            string oldPath = Path.Combine(baseDir, GetRelativeOriginalPath(item));
            string newPath = Path.Combine(Path.GetDirectoryName(oldPath)!, tb.Text);

            try
            {
                if (item is FolderItem)
                {
                    Directory.CreateDirectory(newPath);
                    foreach (var entry in Directory.GetFileSystemEntries(oldPath))
                    {
                        string dest = Path.Combine(newPath, Path.GetFileName(entry)!);
                        if (Directory.Exists(entry))
                            Directory.Move(entry, dest);
                        else
                            File.Move(entry, dest);
                    }
                    Directory.Delete(oldPath, true);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
                    if (File.Exists(oldPath))
                        File.Move(oldPath, newPath);
                    else
                        File.WriteAllText(newPath, "");
                }
                Reload();
            }
            catch (Exception e)
            {
                mainWindow.ApplicationPrint(2, $"Failed to rename file! {e.Message}");
            }
        }

        private void CancelRename(object sender)
        {
            if (sender is TextBox tb && tb.DataContext is FileSystemItem item)
                item.IsRenaming = false;
        }

        private void DeleteItem(FileSystemItem item)
        {
            string path = Path.Combine(mainWindow.WorkspaceFolder, GetRelativePath(item));
            try
            {
                if (item is FolderItem)
                    Directory.Delete(path, true);
                else
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                mainWindow.ApplicationPrint(2, $"Failed to delete file! {ex.Message}");
            }
            Reload();
        }

        private FileSystemItem GetContextItem(object sender)
        {
            if (sender is MenuItem mi)
            {
                if (mi.DataContext is FileSystemItem fsi) return fsi;
                var tvi = FindVisualParent<TreeViewItem>(mi);
                return tvi?.DataContext as FileSystemItem;
            }
            return null;
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent is not T)
                parent = VisualTreeHelper.GetParent(parent);
            return parent as T;
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void newfilefinder(FileSystemItem item)
        {
            if (Main.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
            {
                tvi.Focus();
                var tb = FindVisualChild<TextBox>(tvi);
                if (tb != null)
                {
                    tb.Focus();
                    tb.SelectAll();
                }
                else
                {
                    newfilefinder(item);
                }
            }
        }

        public void NewFile_Click(object sender, RoutedEventArgs e)
        {
            var item = new OtherFileItem
            {
                Header = "",
                OriginalHeader = "",
                IsRenaming = true,
                IsNew = true
            };
            item.OriginalHeader = item.Header;
            item.IsRenaming = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Main.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
                {
                    tvi.Focus();
                    var tb = FindVisualChild<TextBox>(tvi);
                    tb?.Focus();
                    tb?.SelectAll();
                }
            }));
            RootItems.Insert(0, item);
            Dispatcher.BeginInvoke(new Action(() => newfilefinder(item)));
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            var item = new FolderItem
            {
                Header = "",
                OriginalHeader = "",
                IsRenaming = true,
                IsNew = true,
                Children = new ObservableCollection<FileSystemItem>()
            };
            RootItems.Insert(0, item);
            Dispatcher.BeginInvoke(new Action(() => newfilefinder(item)));
        }

        public void Reload()
        {
            if (!string.IsNullOrEmpty(WorkspaceSearch.Text))
            {
                RootItems.Clear();
                LoadFilteredFileTree(mainWindow.WorkspaceFolder, RootItems, WorkspaceSearch.Text);
                return;
            }

            var expanded = GetExpandedPaths();

            RootItems.Clear();
            LoadFileTree(mainWindow.WorkspaceFolder, RootItems);
            RestoreExpanded(expanded);
        }

        public void LoadFileTree(string directoryPath, ObservableCollection<FileSystemItem> parentCollection)
        {
            parentCollection.Clear();
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return;

            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                var folder = new FolderItem
                {
                    Header = Path.GetFileName(dir),
                    Children = new ObservableCollection<FileSystemItem>()
                };
                LoadFileTree(dir, folder.Children);
                parentCollection.Add(folder);
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                var fileName = Path.GetFileName(file);
                var fileItem = CreateFileItem(file, fileName);
                if (fileItem != null) parentCollection.Add(fileItem);
            }
        }

        public FileSystemItem CreateFileItem(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return null;
            string extension = Path.GetExtension(filePath)?.ToLower();
            return extension switch
            {
                ".lua" => new LuaFileItem { Header = fileName },
                ".txt" => new TextFileItem { Header = fileName },
                _ => new OtherFileItem { Header = fileName }
            };
        }

        public void InitializeFileWatcher(string directoryPath)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder))
            {
                inactive.Visibility = Visibility.Visible;
                WorkspaceSearch.Visibility = Visibility.Collapsed;
                SearchBorder.Visibility = Visibility.Collapsed;
                return;
            }

            inactive.Visibility = Visibility.Hidden;
            WorkspaceSearch.Visibility = Visibility.Visible;
            SearchBorder.Visibility = Visibility.Visible;

            fileWatcher = new FileSystemWatcher(directoryPath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
            };

            fileWatcher.Changed += OnFileSystemChanged;
            fileWatcher.Created += OnFileSystemChanged;
            fileWatcher.Deleted += OnFileSystemChanged;
            fileWatcher.Renamed += OnFileSystemRenamed;
        }

        public void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return;
            Dispatcher.Invoke(Reload);
        }

        public void OnFileSystemRenamed(object sender, RenamedEventArgs e)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return;
            Dispatcher.Invoke(Reload);
        }

        public void LoadFilteredFileTree(string directoryPath, ObservableCollection<FileSystemItem> parentCollection, string searchText)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return;
            searchText = searchText.ToLower();

            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                var folder = new FolderItem
                {
                    Header = Path.GetFileName(dir),
                    Children = new ObservableCollection<FileSystemItem>()
                };

                LoadFilteredFileTree(dir, folder.Children, searchText);

                bool folderMatches = folder.Header.ToLower().Contains(searchText);
                bool hasMatchingChildren = folder.Children.Any();

                if (folderMatches || hasMatchingChildren)
                {
                    parentCollection.Add(folder);
                    folder.IsExpanded = hasMatchingChildren;
                }
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                string fileName = Path.GetFileName(file);
                if (fileName.ToLower().Contains(searchText))
                {
                    var fileItem = CreateFileItem(file, fileName);
                    if (fileItem != null) parentCollection.Add(fileItem);
                }
            }
        }

        public string GetRelativePath(FileSystemItem item)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return null;
            var path = new Stack<string>();
            while (item != null)
            {
                path.Push(item.Header);
                if (item is FolderItem folderItem && RootItems.Contains(folderItem)) break;
                item = FindParent(item);
            }
            return Path.Combine(path.ToArray());
        }

        public string GetRelativeOriginalPath(FileSystemItem item)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return null;
            var path = new Stack<string>();
            while (item != null)
            {
                path.Push(item.OriginalHeader);
                if (item is FolderItem folderItem && RootItems.Contains(folderItem)) break;
                item = FindParent(item);
            }
            return Path.Combine(path.ToArray());
        }

        public FileSystemItem FindParent(FileSystemItem child)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return null;
            foreach (var folder in RootItems.OfType<FolderItem>())
            {
                if (folder.Children.Contains(child)) return folder;
                var parent = FindParentRecursive(folder, child);
                if (parent != null) return parent;
            }
            return null;
        }

        public FileSystemItem FindParentRecursive(FolderItem folder, FileSystemItem child)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return null;
            foreach (var subItem in folder.Children)
            {
                if (subItem == child) return folder;
                if (subItem is FolderItem subFolder)
                {
                    var parent = FindParentRecursive(subFolder, child);
                    if (parent != null) return parent;
                }
            }
            return null;
        }

        public void Workspace_SelectedItemChanged(object sender, object e)
        {
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return;
            if (Main.SelectedItem is FileSystemItem selectedItem && selectedItem is not FolderItem)
            {
                string filePath = Path.Combine(mainWindow.WorkspaceFolder, GetRelativePath(selectedItem));
                if (File.Exists(filePath))
                {
                    try
                    {
                        var cm = mainWindow.TabSystemz;
                        cm.add_tab_from_file(Path.GetFullPath(filePath));
                        //mainWindow.ApplicationPrint(1, filePath);
                    }
                    catch { }
                }
            }
        }

        private async void Main_MouseDi(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.Background);
            if (string.IsNullOrEmpty(mainWindow.WorkspaceFolder)) return;
            var element = e.OriginalSource as DependencyObject;
            var item = ItemsControl.ContainerFromElement(Main, element) as TreeViewItem;

            if (item == null)
                return;
            if (Main.SelectedItem is FolderItem selectedItem)
            {
                try
                {
                    selectedItem.IsExpanded = !selectedItem.IsExpanded;
                } catch { mainWindow.ApplicationPrint(1, "failed"); }
            }
        }

        public void WorkspaceSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = WorkspaceSearch.Text.ToLower();

            if (!_isSearching && !string.IsNullOrEmpty(searchText))
            {
                _expandedStateBeforeSearch = GetExpandedPaths();
                _isSearching = true;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                _isSearching = false;

                RootItems.Clear();
                LoadFileTree(mainWindow.WorkspaceFolder, RootItems);
                if (_expandedStateBeforeSearch != null)
                    RestoreExpanded(_expandedStateBeforeSearch);

                return;
            }

            RootItems.Clear();
            LoadFilteredFileTree(mainWindow.WorkspaceFolder, RootItems, searchText);
        }
    }
}
