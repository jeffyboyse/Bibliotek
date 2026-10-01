using System.Windows;
using bibliotek.Services;

namespace bibliotek
{
    public partial class LoginWindow : Window
    {
        private readonly IUserService _userService;

        // Konstruktor som tar emot UserService (via Dependency Injection eller direkt instansering)
        public LoginWindow(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }


        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            // Enkel validering innan anrop
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Fyll i både e-post och lösenord.";
                return;
            }

            // Inaktivera knappen medan vi väntar på databasen
            btnLogin.IsEnabled = false;
            lblMessage.Text = "Loggar in...";

            // Kör logik från UserService
            var result = await _userService.LoginAsync(email, password);

            if (result.Success && result.User != null)
            {
                MessageBox.Show(result.Message, "Inloggad", MessageBoxButton.OK, MessageBoxImage.Information);

                // Styr användaren till rätt fönster beroende på Role (1 = Admin, 0 = Låntagare)
                if (result.Success && result.User != null)
                {
                    MessageBox.Show(result.Message, "Inloggad", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (result.User.Role)
                    {
                        // På grund av att AdminWindow inte finns ännu skickar vi vidare till MainWindow tills vidare
                        //var adminWindow = new MainWindow(result.User);
                        // adminWindow.Show();
                        var mainWindow = new MainWindow(result.User);
                        mainWindow.Show();

                    }
                }
                else
                {
                    // Öppna Huvudfönstret för låntagare
                    var mainWindow = new MainWindow(result.User);
                    mainWindow.Show();
                }

                // Stäng inloggningsfönstret
                this.Close();
            }
            else
            {
                lblMessage.Text = result.Message;
                btnLogin.IsEnabled = true;
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