using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace bibliotek
{
    public partial class MainWindow : Window
    {
        private readonly MediaSearch _searchService = new MediaSearch();

        public MainWindow()
        {
            InitializeComponent();
            PerformSearch(); // Perform an initial search to populate all media on startup
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
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
                string query = txtSearch.Text.Trim();
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
        }
    }
}