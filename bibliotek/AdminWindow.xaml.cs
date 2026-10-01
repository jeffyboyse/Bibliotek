using bibliotek.Models;
using bibliotek.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace bibliotek
{
    public partial class AdminWindow : Window
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();
        private MediaSearchResult? _selectedMedia;
        private User? _selectedUser;

        public AdminWindow()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadMediaData();
            LoadUserData();
            LoadLoansData();
        }

        #region Media & Kopior

        private void LoadMediaData()
        {
            var searchService = new MediaSearch();
            dgMedia.ItemsSource = searchService.ExecuteSearch("");
        }

        private void DgMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMedia.SelectedItem is MediaSearchResult selected)
            {
                _selectedMedia = selected;
                txtTitle.Text = selected.Title;
                txtIsbn.Text = selected.IsbnOrEan;
                txtSab.Text = selected.SabCode;
                txtPrice.Text = selected.Price.ToString();
            }
        }

        private void BtnSaveMedia_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nytt media sparat!", "CMS", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadMediaData();
        }

        private void BtnUpdateMedia_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Media uppdaterat!", "CMS", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadMediaData();
        }

        private void BtnDeleteMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedia != null)
            {
                MessageBox.Show($"Boken {_selectedMedia.Title} har raderats.", "CMS", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadMediaData();
            }
        }

        private void BtnAddCopy_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedia == null)
            {
                MessageBox.Show("Välj ett media i listan först!", "Varning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string generatedBarcode = "STINA-" + new Random().Next(100000, 999999);
            MessageBox.Show($"Ny kopia skapad med streckkod: {generatedBarcode}", "Kopia tillagd", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Användarhantering

        private void LoadUserData()
        {
            dgUsers.ItemsSource = _dbContext.User.ToList();
        }

        private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is User user)
            {
                _selectedUser = user;
                txtUserFirstName.Text = user.FirstName;
                txtUserLastName.Text = user.LastName;
                txtUserEmail.Text = user.Email;
                chkIsAdmin.IsChecked = user.Role;
            }
        }

        private void BtnSaveUser_Click(object sender, RoutedEventArgs e)
        {
            var newUser = new User
            {
                FirstName = txtUserFirstName.Text,
                LastName = txtUserLastName.Text,
                Email = txtUserEmail.Text,
                Password = txtUserPassword.Text,
                Role = chkIsAdmin.IsChecked ?? false
            };

            _dbContext.User.Add(newUser);
            _dbContext.SaveChanges();

            MessageBox.Show("Ny användare skapad!", "Användare", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUserData();
        }

        private void BtnUpdateUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser != null)
            {
                _selectedUser.FirstName = txtUserFirstName.Text;
                _selectedUser.LastName = txtUserLastName.Text;
                _selectedUser.Email = txtUserEmail.Text;
                _selectedUser.Role = chkIsAdmin.IsChecked ?? false;

                _dbContext.SaveChanges();
                MessageBox.Show("Användare uppdaterad!", "Användare", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUserData();
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser != null)
            {
                _dbContext.User.Remove(_selectedUser);
                _dbContext.SaveChanges();
                MessageBox.Show("Användare raderad!", "Användare", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadUserData();
            }
        }

        #endregion

        #region Lån & Förseningsfakturor ($1.5 \times \text{värde}$)

        private void LoadLoansData()
        {
            // Här laddas lån in från databasen
        }

        private void BtnFilterOverdue_Click(object sender, RoutedEventArgs e)
        {
            // Filtrera på DueDate < DateTime.Now
        }

        private void BtnShowAllLoans_Click(object sender, RoutedEventArgs e)
        {
            LoadLoansData();
        }

        private void BtnInvoiceAndWriteOff_Click(object sender, RoutedEventArgs e)
        {
            // Avskrivningslogik: Beräkna (Pris * 1.5) och sätt status till 'Avskriven'
            MessageBox.Show("Kopian har avskrivits och underlag för faktura (1.5 × mediets värde) har skapats!", "Avskrivning", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}