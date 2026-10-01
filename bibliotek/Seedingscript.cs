using MySql.Data.MySqlClient;
using System;
using System.Text;
using System.Threading.Tasks;

namespace bibliotek
{
    public static class Seedingscript
    {
        public static async Task SeedAsync(string connectionString)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            await ExecuteSqlAsync(connection, "SET FOREIGN_KEY_CHECKS = 0;");

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

            // 1. Kategorier
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

            // 2. Attribut (Inkluderar Description = 12)
            await ExecuteSqlAsync(connection, @"
                INSERT INTO Attribute (AttributeID, Name) VALUES
                (1, 'SAB'), (2, 'Narrator'), (3, 'ISBN'), (4, 'Runtime'),
                (5, 'Genre'), (6, 'Price'), (7, 'Language'), (8, 'AgeRating'),
                (9, 'EAN'), (10, 'PageCount'), (11, 'Type'), (12, 'Description');");

            // 3. Riktiga Författare & Regissörer
            var (authors, books, audiobooks, movies) = GetRealMediaData();

            var authorSql = new StringBuilder("INSERT INTO Author (AuthorID, First_name, Last_name) VALUES ");
            for (int i = 0; i < authors.Length; i++)
            {
                var parts = authors[i].Split(' ', 2);
                string firstName = parts[0].Replace("'", "''");
                string lastName = parts.Length > 1 ? parts[1].Replace("'", "''") : "";
                authorSql.Append($"({i + 1}, '{firstName}', '{lastName}'),");
            }
            authorSql.Length--;
            await ExecuteSqlAsync(connection, authorSql.ToString());

            // 4. Bygg upp Media, MediaAuthor, MediaAttribute och Copy
            var mediaSql = new StringBuilder("INSERT INTO Media (MediaID, Title, Publish_year) VALUES ");
            var mediaAuthorSql = new StringBuilder("INSERT INTO MediaAuthor (AuthorID, MediaID) VALUES ");
            var mediaAttrSql = new StringBuilder("INSERT INTO MediaAttribute (MediaID, AttributeID, Value) VALUES ");
            var copySql = new StringBuilder("INSERT INTO Copy (Bar_code, Status, LoanID, MediaID) VALUES ");

            int mediaId = 1;
            int barcodeCounter = 10000;
            Random rnd = new Random(42);

            // BÖCKER (1 - 50)
            for (int i = 0; i < books.Length; i++, mediaId++)
            {
                var item = books[i];
                string cleanTitle = item.Title.Replace("'", "''");
                string cleanDesc = item.Description.Replace("'", "''");

                mediaSql.Append($"({mediaId}, '{cleanTitle}', {item.Year}),");
                mediaAuthorSql.Append($"({item.AuthorId}, {mediaId}),");

                mediaAttrSql.Append($"({mediaId}, 11, 'Bok'),");
                mediaAttrSql.Append($"({mediaId}, 1, '{item.Sab}'),");
                mediaAttrSql.Append($"({mediaId}, 3, '{item.Isbn}'),");
                mediaAttrSql.Append($"({mediaId}, 6, '{item.Price}kr'),"); // Format: 170kr
                mediaAttrSql.Append($"({mediaId}, 10, '{item.Pages}'),");
                mediaAttrSql.Append($"({mediaId}, 7, '{item.Language}'),");
                mediaAttrSql.Append($"({mediaId}, 12, '{cleanDesc}'),");

                int copyCount = rnd.Next(1, 3);
                for (int c = 0; c < copyCount; c++)
                {
                    barcodeCounter++;
                    copySql.Append($"('BC{barcodeCounter}', {rnd.Next(0, 2)}, NULL, {mediaId}),");
                }
            }

            // LJUDBÖCKER (51 - 100)
            for (int i = 0; i < audiobooks.Length; i++, mediaId++)
            {
                var item = audiobooks[i];
                string cleanTitle = item.Title.Replace("'", "''");
                string cleanDesc = item.Description.Replace("'", "''");

                mediaSql.Append($"({mediaId}, '{cleanTitle}', {item.Year}),");
                mediaAuthorSql.Append($"({item.AuthorId}, {mediaId}),");

                mediaAttrSql.Append($"({mediaId}, 11, 'Ljudbok'),");
                mediaAttrSql.Append($"({mediaId}, 1, '{item.Sab}'),");
                mediaAttrSql.Append($"({mediaId}, 3, '{item.Isbn}'),");
                mediaAttrSql.Append($"({mediaId}, 6, '{item.Price}'),"); // Format: 170kr
                mediaAttrSql.Append($"({mediaId}, 4, '{item.Runtime}'),");
                mediaAttrSql.Append($"({mediaId}, 7, '{item.Language}'),");
                mediaAttrSql.Append($"({mediaId}, 12, '{cleanDesc}'),");

                barcodeCounter++;
                copySql.Append($"('BC{barcodeCounter}', {rnd.Next(0, 2)}, NULL, {mediaId}),");
            }

            // FILMER (101 - 150)
            for (int i = 0; i < movies.Length; i++, mediaId++)
            {
                var item = movies[i];
                string cleanTitle = item.Title.Replace("'", "''");
                string cleanDesc = item.Description.Replace("'", "''");

                mediaSql.Append($"({mediaId}, '{cleanTitle}', {item.Year}),");
                mediaAuthorSql.Append($"({item.AuthorId}, {mediaId}),");

                mediaAttrSql.Append($"({mediaId}, 11, 'Film'),");
                mediaAttrSql.Append($"({mediaId}, 1, '{item.Sab}'),");
                mediaAttrSql.Append($"({mediaId}, 9, '{item.Ean}'),");
                mediaAttrSql.Append($"({mediaId}, 6, '{item.Price}kr'),"); // Format: 170kr
                mediaAttrSql.Append($"({mediaId}, 4, '{item.Runtime}'),");
                mediaAttrSql.Append($"({mediaId}, 7, '{item.Language}'),");
                mediaAttrSql.Append($"({mediaId}, 12, '{cleanDesc}'),");

                barcodeCounter++;
                copySql.Append($"('BC{barcodeCounter}', {rnd.Next(0, 2)}, NULL, {mediaId}),");
            }

            mediaSql.Length--;
            mediaAuthorSql.Length--;
            mediaAttrSql.Length--;
            copySql.Length--;

            await ExecuteSqlAsync(connection, mediaSql.ToString());
            await ExecuteSqlAsync(connection, mediaAuthorSql.ToString());
            await ExecuteSqlAsync(connection, mediaAttrSql.ToString());
            await ExecuteSqlAsync(connection, copySql.ToString());

            await ExecuteSqlAsync(connection, "SET FOREIGN_KEY_CHECKS = 1;");
        }

