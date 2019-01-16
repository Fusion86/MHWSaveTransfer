using MHWSaveTransfer.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MHWSaveTransfer.UserControls
{
    /// <summary>
    /// Interaction logic for SaveSlotItem.xaml
    /// </summary>
    public partial class SaveSlotItem : UserControl
    {
        public SaveSlotItem()
        {
            InitializeComponent();
        }

        public SaveSlotViewModel SaveSlot
        {
            get { return (SaveSlotViewModel)GetValue(SaveSlotProperty); }
            set { SetValue(SaveSlotProperty, value); }
        }

        public static readonly DependencyProperty SaveSlotProperty =
            DependencyProperty.Register("SaveSlot", typeof(SaveSlotViewModel), typeof(SaveSlotItem), new PropertyMetadata(null));
    }
}
