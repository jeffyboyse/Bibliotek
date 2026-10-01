using bibliotek.Models;
using bibliotek.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
namespace bibliotek
{
    public partial class MainWindow : Window
    {
        private readonly MediaSearch _searchService = new MediaSearch();
        private readonly IUserService _userService = new UserService(new ApplicationDbContext());
        private User? _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            PerformSearch();
            UpdateAdminButtonVisibility();
        }

        public MainWindow(User user) : this()
        {
            _currentUser = user;

            if (_currentUser != null)
            {
                BtnOpenLogin.Content = $"Inloggad: {_currentUser.FirstName}";
            }
            UpdateAdminButtonVisibility();
        }




        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            string query = txtSearch.Text;
            var results = _searchService.ExecuteSearch(query);
            lbSearchResults.ItemsSource = results;
        }

        private void BtnOpenProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MediaSearchResult selectedItem)
            {
                OpenProductView(selectedItem);
            }
        }

        private void LblTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is MediaSearchResult selectedItem)
            {
                OpenProductView(selectedItem);
            }
        }

        private object? _searchContent;

        private void OpenProductView(MediaSearchResult selectedItem)
        {
            _searchContent = this.Content; // spara sökvyn
            var productView = new ProductView(selectedItem, _currentUser);
            this.Content = productView;
        }

        public void ShowSearchView()
        {
            if (_searchContent != null)
            {
                this.Content = _searchContent; // återställ sökvyn
                _searchContent = null;
            }
            PerformSearch();
        }

        private void BtnOpenLogin_Click(object sender, RoutedEventArgs e)
        {

            if (_currentUser != null)
            {
                var answer = MessageBox.Show(
                    $"Vill du logga ut {_currentUser.FirstName}?",
                    "Logga ut",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    LogOut();
                }
                return;
            }
            var loginWindow = new LoginWindow(_userService);
            // 1. Öppna som dialog (krävs för att DialogResult ska fungera)
            bool? result = loginWindow.ShowDialog();
            if (result == true && loginWindow.LoggedInUser != null)
            {
                _currentUser = loginWindow.LoggedInUser;
                BtnOpenLogin.Content = $"Inloggad: {_currentUser.FirstName}";

                UpdateAdminButtonVisibility(); // Gör Admin-knappen synlig om _currentUser.Role är true
            }


        }

        private void LogOut()
        {
            _currentUser = null;
            BtnOpenLogin.Content = "Logga in"; // use whatever text the button has in your XAML
            UpdateAdminButtonVisibility();     // hides the admin button again

            // Close any admin window that is still open
            foreach (var w in Application.Current.Windows.OfType<AdminWindow>().ToList())
            {
                w.Close();
            }
        }

        private void UpdateAdminButtonVisibility()
        {
            if (_currentUser != null && _currentUser.Role)
            {
                BtnAdmin.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAdmin.Visibility= Visibility.Collapsed;
            }
        }
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null && _currentUser.Role)
            {
                var adminWindow = new AdminWindow(_currentUser);
                adminWindow.Show();
                
            }
            else
            {
                MessageBox.Show("Du har inte behörighet att komma åt Adminpanelen.", "Åtkomst nekad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private async void SeedButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.Show();
        }
    }
}