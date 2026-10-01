using bibliotek.Models;
using bibliotek.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace bibliotek
{
    public partial class MainWindow : Window
    {
        private readonly MediaSearch _searchService = new MediaSearch();
        private User? _currentUser; // Håller koll på den inloggade användaren (om någon)

        public MainWindow()
        {
            InitializeComponent();
            PerformSearch(); // Ladda alla medier direkt vid start
        }
        public MainWindow(User user) : this()
        {
            _currentUser = user;
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            string query = txtSearch.Text;
            var results = _searchService.ExecuteSearch(query);

            // Sätt resultatet till kortvyn (ListBox)
            lbSearchResults.ItemsSource = results;
        }

        // Klick på "Visa tillgänglighet / Produktsida"-knappen
        private void BtnOpenProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MediaSearchResult selectedItem)
            {
                OpenProductView(selectedItem);
            }
        }

        // Klick direkt på titeln i kortet
        private void LblTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is MediaSearchResult selectedItem)
            {
                OpenProductView(selectedItem);
            }
        }

        private void OpenProductView(MediaSearchResult selectedItem)
        {
            // Skapa ProductView och visa den i fönstret
            var productView = new ProductView(selectedItem, _currentUser);

            // Ersätt fönstrets innehåll med ProductView (eller öppna som nytt fönster om du föredrar det)
            this.Content = productView;
        }

        public void ShowSearchView()
        {
            // Återställer MainWindow-innehållet när man klickar "Tillbaka" i ProductView
            InitializeComponent();
            PerformSearch();
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.Show();
        }
    }
}