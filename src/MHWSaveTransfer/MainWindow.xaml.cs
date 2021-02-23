using MHWSaveTransfer.Helpers;
using System.Windows;
using System.Windows.Input;

namespace MHWSaveTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StatusBarItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Utility.OpenBrowser(@"https://www.nexusmods.com/monsterhunterworld/mods/486");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
