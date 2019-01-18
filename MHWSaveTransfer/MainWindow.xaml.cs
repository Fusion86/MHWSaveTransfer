using System.Diagnostics;
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
            if (e.RightButton == MouseButtonState.Pressed)
                Process.Start(@"https://www.nexusmods.com/monsterhunterworld/mods/486");
        }
    }
}