        private static async Task ExecuteSqlAsync(MySqlConnection conn, string sql)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        private class BookData
        {
            public string Title; public int AuthorId; public int Year; private string _sab; public string Sab => _sab;
            public string Isbn; public int Pages; public int Price; public string Language; public string Description;
            public BookData(string t, int a, int y, string s, string i, int p, int pr, string lang, string desc)
            { Title = t; AuthorId = a; Year = y; _sab = s; Isbn = i; Pages = p; Price = pr; Language = lang; Description = desc; }
        }

        private class AudioData
        {
            public string Title; public int AuthorId; public int Year; private string _sab; public string Sab => _sab;
            public string Isbn; public string Runtime; public int Price; public string Language; public string Description;
            public AudioData(string t, int a, int y, string s, string i, string r, int pr, string lang, string desc)
            { Title = t; AuthorId = a; Year = y; _sab = s; Isbn = i; Runtime = r; Price = pr; Language = lang; Description = desc; }
        }

        private class MovieData
        {
            public string Title; public int AuthorId; public int Year; private string _sab; public string Sab => _sab;
            public string Ean; public string Runtime; public int Price; public string Language; public string Description;
            public MovieData(string t, int a, int y, string s, string e, string r, int pr, string lang, string desc)
            { Title = t; AuthorId = a; Year = y; _sab = s; Ean = e; Runtime = r; Price = pr; Language = lang; Description = desc; }
        }

