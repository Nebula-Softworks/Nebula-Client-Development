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
using System.Windows.Shapes;

namespace Celestia_IDE
{
    /// <summary>
    /// Interaction logic for KeySystemWindow.xaml
    /// </summary>
    public partial class KeySystemWindow : Window
    {
        public KeySystemWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, object e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
