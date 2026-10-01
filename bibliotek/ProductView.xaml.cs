using bibliotek.Models;
using bibliotek.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace bibliotek
{
    public partial class ProductView : UserControl
    {
        private readonly MediaSearchResult _mediaItem;
        private readonly MediaSearch _searchService = new MediaSearch();
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();
        private readonly LoanService _loanService;

        // Tills inlogget är byggt använder vi ett hårdkodat test-ID (t.ex. 1)
        private const int CurrentUserId = 1;

        public ProductView(MediaSearchResult mediaItem)
        {
            InitializeComponent();
            _mediaItem = mediaItem;
            _loanService = new LoanService(_dbContext);

            LoadData();
        }

        private void LoadData()
        {
            lblTitle.Text = _mediaItem.Title;
            lblAuthor.Text = $"Av: {_mediaItem.AuthorOrDirector} ({_mediaItem.PublishYear})";
            lblDetails.Text = $"Typ: {_mediaItem.MediaType}  |  Kategori: {_mediaItem.CategoryDescription} ({_mediaItem.SabCode})  |  Omfång: {_mediaItem.LengthOrPages}";

            // Hämta exemplar för denna titel
            var copies = _searchService.ExecuteSearch(_mediaItem.Title);
            dgCopies.ItemsSource = copies;
        }

        private async void BtnBorrow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MediaSearchResult selectedCopy)
            {
                var result = await _loanService.BorrowCopyAsync(CurrentUserId, selectedCopy.Barcode);
                MessageBox.Show(result.Message, "Lån", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.Success)
                {
                    LoadData(); // Blixtsnabb uppdatering av statusen
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Växla tillbaka till sökVy i fönstret
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowSearchView();
            }
        }
    }
}