using bibliotek.Services;
using bibliotek.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bibliotek
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private readonly IUserService _userService;
        public RegisterWindow()
        {
            InitializeComponent();
        }
        public RegisterWindow(IUserService userService) : this()
        {
            _userService = userService;
        }
        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Alla fält måste fyllas i";
                return;
            }

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = false // Standard: Vanlig låntagare (false)
            };

            btnRegister.IsEnabled = false;
            lblMessage.Text = "Registrerar...";

            var result = await _userService.RegisterUserAsync(newUser, password);

            if (result.Success)
            {
                MessageBox.Show(result.Message, "Konto skapat", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Stäng fönstret så användaren kan logga in
            }
            else
            {
                lblMessage.Text = result.Message;
                btnRegister.IsEnabled = true;
            }
        }
    }
}
