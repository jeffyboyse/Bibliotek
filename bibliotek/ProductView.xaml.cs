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
        private readonly User? _currentUser;

        public ProductView(MediaSearchResult mediaItem, User? currentUser = null)
        {
            InitializeComponent();
            _mediaItem = mediaItem;
            _currentUser = currentUser;
            _loanService = new LoanService(_dbContext);

            LoadData();
        }

        private void LoadData()
        {
            // 1. Titel längst upp
            lblTitle.Text = _mediaItem.Title;

            // 2. Detaljinformation
            lblAuthor.Text = $"Författare/Regissör: {_mediaItem.AuthorOrDirector}";
            lblMediaType.Text = $"Mediatyp: {_mediaItem.MediaType}";
            lblPublishYear.Text = $"Utgivningsår: {_mediaItem.PublishYear}";

            lblIsbn.Text = $"ISBN/EAN: {_mediaItem.IsbnOrEan}";
            lblCategory.Text = $"Kategori: {_mediaItem.CategoryDescription} ({_mediaItem.SabCode})";
            lblLength.Text = $"Omfång: {_mediaItem.LengthOrPages}";
            lblPrice.Text = $"Inköpsvärde: {_mediaItem.Price:C0}";

            // 3. Hämta exemplar
            var copies = _searchService.ExecuteSearch(_mediaItem.Title);
            dgCopies.ItemsSource = copies;
        }

        private async void BtnBorrow_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Du måste vara inloggad för att kunna låna.", "Inloggning krävs", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender is Button button && button.DataContext is MediaSearchResult selectedCopy)
            {
                // Kolla om din User-modell använder .Id eller .User_Id
                int userId = _currentUser.User_ID; // Matchar public int User_ID

                var result = await _loanService.BorrowCopyAsync(userId, selectedCopy.Barcode);
                MessageBox.Show(result.Message, "Lånebekräftelse", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.Success)
                {
                    LoadData();
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowSearchView();
            }
        }
    }
}