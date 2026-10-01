using bibliotek.Models;
using bibliotek.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace bibliotek
{
    public partial class MainWindow : Window
    {
        private readonly MediaSearch _searchService = new MediaSearch();
        private User? _currentUser; // Sparar den inloggade användaren

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded; // Körs när fönstret laddats klart
        }

        // Överlagrad konstruktor som tar emot den inloggade användaren från LoginWindow
        public MainWindow(User user) : this()
        {
            _currentUser = user;
            this.Title = $"Bibliotekssystem - Inloggad som {user.FirstName} {user.LastName}";
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PerformSearch(); // Hämta sökresultat vid start
            UpdateLoginButtonState(); // Uppdatera knappen när fönstret är helt redo
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void UpdateLoginButtonState()
        {
            if (_currentUser != null)
            {
                btnLogin.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Visible;
            }
            else
            {
                btnLogin.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var context = new ApplicationDbContext();
            var userService = new UserService(context);

            var loginWindow = new LoginWindow(userService);
            bool? result = loginWindow.ShowDialog();

            if (result == true && loginWindow.LoggedInUser != null)
            {
                _currentUser = loginWindow.LoggedInUser;
                this.Title = $"Bibliotekssystem - Inloggad som {_currentUser.FirstName} {_currentUser.LastName}";
                UpdateLoginButtonState();
            }
            else if (loginWindow.LoggedInUser != null && loginWindow.LoggedInUser.Role)
            {
                this.Close();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            this.Title = "Stina-Lib — Sök Media";
            MessageBox.Show("Du har loggats ut.", "Utloggad", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateLoginButtonState();
        }

        // Hanterar klicket för både Logga in och Logga ut

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            try
            {
                string query = txtSearch.Text.Trim();
                var results = _searchService.ExecuteSearch(query);
                dgSearchResults.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte hämta sökresultat:\n{ex.Message}", "Databasfel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowSearchView()
        {
            InitializeComponent();
            PerformSearch();
        }

        private async void SeedButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=127.0.0.1;Database=bibliotek;Uid=root;Pwd=hemligt-losenord;";

            await Seedingscript.SeedAsync(connectionString);

            MessageBox.Show("Databasen har uppdaterats med ny testdata! 🥳");
        }
    }
}