using MySql.Data.MySqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace bibliotek
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnSeed_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=localhost;Port=3306;Database=bibliotek;UserID=root;Password=hemligt-losenord;";

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. Stäng av främmande nycklar under seeding-processen
                await ExecuteSqlAsync(connection, "SET FOREIGN_KEY_CHECKS = 0;");

                // 2. Rensa alla tabeller i databasen
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Invoice;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Loan;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Copy;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE MediaAttribute;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Attribute;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE MediaAuthor;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Media;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE User;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Author;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Category;");

                // 3. Sätt in testdata anpassad efter schemat i Library.sql

                // Category (SAB_system, Description)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Category (SAB_system, Description) VALUES
            ('Hc', 'Svenska romaner'),
            ('Hcg', 'Barnlitteratur'),
            ('Hcb', 'Ungdomslitteratur'),
            ('He', 'Engelsk skönlitteratur'),
            ('I', 'Konst, musik och film'),
            ('O', 'Samhälls- och rättsvetenskap'),
            ('T', 'Teknik och datavetenskap'),
            ('U', 'Naturvetenskap');");

                // User (Userr_ID är AUTO_INCREMENT)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO User (FirstName, LastName, Email, Password, Role) VALUES
            ('Stina', 'Karlsson', 'stina@lib.se', '1234', 1),
            ('Magnus', 'Boss', 'magnus.chef@lib.se', 'admin999', 1),
            ('Anna', 'Folkesson', 'anna@mail.com', '1234', 0),
            ('Erik','Karlsson', 'erik.karlsson@gmail.com', 'pass123', 0),
            ('Maria','Berg', 'maria.berg@outlook.com', 'marian12', 0),
            ('Johan','Svensson', 'johan.svensson@yahoo.se', 'johan2024', 0),
            ('Linda','Persson', 'linda.persson@hotmail.com', 'lindaPass', 0),
            ('karin','Nilsson', 'karin.nilsson@gmail.com', 'karin88', 0);");

                // Author (AuthorID har inte AUTO_INCREMENT och måste anges explicita)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Author (AuthorID, First_Name, Last_Name) VALUES
            (1, 'Astrid', 'Lindgren'),
            (2, 'George', 'Orwell'),
            (3, 'J.R.R.', 'Tolkien'),
            (4, 'Fredrik', 'Backman'),
            (5, 'Camilla', 'Läckberg'),
            (6, 'Stephen', 'King'),
            (7, 'J.K.', 'Rowling'),
            (8, 'Agatha', 'Christie'),
            (9, 'Selma', 'Lagerlöf'),
            (10, 'Hjalmar', 'Söderberg');");

                // Media (Innehåller endast Title och Publish_year i databasen)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Media (MediaID, Title, Publish_year) VALUES
            (1, 'Bröderna Lejonhjärta', 1973),
            (2, '1984', 1949),
            (3, 'Sagan om Ringen', 1954),
            (4, 'En man som heter Ove', 2012),
            (5, 'Isprinsessan', 2003),
            (6, 'Pippi Långstrump', 1945),
            (7, 'Doktor Glas', 1905),
            (8, 'Harry Potter och De vises sten', 1997),
            (9, 'Inception', 2010),
            (10, 'Interstellar', 2014);");

                // Attribute (Medietyper/Egenskaper)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Attribute (AttributeID, Namn) VALUES
            (1, 'Bok'),
            (2, 'Ljudbok'),
            (3, 'Film');");

                // MediaAttribute (Kopplar Media till Attribute)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO MediaAttribute (MediaID, Value, AttributeID) VALUES
            (1, 1, 1),
            (2, 2, 1),
            (3, 3, 1),
            (4, 4, 1),
            (5, 5, 1),
            (6, 6, 1),
            (7, 7, 1),
            (8, 8, 1),
            (9, 9, 3),
            (10, 10, 3);");

                // MediaAuthor (Kopplar Media till Author)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO MediaAuthor (MediaAuthorID, MediaID, AuthorID) VALUES
            (1, 1, 1),
            (2, 2, 2),
            (3, 3, 3),
            (4, 4, 4),
            (5, 5, 5),
            (6, 6, 1),
            (7, 7, 10),
            (8, 8, 7);");

                // Copy (Exemplar kopplade till streckkoder)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Copy (Bar_code, Status, LoanID) VALUES
            ('BC001', 1, 1),
            ('BC002', 1, 2),
            ('BC003', 0, 3),
            ('BC004', 1, 4),
            ('BC005', 0, 5);");

                // Loan (Aktiva/tidigare lån)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Loan (LoanID, Return_date, Loaning_date, UserID, LastReturn_date, Bar_code) VALUES
            (1, '2026-03-01', '2026-02-01', 3, '2026-03-01', 'BC001'),
            (2, '2026-02-15', '2026-01-15', 4, '2026-02-15', 'BC002'),
            (3, '2026-03-10', '2026-02-10', 5, '2026-03-10', 'BC003'),
            (4, '2026-02-01', '2026-01-01', 6, '2026-02-01', 'BC004'),
            (5, '2026-03-20', '2026-02-20', 7, '2026-03-20', 'BC005');");

                // Invoice (Fakturor)
                await ExecuteSqlAsync(connection, @"
            INSERT INTO Invoice (Paid_date, Invoice_paid, Last_due_date, Created_date, Amount, LoanID) VALUES
            ('2026-03-01', 1, '2026-03-15', '2026-02-15', '100', 1),
            ('2026-02-15', 0, '2026-03-01', '2026-02-01', '150', 2);");

                // 4. Slå på främmande nycklar igen
                await ExecuteSqlAsync(connection, "SET FOREIGN_KEY_CHECKS = 1;");

                MessageBox.Show("Databasen 'bibliotek' har fyllts med testdata!", "Klart", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ett databasfel uppstod:\n\n{ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
            // Hjälpmetod för att exekvera SQL-frågor asynkront
            private async Task ExecuteSqlAsync(MySqlConnection conn, string sql)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
