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

                // Category 
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

                // 
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO User (User_ID, FirstName, LastName, Email, Password, Role) VALUES
                    (1, 'John', 'Doe', 'john.doe@example.com', 'hashed_pass_1', 1),
                    (2, 'Jane', 'Smith', 'jane.smith@example.com', 'hashed_pass_2', 0),
                    (3, 'Alex', 'Miller', 'alex.miller@example.com', 'hashed_pass_3', 0),
                    (4, 'Emily', 'Stone', 'emily.stone@example.com', 'hashed_pass_4', 0),
                    (5, 'Michael', 'Brown', 'michael.brown@example.com', 'hashed_pass_5', 1),
                    (6, 'Sarah', 'Wilson', 'sarah.wilson@example.com', 'hashed_pass_6', 0),
                    (7, 'David', 'King', 'david.king@example.com', 'hashed_pass_7', 0),
                    (8, 'Laura', 'Taylor', 'laura.taylor@example.com', 'hashed_pass_8', 0),
                    (9, 'Chris', 'Parker', 'chris.parker@example.com', 'hashed_pass_9', 0),
                    (10, 'Anna', 'Baker', 'anna.baker@example.com', 'hashed_pass_10', 0);");

                // Author
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Author (AuthorID, First_name, Last_name) VALUES
                    (1, 'J.R.R.', 'Tolkien'),
                    (2, 'George', 'Orwell'),
                    (3, 'Michelle', 'Obama'),
                    (4, 'Frank', 'Herbert'),
                    (5, 'J.K.', 'Rowling'),
                    (6, 'Harper', 'Lee');");

                // Media 
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Media (MediaID, Title, Publish_year) VALUES
                    (1, 'The Hobbit', 1937),
                    (2, 'The Fellowship of the Ring', 2021),
                    (3, 'Inception', 2010),
                    (4, '1984', 1949),
                    (5, 'Becoming', 2018),
                    (6, 'Interstellar', 2014),
                    (7, 'Dune', 1965),
                    (8, 'Harry Potter and the Philosopher''s Stone', 2015),
                    (9, 'The Matrix', 1999),
                    (10, 'To Kill a Mockingbird', 1960);");

                // MediaAuthor 
                await ExecuteSqlAsync(connection, @"
                    INSERT INTO MediaAuthor (AuthorID, MediaID) VALUES
                    (1, 1),   -- The Hobbit -> J.R.R. Tolkien
                    (2, 1),   -- The Fellowship of the Ring -> J.R.R. Tolkien
                    (4, 2),   -- 1984 -> George Orwell
                    (5, 3),   -- Becoming -> Michelle Obama
                    (7, 4),   -- Dune -> Frank Herbert
                    (8, 5),   -- Harry Potter -> J.K. Rowling
                    (10, 6);  -- To Kill a Mockingbird -> Harper Lee;");

                await ExecuteSqlAsync(connection, @"
                    INSERT INTO Attribute (AttributeID, Name) VALUES
                    (1, 'Director'),
                    (2, 'Narrator'),
                    (3, 'ISBN'),
                    (4, 'Runtime'),
                    (5, 'Genre'),
                    (6, 'Publisher'),
                    (7, 'Language'),
                    (8, 'AgeRating'),
                    (9, 'EAN'),
                    (10, 'PageCount');");

                await ExecuteSqlAsync(connection, @"
                    INSERT INTO MediaAttribute (MediaID, AttributeID, Value) VALUES
                    -- Media 1: The Hobbit (Book)
                    (1, 3, '9780261102217'),         -- ISBN
                    (1, 5, 'Fantasy'),               -- Genre
                    (1, 6, 'George Allen & Unwin'),  -- Publisher
                    (1, 7, 'English'),               -- Language
                    (1, 9, '9780261102217'),         -- EAN
                    (1, 10, '310'),                  -- PageCount

                    -- Media 2: The Fellowship of the Ring (Audiobook)
                    (2, 2, 'Andy Serkis'),           -- Narrator
                    (2, 3, '9780063221192'),         -- ISBN
                    (2, 4, '11:43:00'),              -- Runtime
                    (2, 5, 'Fantasy'),               -- Genre
                    (2, 6, 'HarperCollins'),         -- Publisher
                    (2, 7, 'English'),               -- Language
                    (2, 9, '9780063221192'),         -- EAN

                    -- Media 3: Inception (Movie)
                    (3, 1, 'Christopher Nolan'),     -- Director
                    (3, 4, '02:28:00'),              -- Runtime
                    (3, 5, 'Sci-Fi'),                -- Genre
                    (3, 6, 'Warner Bros.'),          -- Publisher / Studio
                    (3, 7, 'English'),               -- Language
                    (3, 8, 'PG-13'),                 -- AgeRating
                    (3, 9, '5051888049732'),         -- EAN

                    -- Media 4: 1984 (Book)
                    (4, 3, '9780451524935'),         -- ISBN
                    (4, 5, 'Dystopian'),             -- Genre
                    (4, 6, 'Secker & Warburg'),      -- Publisher
                    (4, 7, 'English'),               -- Language
                    (4, 9, '9780451524935'),         -- EAN
                    (4, 10, '328'),                  -- PageCount

                    -- Media 5: Becoming (Audiobook)
                    (5, 2, 'Michelle Obama'),        -- Narrator
                    (5, 3, '9780525633686'),         -- ISBN
                    (5, 4, '19:03:00'),              -- Runtime
                    (5, 5, 'Biography'),             -- Genre
                    (5, 6, 'Random House Audio'),    -- Publisher
                    (5, 7, 'English'),               -- Language
                    (5, 9, '9780525633686'),         -- EAN

                    -- Media 6: Interstellar (Movie)
                    (6, 1, 'Christopher Nolan'),     -- Director
                    (6, 4, '02:49:00'),              -- Runtime
                    (6, 5, 'Sci-Fi'),                -- Genre
                    (6, 6, 'Paramount Pictures'),    -- Publisher / Studio
                    (6, 7, 'English'),               -- Language
                    (6, 8, 'PG-13'),                 -- AgeRating
                    (6, 9, '5053083021931'),         -- EAN

                    -- Media 7: Dune (Book)
                    (7, 3, '9780441172719'),         -- ISBN
                    (7, 5, 'Sci-Fi'),                -- Genre
                    (7, 6, 'Chilton Books'),         -- Publisher
                    (7, 7, 'English'),               -- Language
                    (7, 9, '9780441172719'),         -- EAN
                    (7, 10, '412'),                  -- PageCount

                    -- Media 8: Harry Potter and the Philosopher's Stone (Audiobook)
                    (8, 2, 'Stephen Fry'),           -- Narrator
                    (8, 3, '9781781102367'),         -- ISBN
                    (8, 4, '08:25:00'),              -- Runtime
                    (8, 5, 'Fantasy'),               -- Genre
                    (8, 6, 'Pottermore Publishing'), -- Publisher
                    (8, 7, 'English'),               -- Language
                    (8, 9, '9781781102367'),         -- EAN

                    -- Media 9: The Matrix (Movie)
                    (9, 1, 'Lana Wachowski, Lilly Wachowski'), -- Director
                    (9, 4, '02:16:00'),              -- Runtime
                    (9, 5, 'Sci-Fi'),                -- Genre
                    (9, 6, 'Warner Bros.'),          -- Publisher / Studio
                    (9, 7, 'English'),               -- Language
                    (9, 8, 'R'),                     -- AgeRating
                    (9, 9, '7321900161830'),         -- EAN

                    -- Media 10: To Kill a Mockingbird (Book)
                    (10, 3, '9780060935467'),        -- ISBN
                    (10, 5, 'Classic Literature'),   -- Genre
                    (10, 6, 'J. B. Lippincott & Co.'),-- Publisher
                    (10, 7, 'English'),              -- Language
                    (10, 9, '9780060935467'),        -- EAN
                    (10, 10, '281');                 -- PageCount
                    ");


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

            // Helper to execute a SQL statement asynchronously using the open MySqlConnection
            private static async System.Threading.Tasks.Task ExecuteSqlAsync(MySqlConnection connection, string sql)
            {
                using var cmd = new MySqlCommand(sql, connection);
                await cmd.ExecuteNonQueryAsync();
            }
                    }

            }