        private static (string[] Authors, BookData[] Books, AudioData[] Audiobooks, MovieData[] Movies) GetRealMediaData()
        {
            string[] authors = new string[]
            {
                "Astrid Lindgren", "August Strindberg", "Karin Boye", "Selma Lagerlöf", "Vilhelm Moberg",
                "Fredrik Backman", "Stieg Larsson", "Camilla Läckberg", "J.K. Rowling", "George Orwell",
                "J.R.R. Tolkien", "Ernest Hemingway", "Franz Kafka", "Stephen King", "Albert Camus",
                "Hjalmar Söderberg", "Lukas Moodysson", "Jonas Jonasson", "Mari Jungstedt", "Lars Kepler",
                "Alex Schulman", "David Lagercrantz", "Jan Guillou", "Maj Sjöwall", "Per Wahlöö",
                "Jo Nesbø", "Agatha Christie", "Dan Brown", "Yuval Noah Harari", "Walter Isaacson",
                "Ingmar Bergman", "Roy Andersson", "Lasse Hallström", "Ruben Östlund", "Christopher Nolan",
                "Steven Spielberg", "Quentin Tarantino", "Martin Scorsese", "Denis Villeneuve", "Hayao Miyazaki",
                "Peter Jackson", "Ridley Scott", "James Cameron", "Greta Gerwig", "David Fincher",
                "Alfred Hitchcock", "Stanley Kubrick", "Guillermo del Toro", "Francis Ford Coppola", "Wes Anderson"
            };

            BookData[] books = new BookData[]
            {
                new BookData("Pippi Långstrump", 1, 1945, "Hcg", "9789129657470", 144, 179, "Svenska", "Klassisk barnbok om världens starkaste flicka och hennes äventyr i Villa Villekulla."),
                new BookData("Röda rummet", 2, 1879, "Hc", "9789174291186", 350, 129, "Svenska", "En satirisk skildring av det stockholmska samhället och kultur- och finansvärlden."),
                new BookData("Kallocain", 3, 1940, "Hc", "9789113093222", 284, 149, "Svenska", "Dystopisk framtidsroman om en sanningsserumsliknande kemisk substans i en totalitär stat."),
                new BookData("Nils Holgerssons underbara resa", 4, 1906, "Hcg", "9789174292107", 600, 199, "Svenska", "En geografisk och uppfostrande resa genom Sverige på ryggen av en tamgås."),
                new BookData("Utvandrarna", 5, 1949, "Hc", "9789113083650", 540, 189, "Svenska", "Första delen i eposet om några smålänningars utvandring till Nordamerika på 1800-talet."),
                new BookData("En man som heter Ove", 6, 2012, "Hc", "9789137138114", 336, 169, "Svenska", "En hjärtvärmande och humorfylld berättelse om en vresig man och hans nya grannar."),
                new BookData("Män som hatar kvinnor", 7, 2005, "Hc", "9789113014081", 550, 159, "Svenska", "Mörkt spänningsdrama om en försvunnen flicka och en udda investigatorisk duo."),
                new BookData("Isprinsessan", 8, 2003, "Hc", "9789137121284", 360, 149, "Svenska", "Kriminalroman förlagd till Fjällbacka där hemskheter under ytan uppdagas."),
                new BookData("Harry Potter och De vises sten", 9, 1997, "Hcb", "9789129640007", 320, 229, "Svenska", "Inledningen på sagan om en föräldralös pojke som upptäcker att han är en trollkarl."),
                new BookData("1984", 10, 1949, "He", "9780141036144", 328, 139, "Engelska", "Mörk dystopi om massövervakning, sanningskontroll och Storebror."),
                new BookData("Sagan om ringen", 11, 1954, "He", "9789113084534", 520, 249, "Svenska", "Det stora fantasyeposet om hobbiten Frodos uppdrag att förstöra den enda ringen."),
                new BookData("Den gamle och havet", 12, 1952, "He", "9789113083889", 128, 119, "Svenska", "En åldrad kubansk fiskares kamp mot en gigantisk svärdfisk ute på havet."),
                new BookData("Processen", 13, 1925, "He", "9789174291889", 240, 139, "Svenska", "Josef K. vaknar upp en morgon och blir häktad utan att få veta vad han är anklagad för."),
                new BookData("Främlingen", 15, 1942, "He", "9789113084121", 160, 129, "Svenska", "Existentiell klassiker om Meursault och hans likgiltiga förhållningssätt till livet."),
                new BookData("Doktor Glas", 16, 1905, "Hc", "9789174291122", 170, 119, "Svenska", "Psykologisk dagboksroman om moralkval och idén om det lovliga mordet."),
                new BookData("Hundraåringen som klev ut genom fönstret", 18, 2009, "Hc", "9789100118020", 390, 159, "Svenska", "Humoristisk skröna om Allan Karlsson som rymmer från ålderdomshemmet på sin födelsedag."),
                new BookData("Den dubbla tystnaden", 19, 2007, "Hc", "9789100118020", 380, 149, "Svenska", "Kriminalfall på Gotland präglat av gamla synder och lokala hemligheter."),
                new BookData("Hypnotisören", 20, 2009, "Hc", "9789100123512", 600, 179, "Svenska", "Intensiv och mörk thriller om jakten på en skoningslös mördare."),
                new BookData("Överlevarna", 21, 2020, "Hc", "9789100182601", 290, 199, "Svenska", "Tre bröder återvänder till torpet där en avgörande händelse förändrade allt."),
                new BookData("Det som inte dödar oss", 22, 2015, "Hc", "9789113061016", 500, 189, "Svenska", "Fortsättning på Millennium-serien med Lisbeth Salander och Mikael Blomkvist."),
                new BookData("Ondskan", 23, 1981, "Hcb", "9789113084008", 300, 149, "Svenska", "Om våld och kamratuppfostran på internatskolan Stjärnsberg."),
                new BookData("Mannen på taket", 24, 1971, "Hc", "9789100121112", 220, 129, "Svenska", "Klassisk polistriller där Martin Beck utreder ett uppmärksammat sjukhusmord."),
                new BookData("Fladdermusmannen", 26, 1997, "Hc", "9789100115005", 340, 139, "Svenska", "Harry Hole reser till Sydney för att utreda mordet på en norsk kvinna."),
                new BookData("Och så var de bara en", 27, 1939, "He", "9789100181116", 250, 129, "Svenska", "Tio främlingar lockas till en öde ö där de mördas en efter en."),
                new BookData("Da Vinci-koden", 28, 2003, "He", "9789100109004", 510, 169, "Svenska", "Symbolforskare Robert Langdon dras in i en uråldrig hemlighet i Paris."),
                new BookData("Sapiens", 29, 2011, "O", "9789127143418", 450, 249, "Svenska", "En djupdykning i mänsklighetens historia från apor till rymdfarare."),
                new BookData("Steve Jobs", 30, 2011, "T", "9789100126933", 630, 229, "Svenska", "Den exklusiva biografin om grundaren av Apple."),
                new BookData("Mio, min Mio", 1, 1954, "Hcg", "9789129657487", 180, 149, "Svenska", "Bo Vilhelm Olsson förs bort till Landet i Fjärran för att kämpa mot riddar Kato."),
                new BookData("Bröderna Lejonhjärta", 1, 1973, "Hcg", "9789129657494", 230, 169, "Svenska", "En saga om kärlek, döden och striden mot tyrannen Tengil i Nangijala."),
                new BookData("Ronja Rövardotter", 1, 1981, "Hcg", "9789129657500", 200, 169, "Svenska", "Om vänskapen mellan två rövarbarn i en förtrollad skog."),
                new BookData("Mäster Olof", 2, 1872, "Hc", "9789174291193", 190, 109, "Svenska", "Historiskt drama om reformatorn Olaus Petri och hans konflikt med Gustav Vasa."),
                new BookData("Giftas", 2, 1884, "Hc", "9789174291209", 280, 119, "Svenska", "Novellsamling som kritiskt granskar äktenskapet och könsrollerna."),
                new BookData("Drottningens juvelsmycke", 3, 1939, "Hc", "9789113093239", 210, 129, "Svenska", "Historisk roman kring mordet på Gustav III och den enigmatiske Tintomara."),
                new BookData("Gösta Berlings saga", 4, 1891, "Hc", "9789174292114", 420, 159, "Svenska", "Romantisk skröna om kavaljererna på Ekeby i Värmland."),
                new BookData("Kejsarn av Portugallien", 4, 1914, "Hc", "9789174292121", 210, 139, "Svenska", "En faders gränslösa kärlek till sin dotter drivs in i galenskap."),
                new BookData("Nybyggarna", 5, 1956, "Hc", "9789113083667", 610, 189, "Svenska", "Tredje delen om svenskarnas hårda liv i Minnesotas vildmark."),
                new BookData("Sista brevet till Sverige", 5, 1959, "Hc", "9789113083674", 580, 189, "Svenska", "Sista delen i Utvandrareposet om livets slutskede i det nya landet."),
                new BookData("Folkmedicin", 6, 2019, "U", "9789137138121", 250, 179, "Svenska", "Insiktsfull granskning av traditionella läkeörter och folklig tro."),
                new BookData("Flickan som lekte med elden", 7, 2006, "Hc", "9789113014098", 650, 169, "Svenska", "Lisbeth Salander blir misstänkt för dubbelmord och jagas av polisen."),
                new BookData("Luftslottet som sprängdes", 7, 2007, "Hc", "9789113014104", 700, 169, "Svenska", "Den slutgiltiga uppgörelsen med de mörka krafterna i skuggan av staten."),
                new BookData("Predikanten", 8, 2004, "Hc", "9789137121291", 380, 149, "Svenska", "Gamla familjehemligheter väcks till liv vid ett fynd i Kungsklyftan."),
                new BookData("Stenhuggaren", 8, 2005, "Hc", "9789137121307", 420, 149, "Svenska", "Ett druknat barn hittas i ett fisknät och startar en uppskakande utredning."),
                new BookData("Harry Potter och Hemligheternas kammare", 9, 1998, "Hcb", "9789129640014", 340, 229, "Svenska", "Mörka krafter hotar återigen Hogwarts när en hemlig kammare öppnas."),
                new BookData("Harry Potter och Fången från Azkaban", 9, 1999, "Hcb", "9789129640021", 430, 239, "Svenska", "Beryktade fången Sirius Black rymmer för att söka upp Harry Potter."),
                new BookData("Djurfarmen", 10, 1945, "He", "9780141036137", 120, 119, "Engelska", "Satirisk allegori över maktmissbruk och korruption på en bondgård."),
                new BookData("Sagan om de två tornen", 11, 1954, "He", "9789113084541", 450, 249, "Svenska", "Ringens brödraskap har splittrats och kampen mot Sauron tätnar."),
                new BookData("Sagan om konungens återkomst", 11, 1955, "He", "9789113084558", 490, 249, "Svenska", "Det slutgiltiga kriget om Midgård avgörs vid Domedagsberget."),
                new BookData("För vem klämtar klockan", 12, 1940, "He", "9789113083889", 480, 159, "Svenska", "En amerikan deltar i spanska inbördeskriget med uppdrag att spränga en bro."),
                new BookData("Förvandlingen", 13, 1915, "He", "9789174291896", 90, 99, "Svenska", "Gregor Samsa vaknar upp en morgon förvandlad till en jättelik insekt."),
                new BookData("Pestens tid", 14, 1978, "He", "9789113085000", 1100, 279, "Svenska", "Ett biologiskt vapen raderar mänskligheten och skapar en kamp mellan gott och ont.")
            };

            AudioData[] audiobooks = new AudioData[]
            {
                new AudioData("Pippi Långstrump", 1, 2015, "Hcg", "9789129699005", "02:45:00", 129, "Svenska", "Inläsning av den klassiska barnboken om Pippi och hennes vänner."),
                new AudioData("Kallocain", 3, 2018, "Hc", "9789113099019", "07:30:00", 149, "Svenska", "Ljudboksupplaga av Karin Boyes klassiska framtidsdystopi."),
                new AudioData("Utvandrarna", 5, 2010, "Hc", "9789113089021", "18:15:00", 199, "Svenska", "Ljudboksversion av Mobergs epos om utvandringen till Amerika."),
                new AudioData("En man som heter Ove", 6, 2013, "Hc", "9789137139036", "09:20:00", 159, "Svenska", "Inläst av George Fant – den älskade romanen som ljudbok."),
                new AudioData("Män som hatar kvinnor", 7, 2006, "Hc", "9789113019048", "16:40:00", 179, "Svenska", "Spännande inläsning av första delen i Millennium-trilogin."),
                new AudioData("Harry Potter och De vises sten", 9, 2002, "Hcb", "9789129649055", "08:10:00", 199, "Svenska", "Magisk inläsning av Krister Henriksson om den unge trollkarlen."),
                new AudioData("1984", 10, 2015, "He", "9780141039061", "11:30:00", 139, "Engelska", "Engelsk originalinläsning av Orwells dystopiska mästerverk."),
                new AudioData("Sagan om ringen", 11, 2008, "He", "9789113089076", "19:50:00", 219, "Svenska", "Storslagen inläsning av Tolkiens fantastyklassiker."),
                new AudioData("Hundraåringen som klev ut genom fönstret", 18, 2010, "Hc", "9789100119089", "11:45:00", 149, "Svenska", "Underhållande inläsning av den berömda skrönan."),
                new AudioData("Överlevarna", 21, 2020, "Hc", "9789100189090", "06:50:00", 169, "Svenska", "Stämningsfull inläsning av Alex Schulmans kritikerrosade roman."),
                new AudioData("Ondskan", 23, 2005, "Hcb", "9789113089106", "08:15:00", 139, "Svenska", "Jan Guillous uppgörelse med våld och internatliv som ljudbok."),
                new AudioData("Da Vinci-koden", 28, 2004, "He", "9789100109110", "17:00:00", 169, "Svenska", "Rafflande ljudbok om jakt på uråldriga gåtor och koder."),
                new AudioData("Sapiens", 29, 2015, "O", "9789127149120", "15:10:00", 229, "Svenska", "Faktaspäckad och medryckande ljudbok om mänsklighetens historia."),
                new AudioData("Steve Jobs", 30, 2012, "T", "9789100129132", "21:30:00", 219, "Svenska", "Biografin om Apples grundare i tillgängligt ljudboksformat."),
                new AudioData("Mio, min Mio", 1, 2016, "Hcg", "9789129659146", "03:40:00", 119, "Svenska", "Astrid Lindgrens stämningsfulla saga inläst för yngre lyssnare."),
                new AudioData("Bröderna Lejonhjärta", 1, 2016, "Hcg", "9789129659153", "05:10:00", 129, "Svenska", "Berättelsen om Skorpan och Jonathan i Nangijala som ljudbok."),
                new AudioData("Ronja Rövardotter", 1, 2017, "Hcg", "9789129659160", "04:50:00", 129, "Svenska", "Ljudbok om rövarliv, vårskrik och vänskap över gränser."),
                new AudioData("Doktor Glas", 16, 2011, "Hc", "9789174299175", "04:15:00", 109, "Svenska", "Hjalmar Söderbergs klassiska Stockholmsskildring som ljudbok."),
                new AudioData("Isprinsessan", 8, 2005, "Hc", "9789137129181", "10:30:00", 139, "Svenska", "Camilla Läckbergs pusseldäckare i Fjällbacka som ljudbok."),
                new AudioData("Hypnotisören", 20, 2010, "Hc", "9789100129194", "15:45:00", 169, "Svenska", "Lars Keplers spänningsroman som intensiv ljudbok."),
                new AudioData("Fladdermusmannen", 26, 2008, "Hc", "9789100119202", "11:00:00", 129, "Svenska", "Första fallet för kriminalkommissarie Harry Hole som ljudbok."),
                new AudioData("Och så var de bara en", 27, 2019, "He", "9789100189218", "07:20:00", 129, "Svenska", "Agatha Christies mest berömda mysterium i ljudformat."),
                new AudioData("Flickan som lekte med elden", 7, 2007, "Hc", "9789113019222", "18:30:00", 179, "Svenska", "Andra delen i Millennium-serien som oavbruten ljudbok."),
                new AudioData("Luftslottet som sprängdes", 7, 2008, "Hc", "9789113019239", "20:10:00", 179, "Svenska", "Den rafflande finalen i Stieg Larssons Millennium-trilogi."),
                new AudioData("Predikanten", 8, 2006, "Hc", "9789137129242", "11:15:00", 139, "Svenska", "Kriminalroman från Västkusten inläst som ljudbok."),
                new AudioData("Stenhuggaren", 8, 2007, "Hc", "9789137129259", "12:40:00", 139, "Svenska", "Mörk deckare från Fjällbacka i tillgängligt ljudformat."),
                new AudioData("Röda rummet", 2, 2012, "Hc", "9789174299267", "10:00:00", 119, "Svenska", "Strindbergs samhällssatir inläst av professionell skådespelare."),
                new AudioData("Nils Holgersson", 4, 2014, "Hcg", "9789174299274", "14:20:00", 149, "Svenska", "Klassiska resan genom Sverige som medryckande ljudbok."),
                new AudioData("Processen", 13, 2016, "He", "9789174299281", "07:05:00", 119, "Svenska", "Kafkas absurda domstolsprocess inläst på svenska."),
                new AudioData("Den gamle och havet", 12, 2015, "He", "9789113089298", "03:15:00", 99, "Svenska", "Kort och kraftfull inläsning av Hemingways nobelprisbelönade verk."),
                new AudioData("Harry Potter och Hemligheternas kammare", 9, 2003, "Hcb", "9789129649309", "09:40:00", 199, "Svenska", "Ljudboksversion av andra boken i den populära serien."),
                new AudioData("Harry Potter och Fången från Azkaban", 9, 2004, "Hcb", "9789129649316", "12:10:00", 209, "Svenska", "Tredje boken om Harry Potter inläst för unga lyssnare."),
                new AudioData("Djurfarmen", 10, 2016, "He", "9780141039320", "03:30:00", 99, "Engelska", "Engelsk ljudboksupplaga av George Orwells kända fabel."),
                new AudioData("Sagan om de två tornen", 11, 2009, "He", "9789113089335", "16:40:00", 219, "Svenska", "Ljudboksupplaga av mitten delen i Ringens Triologi."),
                new AudioData("Sagan om konungens återkomst", 11, 2010, "He", "9789113089342", "18:00:00", 219, "Svenska", "Upplösningen på Tolkiens mästerverk i storslagen inläsning."),
                new AudioData("Pestens tid", 14, 2018, "He", "9789113089359", "47:30:00", 299, "Svenska", "Maffig och lång ljudboksupplevelse av Stephen Kings klassiker."),
                new AudioData("Kejsarn av Portugallien", 4, 2013, "Hc", "9789174299365", "06:10:00", 119, "Svenska", "Gripande inläsning av Selma Lagerlöfs älskade roman."),
                new AudioData("Nybyggarna", 5, 2011, "Hc", "9789113089372", "19:20:00", 189, "Svenska", "Tredje delen i Vilhelm Mobergs Utvandrarepos som ljudbok."),
                new AudioData("Sista brevet till Sverige", 5, 2012, "Hc", "9789113089389", "18:50:00", 189, "Svenska", "Sista delen om svenskarna i Amerika i ljudboksformat."),
                new AudioData("Den dubbla tystnaden", 19, 2008, "Hc", "9789100119393", "11:30:00", 139, "Svenska", "Gotländsk kriminalgåva inläst som spännande ljudbok."),
                new AudioData("Det som inte dödar oss", 22, 2015, "Hc", "9789113069401", "15:00:00", 179, "Svenska", "Millennium-fortsättningen inläst av professionell röst."),
                new AudioData("Mannen på taket", 24, 2010, "Hc", "9789100129415", "06:45:00", 119, "Svenska", "Sjöwall Wahlöös klassiska polischef Martin Beck som ljudbok."),
                new AudioData("Gösta Berlings saga", 4, 2015, "Hc", "9789174299421", "13:10:00", 139, "Svenska", "Värmländsk skröna i stämningsfull inläsning."),
                new AudioData("Mäster Olof", 2, 2016, "Hc", "9789174299438", "05:30:00", 99, "Svenska", "Dramatisk inläsning av Strindbergs historiska skådespel."),
                new AudioData("Giftas", 2, 2017, "Hc", "9789174299445", "08:00:00", 109, "Svenska", "Strindbergs novellserie uppläst i ljudboksform."),
                new AudioData("Drottningens juvelsmycke", 3, 2018, "Hc", "9789113099453", "06:20:00", 119, "Svenska", "Almqvists romantiska och gåtfulla klassiker som ljudbok."),
                new AudioData("Folkmedicin", 6, 2020, "U", "9789137139463", "07:45:00", 159, "Svenska", "Faktabok om folktro och läkedom i ljudboksformat."),
                new AudioData("För vem klämtar klockan", 12, 2016, "He", "9789113089471", "15:20:00", 149, "Svenska", "Hemingways klassiska krigsroman som ljudbok."),
                new AudioData("Förvandlingen", 13, 2017, "He", "9789174299482", "02:10:00", 89, "Svenska", "Kort och intensiv inläsning av Kafkas kända berättelse."),
                new AudioData("Pestmätaren", 15, 2018, "He", "9789113089495", "04:30:00", 109, "Svenska", "Existentiellt verk uppläst på svenska.")
            };

            MovieData[] movies = new MovieData[]
            {
                new MovieData("Det sjunde inseglet", 31, 1957, "I", "7333018001011", "01:36:00", 149, "Svenska", "Riddaren Antonius Block spelar schack med Döden under pestens härjningar."),
                new MovieData("Smultronstället", 31, 1957, "I", "7333018001028", "01:31:00", 149, "Svenska", "Den gamle professorn Isak Borg gör en bilresa som blir en inre tillbakablick."),
                new MovieData("Fanny och Alexander", 31, 1982, "I", "7333018001035", "03:08:00", 199, "Svenska", "Storslagen familjesaga i Uppsala genom två barns ögon."),
                new MovieData("Sånger från andra våningen", 32, 2000, "I", "7333018001042", "01:38:00", 129, "Svenska", "Surrealistisk och tragikomisk skildring av det moderna västerländska samhället."),
                new MovieData("En duva satt på en gren och funderade på tillvaron", 32, 2014, "I", "7333018001059", "01:41:00", 139, "Svenska", "Två absurda försäljare tar oss med på en filosofisk resa genom mänskligheten."),
                new MovieData("Mitt liv som hund", 33, 1985, "I", "7333018001066", "01:41:00", 129, "Svenska", "Pojken Ingemar skickas till släktingar i Småland under 1950-talet."),
                new MovieData("Ciderhusreglerna", 33, 1999, "I", "7333018001073", "02:06:00", 119, "Engelska", "En föräldralös pojke växer upp på ett barnhem och söker sin egen väg i livet."),
                new MovieData("The Square", 34, 2017, "I", "7333018001080", "02:22:00", 149, "Svenska", "Guldpalmen-belönad satir om en museichef och ett kontroversiellt konstverk."),
                new MovieData("Triangle of Sadness", 34, 2022, "I", "7333018001097", "02:27:00", 169, "Engelska", "Skarp satir om modevärlden och superrika på en lyxkryssning."),
                new MovieData("Inception", 35, 2010, "I", "7333018001103", "02:28:00", 149, "Engelska", "Sci-fi-thriller om en tjuv som stjäl företagshemligheter genom drömmar."),
                new MovieData("Interstellar", 35, 2014, "I", "7333018001110", "02:49:00", 159, "Engelska", "En grupp upptäcktsresande reser genom ett maskhål i rymden för att rädda mänskligheten."),
                new MovieData("Oppenheimer", 35, 2023, "I", "7333018001127", "03:00:00", 219, "Engelska", "Det historiska dramat om J. Robert Oppenheimer och utvecklingen av atombomben."),
                new MovieData("Jurassic Park", 36, 1993, "I", "7333018001134", "02:07:00", 129, "Engelska", "Klonade dinosaurier bryter sig loss på en nöjespark på en öde ö."),
                new MovieData("Schindler's List", 36, 1993, "I", "7333018001141", "03:15:00", 169, "Engelska", "Gripanade verklighetsbaserat drama om Oskar Schindler under förintelsen."),
                new MovieData("Pulp Fiction", 37, 1994, "I", "7333018001158", "02:34:00", 139, "Engelska", "Sammankopplade berättelser om yrkesmördare, boxare och maffia i Los Angeles."),
                new MovieData("Inglourious Basterds", 37, 2009, "I", "7333018001165", "02:33:00", 149, "Engelska", "En grupp judisk-amerikanska soldater planerar lönnmord under andra världskriget."),
                new MovieData("Taxi Driver", 38, 1976, "I", "7333018001172", "01:54:00", 129, "Engelska", "Mörk studie av en ensam krigsveteran som kör taxibil i New York."),
                new MovieData("Goodfellas", 38, 1990, "I", "7333018001189", "02:26:00", 139, "Engelska", "Klassisk maffiafilm som följer Henry Hills uppgång och fall inom maffian."),
                new MovieData("Arrival", 39, 2016, "I", "7333018001196", "01:56:00", 139, "Engelska", "En lingvist får i uppdrag att kommunicera med utomjordiska varelser på jorden."),
                new MovieData("Dune", 39, 2021, "I", "7333018001202", "02:35:00", 179, "Engelska", "Storslagen filmatisering av Frank Herberts sci-fi-epos på ökenplaneten Arrakis."),
                new MovieData("Blade Runner 2049", 39, 2017, "I", "7333018001219", "02:44:00", 169, "Engelska", "En ny replikantjägare nystar upp en långt begravd hemlighet som kan stänga ner samhället."),
                new MovieData("Spirited Away", 40, 2001, "I", "7333018001226", "02:05:00", 149, "Japanska", "En ung flicka går vilse i en värld av andar och måste rädda sina föräldrar."),
                new MovieData("Min granne Totoro", 40, 1988, "I", "7333018001233", "01:26:00", 139, "Japanska", "Två unga systrar upptäcker att den närliggande skogen bebos av vänliga andar."),
                new MovieData("Prinsessan Mononoke", 40, 1997, "I", "7333018001240", "02:14:00", 149, "Japanska", "En ung krigare dras in i en konflikt mellan skogens övernaturliga väsen och en gruvstad."),
                new MovieData("Sagan om ringen: Härskarringen", 41, 2001, "I", "7333018001257", "02:58:00", 159, "Engelska", "Frodo Bagger påbörjar sin farfyllda resa mot Domedagsberget."),
                new MovieData("Sagan om de två tornen", 41, 2002, "I", "7333018001264", "02:59:00", 159, "Engelska", "Brödraskapet är splittrat men kampen mot Saurons arméer fortsätter."),
                new MovieData("Sagan om konungens återkomst", 41, 2003, "I", "7333018001271", "03:21:00", 169, "Engelska", "Det slutgiltiga slaget om Midgård utkämpas vid Minas Tirith."),
                new MovieData("Alien", 42, 1979, "I", "7333018001288", "01:57:00", 129, "Engelska", "Besättningen på ett rymdskepp ställs inför en livsfarlig utomjordisk organism."),
                new MovieData("Gladiator", 42, 2000, "I", "7333018001295", "02:35:00", 149, "Engelska", "En förrådd romersk general söker hämnd mot den korrupte kejsaren."),
                new MovieData("Blade Runner", 42, 1982, "I", "7333018001301", "01:57:00", 139, "Engelska", "En detektiv får i uppdrag att spåra upp och 'pensionera' undflyende replikanter."),
                new MovieData("Titanic", 43, 1997, "I", "7333018001318", "03:14:00", 149, "Engelska", "En passionerad kärlekssaga ombord på det ödesdigra fartyget Titanic."),
                new MovieData("Avatar", 43, 2009, "I", "7333018001325", "02:42:00", 159, "Engelska", "En paraplegisk marinsoldat skickas till månen Pandora på ett unikt uppdrag."),
                new MovieData("Terminator 2: Domedagen", 43, 1991, "I", "7333018001332", "02:17:00", 139, "Engelska", "En omprogrammerad cyborg skickas för att skydda den unge John Connor."),
                new MovieData("Barbie", 44, 2023, "I", "7333018001349", "01:54:00", 179, "Engelska", "Barbie drabbas av en existentiell kris och lämnar Barbieland för den riktiga världen."),
                new MovieData("Lady Bird", 44, 2017, "I", "7333018001356", "01:34:00", 129, "Engelska", "En konstnärligt lagd gymnasieelev navigerar relationer och sin uppväxt."),
                new MovieData("Fight Club", 45, 1999, "I", "7333018001363", "02:19:00", 139, "Engelska", "En sömnlös kontorsarbetare skapar en underjordisk slagsmålsklubb."),
                new MovieData("Se7en", 45, 1995, "I", "7333018001370", "02:07:00", 139, "Engelska", "Två detektiver jagar en seriemördare som använder de sju dödssynderna som motiv."),
                new MovieData("The Social Network", 45, 2010, "I", "7333018001387", "02:00:00", 139, "Engelska", "Historien om grundandet av Facebook och de efterföljande stämningarna."),
                new MovieData("Psycho", 46, 1960, "I", "7333018001394", "01:49:00", 119, "Engelska", "En sekreterare på flykt stannar till på ett isolerat motell som drivs av Norman Bates."),
                new MovieData("Fönstret åt gården", 46, 1954, "I", "7333018001400", "01:52:00", 119, "Engelska", "En rullstolsbunden fotograf spionerar på sina grannar och misstänker ett mord."),
                new MovieData("2001: Ett rymdäventyr", 47, 1968, "I", "7333018001417", "02:29:00", 139, "Engelska", "En episk resa mot Jupiter där en superdator uppvisar ett allt märkligare beteende."),
                new MovieData("The Shining", 47, 1980, "I", "7333018001424", "02:26:00", 139, "Engelska", "En familj blir isolerad på ett vinterstängt hotell där mörka krafter påverkar fadern."),
                new MovieData("A Clockwork Orange", 47, 1971, "I", "7333018001431", "02:16:00", 129, "Engelska", "I en framtida dystopi undergår en våldsam ungdom ett kontroversiellt behandlingsprogram."),
                new MovieData("Pans labyrint", 48, 2006, "I", "7333018001448", "01:58:00", 139, "Spanska", "I efterdyningarna av spanska inbördeskriget flyr en flicka in i en mörk och magisk sagovärld."),
                new MovieData("Gudfadern", 49, 1972, "I", "7333018001455", "02:55:00", 159, "Engelska", "Den åldrande patriarchaten i en maffiafamilj överlämnar kontrollen till sin motvillige son."),
                new MovieData("Gudfadern del II", 49, 1974, "I", "7333018001462", "03:22:00", 159, "Engelska", "Porträtt av Vito Corleones tidiga år parallellt med sonen Michaels expanderande imperium."),
                new MovieData("Apocalypse Now", 49, 1979, "I", "7333018001479", "02:27:00", 149, "Engelska", "Under Vietnamkriget skickas en kapten på ett hemligt uppdrag för att likvidera en galen överste."),
                new MovieData("The Grand Budapest Hotel", 50, 2014, "I", "7333018001486", "01:39:00", 139, "Engelska", "En legendomspunnen concierge och hans protege dras in i en stöld av en ovärderlig målning."),
                new MovieData("Moonrise Kingdom", 50, 2012, "I", "7333018001493", "01:34:00", 129, "Engelska", "Två unga älskande rymmer från sin hemstad på en ö och utlöser en omfattande sökning."),
                new MovieData("Fantastic Mr. Fox", 50, 2009, "I", "7333018001509", "01:27:00", 129, "Engelska", "En slug räv måste överlista tre elaka bönder för att skydda sin familj och sin gemenskap.")
            };

            return (authors, books, audiobooks, movies);
        }
    }
}