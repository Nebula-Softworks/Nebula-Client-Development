using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Celestia_IDE.Core
{
    public static class Settings
    {
        public static event Action<string>? OnUpdate;

        //interface
        private static bool _topMost;
        private static bool _isOBSHidden;
        private static bool _usingTrayIcon;
        private static double _opacity;
        private static double _scale;
        private static int _langauge;
        private static bool _rpc = true;
        private static Dictionary<string, Key> _keyBinds = new Dictionary<string, Key>()
        {
            { "SideBarToggle", Key.B },
            { "PanelToggle", Key.J },
            { "CommandPaletteToggle", Key.P },
            { "Explorer_RenameFile", Key.F2 },
            { "Explorer_DeleteFile", Key.Delete },
            { "OpenFile", Key.O },
            { "OpenFolder", Key.K },
            { "NewTextFile", Key.N },
            { "SaveFile", Key.S },
            { "Panel_ViewTerminal", Key.OemTilde },
            { "Panel_ViewOutput", Key.L },
        };
        private static bool _startOnStartup;
        //appearance
        private static Dictionary<string, Color> _colorChoices;
        private static string? _backgroundPhotoPath;
        //engine
        private static bool _fpsUnlock;
        private static bool _replicateFirst;
        private static bool _useCpuLimit = false;
        private static double _cpuLimit = 10000; // SLIDER
        private static bool _useRamLimit = false;
        private static ulong _ramLimit = 8192UL * 1024 * 1024;
        private static bool _runAutoExecute;
        //editor
        private static bool _minimap;
        private static bool _formatOnSave;
        private static bool _saveWorkspaceTabs;
        private static bool _ligatures;
        private static bool _intellisense;
        private static bool _antiSkid;
        private static double _fontSize;
        private static string _font;
        private static string _textFileHeader = "New Untitled File";
        private static int _defaultLanguage;
        private static bool _autoFormat;
        private static bool _inlayHints;
        //account
        private static string _displayName;
        private static string? _userName;
        private static string _pfpPath;
        private static Dictionary<string, string> _interogations;
        private static double _keyTimeLeft;

        public static bool TopMost
        {
            get
            {
                return _topMost;
            }
            set
            {
                _topMost = value;
                OnUpdate?.Invoke("TopMost_Setting");
            }
        }
        public static bool IsOBSHidden
        {
            get
            {
                return _isOBSHidden;
            }
            set
            {
                _isOBSHidden = value;
                OnUpdate?.Invoke("IsOBSHidden_Setting");
            }
        }
        public static bool UsingTrayIcon
        {
            get
            {
                return _usingTrayIcon;
            }
            set
            {
                _usingTrayIcon = value;
                OnUpdate?.Invoke("UsingTrayIcon_Setting");
            }
        }
        public static double Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                _opacity = value;
                OnUpdate?.Invoke("Opacity_Setting");
            }
        }
        public static double InterfaceScale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                OnUpdate?.Invoke("Scale_Setting");
            }
        }
        public static int InterfaceLanguage
        {
            // 0 = English
            // 1 = Mandarin
            // 2 = Hindi
            // 3 = Japanese
            // 4 = German
            // 5 = Portugese
            // 6 = Filipino
            // 7 = French
            // 8 = Arabic
            // 9 = Spanish
            get
            {
                return _langauge;
            }
            set
            {
                _langauge = value;
                OnUpdate?.Invoke("Language_Setting");
            }
        }
        public static bool DiscordRPCEnabled
        {
            get
            {
                return _rpc;
            }
            set
            {
                _rpc = value;
                OnUpdate?.Invoke("RPCEnabled_Setting");
            }
        }
        public static Dictionary<string, Key> KeyBinds
        {
            get
            {
                return _keyBinds;
            }
            set
            {
                _keyBinds = value;
                OnUpdate?.Invoke("KeyBinds_Setting");
            }
        }
        public static bool StartOnStartup
        {
            get
            {
                return _startOnStartup;
            }
            set
            {
                _startOnStartup = value;
                OnUpdate?.Invoke("StartOnStartup_Setting");
            }
        }

        public static Dictionary<string, Color> ColorChoices
        {
            get
            {
                return _colorChoices;
            }
            set
            {
                _colorChoices = value;
                OnUpdate?.Invoke("ColorChoices_Setting");
            }
        }
        public static string BackgroundPhotoPath
        {
            get
            {
                return _backgroundPhotoPath;
            }
            set
            {
                _backgroundPhotoPath = value;
                OnUpdate?.Invoke("BackgroundPhotoPath_Setting");
            }
        }

        public static bool FpsUnlock
        {
            get
            {
                return _fpsUnlock;
            }
            set
            {
                _fpsUnlock = value;
                OnUpdate?.Invoke("FpsUnlock_Setting");
            }
        }
        public static bool ReplicateFirst
        {
            get
            {
                return _replicateFirst;
            }
            set
            {
                _replicateFirst = value;
                OnUpdate?.Invoke("ReplicateFirst_Setting");
            }
        }
        public static bool UseCpuLimit
        {
            get
            {
                return _useCpuLimit;
            }
            set
            {
                _useCpuLimit = value;
                OnUpdate?.Invoke("UseCpuLimit_Setting");
            }
        }
        public static double CpuLimit
        {
            get
            {
                return _cpuLimit;
            }
            set
            {
                _cpuLimit = value;
                OnUpdate?.Invoke("CpuLimit_Setting");
            }
        }
        public static bool UseRamLimit
        {
            get
            {
                return _useRamLimit;
            }
            set
            {
                _useRamLimit = value;
                OnUpdate?.Invoke("UseCpuLimit_Setting");
            }
        }
        public static ulong RamLimit
        {
            get
            {
                return _ramLimit;
            }
            set
            {
                _ramLimit = value;
                OnUpdate?.Invoke("RamLimit_Setting");
            }
        }
        public static bool RunAutoExecute
        {
            get
            {
                return _runAutoExecute;
            }
            set
            {
                _runAutoExecute = value;
                OnUpdate?.Invoke("RunAutoExecute_Setting");
            }
        }

        public static bool Minimap
        {
            get
            {
                return _minimap;
            }
            set
            {
                _minimap = value;
                OnUpdate?.Invoke("Minimap_Setting");
            }
        }
        public static bool FormatOnSave
        {
            get
            {
                return _formatOnSave;
            }
            set
            {
                _formatOnSave = value;
                OnUpdate?.Invoke("FormatOnSave_Setting");
            }
        }
        public static bool SaveWorkspaceTabs
        {
            get
            {
                return _saveWorkspaceTabs;
            }
            set
            {
                _saveWorkspaceTabs = value;
                OnUpdate?.Invoke("SaveWorkspaceTabs_Setting");
            }
        }
        public static bool Ligatures
        {
            get
            {
                return _ligatures;
            }
            set
            {
                _ligatures = value;
                OnUpdate?.Invoke("Ligatures_Setting");
            }
        }
        public static bool Intellisense
        {
            get
            {
                return _intellisense;
            }
            set
            {
                _intellisense = value;
                OnUpdate?.Invoke("Intellisense_Setting");
            }
        }
        public static bool AntiSkid
        {
            get
            {
                return _antiSkid;
            }
            set
            {
                _antiSkid = value;
                OnUpdate?.Invoke("AntiSkid_Setting");
            }
        }
        public static double FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
                OnUpdate?.Invoke("FontSize_Setting");
            }
        }
        public static string Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
                OnUpdate?.Invoke("Font_Setting");
            }
        }
        public static string TextFileHeader
        {
            get
            {
                return _textFileHeader;
            }
            set
            {
                _textFileHeader = value;
                OnUpdate?.Invoke("TextFileHeader_Setting");
            }
        }
        public static int EditorLanguage
        {
            get
            {
                return _defaultLanguage;
            }
            set
            {
                _defaultLanguage = value;
                OnUpdate?.Invoke("EditorLanguage_Setting");
            }
        }
        public static bool AutoFormat
        {
            get
            {
                return _autoFormat;
            }
            set
            {
                _autoFormat = value;
                OnUpdate?.Invoke("AutoFormat_Setting");
            }
        }
        public static bool InlayHints
        {
            get
            {
                return _inlayHints;
            }
            set
            {
                _inlayHints = value;
                OnUpdate?.Invoke("InlayHints_Setting");
            }
        }

        public static string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                OnUpdate?.Invoke("DisplayName_Setting");
            }
        }
        public static string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                OnUpdate?.Invoke("UserName_Setting");
            }
        }
        public static string ProfilePicturePath
        {
            get
            {
                return _pfpPath;
            }
            set
            {
                _pfpPath = value;
                OnUpdate?.Invoke("ProfilePicturePath_Setting");
            }
        }
        public static Dictionary<string, string> Interogations
        {
            get
            {
                return _interogations;
            }
            set
            {
                _interogations = value;
                OnUpdate?.Invoke("Interogations_Setting");
            }
        }
        public static double KeyTimeLeft
        {
            get
            {
                return _keyTimeLeft;
            }
            set
            {
                _keyTimeLeft = value;
                OnUpdate?.Invoke("KeyTimeLeft_Setting");
            }
        }
    }
}
