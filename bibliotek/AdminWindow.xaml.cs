using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using bibliotek.Models;
using bibliotek.Services;

namespace bibliotek
{
    public partial class AdminWindow : Window
    {
        private readonly User? _adminUser;

        public AdminWindow()
        {
            InitializeComponent();
            LoadAllData();
            LoadAttributesList();
        }

        public AdminWindow(User adminUser) : this()
        {
            _adminUser = adminUser;
            lblAdminHeader.Text = $"⚙️ Adminpanel — Inloggad som {adminUser.FirstName} {adminUser.LastName}";
        }

        private void LoadAllData()
        {
            try
            {
                using var db = new ApplicationDbContext();

                // Ladda media, användare och kopior
                dgMedia.ItemsSource = db.Media.ToList();
                dgUsers.ItemsSource = db.User.ToList();
                dgCopies.ItemsSource = db.Copy.ToList();

                // Hämta aktiva lån
                dgActiveLoans.ItemsSource = db.Loan
                    .Where(l => l.Return_date == null)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte ladda databasdata: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAttributesList()
        {
            try
            {
                using var db = new ApplicationDbContext();
                cmbAttributes.ItemsSource = db.Attribute.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte ladda attributlista: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- NÄR NÅGOT MEDIA MARKERAS I TABELLEN ---
        private void DgMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMedia.SelectedItem is Media selectedMedia)
            {
                txtSelectedMediaId.Text = selectedMedia.MediaID.ToString();
                LoadMediaAttributes(selectedMedia.MediaID);
            }
            else
            {
                txtSelectedMediaId.Clear();
                dgMediaAttributes.ItemsSource = null;
            }
        }

        private void LoadMediaAttributes(int mediaId)
        {
            try
            {
                using var db = new ApplicationDbContext();

                // Hämta alla EAV-rader för valt MediaID
                var mediaAttrs = db.MediaAttribute
                    .Include(ma => ma.Attribute)
                    .Where(ma => ma.MediaID == mediaId)
                    .Select(ma => new
                    {
                        ma.MediaID,
                        ma.AttributeID,
                        Attributnamn = ma.Attribute.Name,
                        Varde = ma.Value
                    })
                    .ToList();

                dgMediaAttributes.ItemsSource = mediaAttrs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte ladda media-attribut: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- SPARA / UPPDATERA EAV-ATTRIBUT ---
        private void BtnSaveAttribute_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtSelectedMediaId.Text, out int mediaId))
            {
                MessageBox.Show("Välj ett media i tabellen först.", "Validering", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbAttributes.SelectedValue == null || string.IsNullOrWhiteSpace(txtAttributeValue.Text))
            {
                MessageBox.Show("Välj ett attribut i listan och ange ett värde.", "Validering", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int attributeId = (int)cmbAttributes.SelectedValue;
            string attrValue = txtAttributeValue.Text.Trim();

            try
            {
                using var db = new ApplicationDbContext();

                var existingAttr = db.MediaAttribute
                    .FirstOrDefault(ma => ma.MediaID == mediaId && ma.AttributeID == attributeId);

                if (existingAttr != null)
                {
                    existingAttr.Value = attrValue;
                }
                else
                {
                    var newAttr = new MediaAttribute
                    {
                        MediaID = mediaId,
                        AttributeID = attributeId,
                        Value = attrValue
                    };
                    db.MediaAttribute.Add(newAttr);
                }

                db.SaveChanges();
                MessageBox.Show("Attributet sparades!", "Framgång", MessageBoxButton.OK, MessageBoxImage.Information);

                txtAttributeValue.Clear();
                LoadMediaAttributes(mediaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte spara attribut: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- TA BORT EAV-ATTRIBUT ---
        private void BtnDeleteAttribute_Click(object sender, RoutedEventArgs e)
        {
            if (dgMediaAttributes.SelectedItem == null)
            {
                MessageBox.Show("Markera ett attribut i nedre tabellen att ta bort.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                dynamic selectedItem = dgMediaAttributes.SelectedItem;
                int mediaId = selectedItem.MediaID;
                int attributeId = selectedItem.AttributeID;

                using var db = new ApplicationDbContext();
                var itemToDelete = db.MediaAttribute
                    .FirstOrDefault(ma => ma.MediaID == mediaId && ma.AttributeID == attributeId);

                if (itemToDelete != null)
                {
                    db.MediaAttribute.Remove(itemToDelete);
                    db.SaveChanges();
                    LoadMediaAttributes(mediaId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte ta bort attribut: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MEDIA-HANTERING ---
        private void BtnSaveMedia_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Ange en titel för mediet.", "Validering", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new ApplicationDbContext();

                // 1. Skapa Media utan fast Type-kolumn
                var newMedia = new Media
                {
                    Title = txtTitle.Text.Trim()
                };

                db.Media.Add(newMedia);
                db.SaveChanges(); // Sparar så vi får ett giltigt MediaID

                // 2. Om du vill spara mediatyp som ett EAV-attribut automatiskt:
                if (cmbMediaType.SelectedItem is ComboBoxItem selectedTypeItem)
                {
                    string selectedTypeName = selectedTypeItem.Content?.ToString() ?? "Bok";

                    // Sök efter attributet "Mediatyp" eller "Typ" i databasen
                    var typeAttribute = db.Attribute.FirstOrDefault(a => a.Name == "Mediatyp" || a.Name == "Typ");

                    if (typeAttribute != null)
                    {
                        var mediaTypeEav = new MediaAttribute
                        {
                            MediaID = newMedia.MediaID,
                            AttributeID = typeAttribute.AttributeID,
                            Value = selectedTypeName
                        };
                        db.MediaAttribute.Add(mediaTypeEav);
                        db.SaveChanges();
                    }
                }

                MessageBox.Show("Nytt media har skapats!", "Framgång", MessageBoxButton.OK, MessageBoxImage.Information);

                txtTitle.Clear();
                cmbMediaType.SelectedIndex = -1;
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte spara media: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteMedia_Click(object sender, RoutedEventArgs e)
        {
            if (dgMedia.SelectedItem is Media selectedMedia)
            {
                if (MessageBox.Show($"Vill du ta bort '{selectedMedia.Title}'?", "Bekräfta", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new ApplicationDbContext();
                        db.Media.Remove(selectedMedia);
                        db.SaveChanges();
                        LoadAllData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Kunde inte ta bort media: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // --- KOPIOR-HANTERING ---
        private void BtnCreateCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCopyMediaId.Text, out int mediaId) || string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Ange ett giltigt Media ID och Streckkod.", "Validering", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new ApplicationDbContext();
                var newCopy = new Copy
                {
                    MediaID = mediaId,
                    Bar_code = txtBarcode.Text.Trim(),
                    Status = 1
                };

                db.Copy.Add(newCopy);
                db.SaveChanges();

                MessageBox.Show("Ny kopia registrerad!", "Framgång", MessageBoxButton.OK, MessageBoxImage.Information);
                txtBarcode.Clear();
                txtCopyMediaId.Clear();
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte skapa kopia: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteCopy_Click(object sender, RoutedEventArgs e)
        {
            if (dgCopies.SelectedItem is Copy selectedCopy)
            {
                try
                {
                    using var db = new ApplicationDbContext();
                    db.Copy.Remove(selectedCopy);
                    db.SaveChanges();
                    LoadAllData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kunde inte ta bort kopia: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- ANVÄNDAR-HANTERING ---
        private void BtnSaveUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtUserEmail.Text))
            {
                MessageBox.Show("Ange minst förnamn och e-post.", "Validering", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new ApplicationDbContext();
                var newUser = new User
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtUserEmail.Text.Trim(),
                    Password = BCrypt.Net.BCrypt.HashPassword("Lösenord123!"),
                    Role = chkIsAdmin.IsChecked ?? false
                };

                db.User.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Ny användare skapad!", "Framgång", MessageBoxButton.OK, MessageBoxImage.Information);
                txtFirstName.Clear();
                txtLastName.Clear();
                txtUserEmail.Clear();
                chkIsAdmin.IsChecked = false;

                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte skapa användare: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is User selectedUser)
            {
                try
                {
                    using var db = new ApplicationDbContext();
                    db.User.Remove(selectedUser);
                    db.SaveChanges();
                    LoadAllData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kunde inte ta bort användare: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- FÖRSENINGSKONTROLL ---
        private void BtnCheckOverdue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var db = new ApplicationDbContext();
                var now = DateTime.Now;

                var overdueLoans = db.Loan
                    .Where(l => l.Return_date == null && l.LastReturn_date < now)
                    .ToList();

                if (!overdueLoans.Any())
                {
                    MessageBox.Show("Inga försenade lån hittades.", "Förseningskontroll", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int count = 0;
                foreach (var loan in overdueLoans)
                {
                    var invoice = new Invoice
                    {
                        LoanID = loan.LoanID,
                        Amount = "150.00",
                        Created_date = now,
                        Last_due_date = now.AddDays(30),
                        Invoice_paid = false
                    };

                    db.Invoice.Add(invoice);
                    count++;
                }

                db.SaveChanges();
                MessageBox.Show($"Kontroll klar! {count} st fakturor underlag för försenade lån har genererats.", "Fakturering", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte utföra förseningskontroll: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}