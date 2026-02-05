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

namespace Celestia_IDE.Controls.ScriptCloud
{
    /// <summary>
    /// Interaction logic for FavoritedScriptControl.xaml
    /// </summary>
    public partial class FavoritedScriptControl : UserControl
    {
        public FavoritedScriptControl(bool isPatched, bool hasKey)
        {
            InitializeComponent();
            PatchedFlag.Visibility = isPatched ? Visibility.Visible : Visibility.Collapsed;
            KeyFlag.Visibility = hasKey ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
