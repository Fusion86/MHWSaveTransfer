using MHWSaveTransfer.Helpers;
using MHWSaveTransfer.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MHWSaveTransfer.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangeSteamIdDialog.xaml
    /// </summary>
    public partial class ChangeSteamIdDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string SteamId { get; set; }

        private CancellationTokenSource? cts;
        private readonly SteamWebApi steamWebApi = new SteamWebApi(SuperSecret.STEAM_WEB_API_KEY);

        public ChangeSteamIdDialog(string text = "")
        {
            DataContext = this;
            SteamId = text;
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

        private void Button_SearchSteamAccount(object sender, RoutedEventArgs e)
        {
            _ = SearchSteamAccount();
        }

        private void TxtSearchQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _ = SearchSteamAccount();
        }

        private void LstSteamAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is SteamUserInfo user)
                    _ = GetAndSetSteamId(user);
            }
        }

        private async Task SearchSteamAccount()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Dispatcher.Invoke(() =>
            {
                TxtLoading.Text = "Searching users...";
                TxtLoading.Visibility = Visibility.Visible;
                LstSteamAccounts.Visibility = Visibility.Collapsed;
            });

            string query = TxtSearchQuery.Text;

            try
            {
                var users = await steamWebApi.SearchSteamUser(query, cts.Token);

                Dispatcher.Invoke(() =>
                {
                    LstSteamAccounts.ItemsSource = users;
                });
            }
            catch (Exception ex)
            {
                Log.Error("SearchSteamAccount Error", ex);
            }

            Dispatcher.Invoke(() =>
            {
                TxtLoading.Visibility = Visibility.Collapsed;
                LstSteamAccounts.Visibility = Visibility.Visible;
            });
        }

        private async Task GetAndSetSteamId(SteamUserInfo user)
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Dispatcher.Invoke(() =>
            {
                TxtLoading.Text = "Requesting SteamId...";
                TxtLoading.Visibility = Visibility.Visible;
            });

            if (user.SteamId == null)
            {
                try
                {
                    user.SteamId = await steamWebApi.GetSteamId(user.VanityURL);
                }
                catch (Exception ex)
                {
                    Log.Error("SearchSteamAccount Error", ex);
                }
            }

            Dispatcher.Invoke(() =>
            {
                if (user.SteamId != null)
                {
                    SteamId = user.SteamId;
                }
                TxtLoading.Visibility = Visibility.Collapsed;
            });
        }
    }
}
