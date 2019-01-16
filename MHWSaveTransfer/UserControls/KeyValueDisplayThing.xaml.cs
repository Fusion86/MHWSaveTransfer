using System.Windows;
using System.Windows.Controls;

namespace MHWSaveTransfer.UserControls
{
    /// <summary>
    /// Interaction logic for KeyValueDisplayThing.xaml
    /// </summary>
    public partial class KeyValueDisplayThing : UserControl
    {
        public KeyValueDisplayThing()
        {
            InitializeComponent();
        }

        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(KeyValueDisplayThing), new PropertyMetadata(""));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(KeyValueDisplayThing), new PropertyMetadata(""));
    }
}
