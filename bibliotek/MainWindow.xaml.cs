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
        private readonly IUserService _userService = new UserService(new ApplicationDbContext());
        private User? _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            PerformSearch();
        }

        public MainWindow(User user) : this()
        {
            _currentUser = user;

            if (_currentUser != null)
            {
                BtnOpenLogin.Content = $"Inloggad: {_currentUser.FirstName}";
            }
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

        private void OpenProductView(MediaSearchResult selectedItem)
        {
            var productView = new ProductView(selectedItem, _currentUser);
            this.Content = productView;
        }

        public void ShowSearchView()
        {
            InitializeComponent();
            PerformSearch();
        }

        private void BtnOpenLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow(_userService);
            loginWindow.Show();
            this.Close();
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.Show();
        }
    }
}