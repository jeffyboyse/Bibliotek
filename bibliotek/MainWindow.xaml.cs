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
        private readonly User? _currentUser; // Sparar den inloggade användaren

        public MainWindow()
        {
            InitializeComponent();
            PerformSearch(); // Gör en första sökning när fönstret öppnas
        }

        // Överlagrad konstruktor som tar emot den inloggade användaren
        public MainWindow(User user) : this()
        {
            _currentUser = user;
            this.Title = $"Bibliotekssystem - Inloggad som {user.FirstName} {user.LastName}";
        }

        // LÖSNING PÅ FELET: Denna metod kallas från ProductView för att gå tillbaka
        public void ShowSearchView()
        {
            MainContent.Content = SearchGrid;
            PerformSearch();
        }

        // Navigera till produktsidan direkt vid enkelklick i tabellen
        private void DgSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSearchResults.SelectedItem is MediaSearchResult selectedItem)
            {
                // Nollställ markeringen så att raden kan klickas igen senare
                dgSearchResults.SelectedItem = null;

                // Växla innehållet i MainWindow till ProductView
                MainContent.Content = new ProductView(selectedItem);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var context = new ApplicationDbContext();
            var userService = new UserService(context);
            var loginWindow = new LoginWindow(userService);
            loginWindow.Show();
        }

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
                string query = txtSearch.Text?.Trim() ?? string.Empty;
                var results = _searchService.ExecuteSearch(query);
                dgSearchResults.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte hämta sökresultat:\n{ex.Message}", "Databasfel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SeedButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=127.0.0.1;Database=bibliotek;Uid=root;Pwd=hemligt-losenord;";
            await Seedingscript.SeedAsync(connectionString);
            MessageBox.Show("Databasen har uppdaterats med ny testdata! 🥳");
            PerformSearch();
        }
    }
}