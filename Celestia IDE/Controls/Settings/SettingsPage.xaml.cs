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

namespace Celestia_IDE.Controls.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            MainGrid.Children.Remove(Pages_Editor);
            MainGrid.Children.Remove(Pages_Appearance);
            MainGrid.Children.Remove(Pages_Engine);
            MainGrid.Children.Remove(Pages_Account);
        }

        private void PageSelector_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            switch (((RadioButton)sender).Name) 
            {
                case "Selectors_Interface":
                    CurrentSettingsNameBlock.Text = "Interface Settings";
                    MainContentScroller.Content = Pages_Interface;
                    break;
                case "Selectors_Editor":
                    CurrentSettingsNameBlock.Text = "Code Editor Settings";
                    MainContentScroller.Content = Pages_Editor;
                    break;
                case "Selectors_Appearance":
                    CurrentSettingsNameBlock.Text = "Interface Appearance";
                    MainContentScroller.Content = Pages_Appearance;
                    break;
                case "Selectors_Engine":
                    CurrentSettingsNameBlock.Text = "Engine Settings";
                    MainContentScroller.Content = Pages_Engine;
                    break;
                case "Selectors_Account":
                    CurrentSettingsNameBlock.Text = "Account Management";
                    MainContentScroller.Content = Pages_Account;
                    break;
            }
        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
