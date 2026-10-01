using System.Windows;
using bibliotek.Models;

namespace bibliotek
{
    public partial class AdminWindow : Window
    {
        private readonly User _adminUser;

        // Standardkonstruktor för XAML-designern
        public AdminWindow()
        {
            InitializeComponent();
        }

        // Konstruktor som tar emot admin-användaren
        public AdminWindow(User adminUser) : this()
        {
            _adminUser = adminUser;
            lblWelcome.Text = $"Välkommen Admin, {adminUser.FirstName} {adminUser.LastName}!";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Du har loggats ut från Adminpanelen.", "Utloggad", MessageBoxButton.OK, MessageBoxImage.Information);

            // Öppna MainWindow igen när admin loggar ut
            var mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }
    }
}