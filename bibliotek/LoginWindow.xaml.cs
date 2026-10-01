using bibliotek.Models;
using bibliotek.Services;
using System.Windows;

namespace bibliotek
{
    public partial class LoginWindow : Window
    {
        private readonly IUserService _userService;
        public User? LoggedInUser { get; private set; }


        // Konstruktor som tar emot UserService (via Dependency Injection eller direkt instansering)
        public LoginWindow(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }


        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_userService == null) return;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Fyll i både e-post och lösenord.";
                return;
            }

            btnLogin.IsEnabled = false;
            lblMessage.Text = "Loggar in...";

            var result = await _userService.LoginAsync(email, password);
            if (result.Success && result.User != null)
            {
                LoggedInUser = result.User;

                if (result.User.Role)
                {
                    var adminWindow = new AdminWindow(result.User);
                    adminWindow.Show();
                    this.DialogResult = false;
                }
                else
                {
                    this.DialogResult = true; // Stänger fönstret och skickar svar till MainWindow
                }
            }
        }

        private void BtnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            if(_userService != null)
            {
                var registerWindow = new RegisterWindow(_userService);
                registerWindow.ShowDialog();
            }
        }
    }
}