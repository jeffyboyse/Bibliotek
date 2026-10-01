using bibliotek.Models;
using bibliotek.Services;
using System.Windows;

namespace bibliotek
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Skapa databas-kontext och UserService
            var context = new ApplicationDbContext();
            var userService = new UserService(context);

            // Starta inloggningsfönstret först
            var loginWindow = new LoginWindow(userService);
            loginWindow.Show();
        }
    }
}