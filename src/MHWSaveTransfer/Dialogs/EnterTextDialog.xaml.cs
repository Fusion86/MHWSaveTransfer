using System.Windows;

namespace MHWSaveTransfer.Dialogs
{
    /// <summary>
    /// Interaction logic for EnterTextDialog.xaml
    /// </summary>
    public partial class EnterTextDialog : Window
    {
        public string Text { get; set; }

        public EnterTextDialog(string title, string text = "")
        {
            DataContext = this;
            Title = title;
            Text = text;
            InitializeComponent();
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Button_Apply(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
