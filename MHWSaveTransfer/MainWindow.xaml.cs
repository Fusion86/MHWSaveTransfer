using MHWSaveTransfer.Helpers;
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
            // Check compatibility and close if not compatible
            // We need to manually check this because due to the way we publish our binary Windows can't seem to figure out which minor .NET Framework version is needed.
            // Example: user has 4.7 installed and tries to open this app, it opens just fine but crashes whenever the user tries to do something (because we need .NET Framework 4.7.2)
            if (Compatibility.IsCompatible(Compatibility.DotNetRelease.NET472) == false)
                Close();

            InitializeComponent();
        }

        private void StatusBarItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                Process.Start(@"https://www.nexusmods.com/monsterhunterworld/mods/486");
        }
    }
}
