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

                // 1. Stäng av främmande nycklar under HELA seeding-processen
                await ExecuteSqlAsync(connection, "SET FOREIGN_KEY_CHECKS = 0;");

                // 2. Rensa alla tabeller
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Invoice;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Loan;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Copy;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE MediaAuthor;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Media;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE User;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Author;");
                await ExecuteSqlAsync(connection, "TRUNCATE TABLE Category;");

                // 3. Sätt in testdata

                // Category (8 st)
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

                // User (8 st)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO User (Username, Email, Password, Role) VALUES
                    ('stina_admin', 'stina@lib.se', '1234', 1),
                    ('boss_magnus', 'magnus.chef@lib.se', 'admin999', 1),
                    ('anna_låntagare', 'anna@mail.com', '1234', 0),
                    ('erik_k', 'erik.karlsson@gmail.com', 'pass123', 0),
                    ('maria_b', 'maria.berg@outlook.com', 'marian12', 0),
                    ('johan_s', 'johan.svensson@yahoo.se', 'johan2024', 0),
                    ('linda_p', 'linda.persson@hotmail.com', 'lindaPass', 0),
                    ('karin_n', 'karin.nilsson@gmail.com', 'karin88', 0);");

                // Author (10 st)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Author (First_name, Last_name) VALUES
                    ('Astrid', 'Lindgren'),
                    ('George', 'Orwell'),
                    ('J.R.R.', 'Tolkien'),
                    ('Fredrik', 'Backman'),
                    ('Camilla', 'Läckberg'),
                    ('Stephen', 'King'),
                    ('J.K.', 'Rowling'),
                    ('Agatha', 'Christie'),
                    ('Selma', 'Lagerlöf'),
                    ('Hjalmar', 'Söderberg');");

                // Media (10 st)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Media (Type, Title, Replacement_value, SAB_system, ISBN, EAN) VALUES
                    ('book', 'Bröderna Lejonhjärta', 199.00, 'Hcg', '9789129688313', NULL),
                    ('book', '1984', 149.00, 'Hc', '9780451524935', NULL),
                    ('book', 'Sagan om Ringen', 249.00, 'He', '9789113084626', NULL),
                    ('book', 'En man som heter Ove', 189.00, 'Hc', '9789137138114', NULL),
                    ('book', 'Isprinsessan', 169.00, 'Hc', '9789137122113', NULL),
                    ('book', 'Pippi Långstrump', 159.00, 'Hcg', '9789129657005', NULL),
                    ('book', 'Doktor Glas', 139.00, 'Hc', '9789174291889', NULL),
                    ('book', 'Harry Potter och De vises sten', 219.00, 'Hcb', '9789129640007', NULL),
                    ('movie', 'Inception', 199.00, 'I', NULL, '7391772322115'),
                    ('movie', 'Interstellar', 199.00, 'I', NULL, '7391772322117');");

                // MediaAuthor (8 st kopplingar)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO MediaAuthor (AuthorID, MediaID) VALUES
                    (1, 1),
                    (2, 2),
                    (3, 3),
                    (4, 4),
                    (5, 5),
                    (1, 6),
                    (10, 7),
                    (7, 8);");

                // Loan (Lån, skapar LoanID 1-8)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Loan (Return_date, Loaning_date, UserID, LastReturn_date, Bar_code) VALUES
                    ('2026-03-01', '2026-02-01', 3, '2026-03-01', 'BC001'),
                    ('2026-02-15', '2026-01-15', 4, '2026-02-15', 'BC002'),
                    ('2026-03-10', '2026-02-10', 5, '2026-03-10', 'BC003'),
                    ('2026-02-01', '2026-01-01', 6, '2026-02-01', 'BC004'),
                    ('2026-03-20', '2026-02-20', 7, '2026-03-20', 'BC005'),
                    ('2026-01-30', '2026-01-01', 3, '2026-01-25', 'BC006'),
                    ('2026-02-05', '2026-01-05', 4, '2026-02-02', 'BC007'),
                    ('2026-02-10', '2026-01-10', 5, '2026-02-08', 'BC008');");

                // Copy (Kopplas till LoanID 1-8)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Copy (Bar_code, Status, LoanID) VALUES
                    ('BC001', 0, 1),
                    ('BC002', 1, 2),
                    ('BC003', 0, 3),
                    ('BC004', 1, 4),
                    ('BC005', 0, 5),
                    ('BC006', 0, 6),
                    ('BC007', 1, 7),
                    ('BC008', 0, 8);");

                // Invoice (Fakturor)
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Invoice (Paid_date, Last_due_date, Created_date, Amount, LoanID) VALUES
                    ('2026-03-01', '2026-03-15', '2026-02-15', '100', 1),
                    ('2026-02-15', '2026-03-01', '2026-02-01', '150', 2),
                    ('2026-02-01', '2026-02-20', '2026-01-20', '50', 3),
                    ('2026-02-15', '2026-03-05', '2026-02-05', '50', 4),
                    ('2026-03-15', '2026-04-01', '2026-03-01', '200', 5);");

                // 4. Slå på främmande nycklar igen när all data är insatt
                await ExecuteSqlAsync(connection, "SET FOREIGN_KEY_CHECKS = 1;");

                MessageBox.Show("Databasen 'bibliotek' har fyllts med testdata!", "Klart", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ett databasfel uppstod:\n\n{ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hjälpmetod för att exekvera SQL-frågor
        private async Task ExecuteSqlAsync(MySqlConnection conn, string sql)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}