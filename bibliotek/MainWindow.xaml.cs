using System;
using System.Linq;
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
            BuildSearchViewUI();
            ShowSearchView();
        }

        // Skapar sökvyn programmatiskt eller så kan den ligga i en egen UserControl
        private void BuildSearchViewUI()
        {
            _searchGrid = new Grid { Margin = new Thickness(15) };
            _searchGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _searchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Sökfält
            var topPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };
            topPanel.Children.Add(new TextBlock { Text = "Sök:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });

            _txtSearch = new TextBox { Width = 300, Height = 30, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), FontSize = 14 };
            _txtSearch.KeyDown += (s, e) => { if (e.Key == Key.Enter) PerformSearch(); };
            topPanel.Children.Add(_txtSearch);

            var btnSearch = new Button { Content = "Sök", Width = 80, Height = 30, Margin = new Thickness(0, 0, 15, 0), IsDefault = true };
            btnSearch.Click += (s, e) => PerformSearch();
            topPanel.Children.Add(btnSearch);

            var btnSeed = new Button { Content = "Reset & Seed DB", Width = 120, Height = 30 };
            btnSeed.Click += SeedButton_Click;
            topPanel.Children.Add(btnSeed);

            Grid.SetRow(topPanel, 0);
            _searchGrid.Children.Add(topPanel);

            // DataGrid med ENKELKLICK (SelectionChanged)
            _dgSearchResults = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Cursor = Cursors.Hand
            };

            _dgSearchResults.Columns.Add(new DataGridTextColumn { Header = "Titel", Binding = new System.Windows.Data.Binding("Title"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
            _dgSearchResults.Columns.Add(new DataGridTextColumn { Header = "Författare/Regissör", Binding = new System.Windows.Data.Binding("AuthorOrDirector"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            _dgSearchResults.Columns.Add(new DataGridTextColumn { Header = "Typ", Binding = new System.Windows.Data.Binding("MediaType"), Width = 100 });
            _dgSearchResults.Columns.Add(new DataGridTextColumn { Header = "Kategori", Binding = new System.Windows.Data.Binding("CategoryDescription"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            // Navigera direkt vid 1 klick
            _dgSearchResults.SelectionChanged += DgSearchResults_SelectionChanged;

            Grid.SetRow(_dgSearchResults, 1);
            _searchGrid.Children.Add(_dgSearchResults);
        }
        //Överlagrad konstruktor som tar emot den inloggade användaren från LoginWindow
        public MainWindow(User user) : this()
        {
            _currentUser = user;
            // Exempel: Sätt fönstrets titel med användarens namn
            this.Title = $"Bibliotekssystem - Inloggad som {user.FirstName} {user.LastName}";
        }

        public void ShowSearchView()
        {
            MainContent.Content = _searchGrid;
            PerformSearch();
        }

        private void DgSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dgSearchResults.SelectedItem is MediaSearchResult selectedItem)
            {
                // Nollställ markeringen så att man kan klicka på samma rad igen senare
                _dgSearchResults.SelectedItem = null;

                // Växla direkt till produktsidan i SAMMA fönster utan fördröjning
                MainContent.Content = new ProductView(selectedItem);
            }
        }

        private void PerformSearch()
        {
            try
            {
                string query = _txtSearch.Text.Trim();
                var rawResults = _searchService.ExecuteSearch(query);

                var groupedResults = rawResults
                    .GroupBy(m => string.IsNullOrEmpty(m.IsbnOrEan) || m.IsbnOrEan == "-" ? m.Title : m.IsbnOrEan)
                    .Select(g => g.First())
                    .ToList();

                _dgSearchResults.ItemsSource = groupedResults;
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