using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace bibliotek
{
    public class MediaSearchResult
    {
        public string IsbnOrEan { get; set; }           // Identifierare för användaren istället för internt MediaID
        public string MediaType { get; set; }           // Bok, Ljudbok, Film
        public string Title { get; set; }
        public string AuthorOrDirector { get; set; }    // Författare eller regissör
        public int PublishYear { get; set; }
        public decimal Price { get; set; }
        public string SabCode { get; set; }
        public string CategoryDescription { get; set; }
        public string LengthOrPages { get; set; }        // "320 sidor" eller "02:15:00"
        public string Barcode { get; set; }             // Exemplarets streckkod
        public bool IsBorrowed { get; set; }
        public string StatusText => IsBorrowed ? "Utlånad" : "Tillgänglig";
    }

    public class MediaSearch
    {
        private readonly string _connectionString = "Server=127.0.0.1;Database=bibliotek;Uid=root;Pwd=hemligt-losenord;";

        public List<MediaSearchResult> ExecuteSearch(string searchTerm)
        {
            var results = new List<MediaSearchResult>();

            string query = @"
                SELECT 
                    -- Hämtar ISBN (3) i första hand, annars EAN (9)
                    IFNULL(ma_isbn.Value, IFNULL(ma_ean.Value, '-')) AS IsbnOrEan,
                    
                    Media.Title,
                    Media.Publish_year,
                    IFNULL(ma_type.Value, 'Bok') AS MediaType,
                    IFNULL(ma_price.Value, '0') AS RawPrice,
                    CONCAT(IFNULL(Author.First_Name, ''), ' ', IFNULL(Author.Last_Name, '')) AS AuthorOrDirector,
                    
                    Category.SAB_system AS SabCode,
                    Category.Description AS CategoryDescription,

                    -- Dynamisk visning: Sidor för böcker, speltid för film/ljudbok
                    CASE 
                        WHEN ma_type.Value = 'Bok' THEN CONCAT(IFNULL(ma_pages.Value, '-'), ' sidor')
                        ELSE IFNULL(ma_runtime.Value, '-')
                    END AS LengthOrPages,

                    Copy.Bar_code,
                    IFNULL(Copy.Status, 0) AS Status

                FROM Media

                -- Kräv att författare/regissör finns
                LEFT JOIN MediaAuthor ON Media.MediaID = MediaAuthor.MediaID
                LEFT JOIN Author      ON MediaAuthor.AuthorID = Author.AuthorID

                -- Fysiska exemplar
                LEFT JOIN Copy ON Media.MediaID = Copy.MediaID

                -- Attribut-kopplingar
                LEFT JOIN MediaAttribute ma_type    ON Media.MediaID = ma_type.MediaID    AND ma_type.AttributeID = 11
                LEFT JOIN MediaAttribute ma_price   ON Media.MediaID = ma_price.MediaID   AND ma_price.AttributeID = 6
                LEFT JOIN MediaAttribute ma_pages   ON Media.MediaID = ma_pages.MediaID   AND ma_pages.AttributeID = 10
                LEFT JOIN MediaAttribute ma_runtime ON Media.MediaID = ma_runtime.MediaID AND ma_runtime.AttributeID = 4
                LEFT JOIN MediaAttribute ma_isbn    ON Media.MediaID = ma_isbn.MediaID    AND ma_isbn.AttributeID = 3
                LEFT JOIN MediaAttribute ma_ean     ON Media.MediaID = ma_ean.MediaID     AND ma_ean.AttributeID = 9

                -- KRÄV KATEGORI & SAB-KOD (INNER JOIN sorterar bort media utan kategori)
                INNER JOIN MediaAttribute ma_sab ON Media.MediaID = ma_sab.MediaID AND ma_sab.AttributeID = 1
                INNER JOIN Category              ON ma_sab.Value = Category.SAB_system

                WHERE 
                    -- KRÄV ATT ETT ISBN ELLER EAN FINNS
                    (ma_isbn.Value IS NOT NULL OR ma_ean.Value IS NOT NULL)
                    
                    -- Sökfilter
                    AND (
                        Media.Title LIKE @Search
                        OR Author.First_Name LIKE @Search
                        OR Author.Last_Name LIKE @Search
                        OR CONCAT(Author.First_Name, ' ', Author.Last_Name) LIKE @Search
                        OR Category.SAB_system LIKE @Search
                        OR Category.Description LIKE @Search
                        OR Copy.Bar_code LIKE @Search
                    )

                GROUP BY 
                    Media.MediaID, 
                    Copy.Bar_code, 
                    IsbnOrEan, 
                    MediaType, 
                    RawPrice, 
                    AuthorOrDirector, 
                    SabCode, 
                    CategoryDescription, 
                    LengthOrPages, 
                    Copy.Status;";

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string rawPrice = reader.GetString("RawPrice");
                            string cleanPrice = Regex.Replace(rawPrice, @"[^\d]", "");
                            decimal.TryParse(cleanPrice, out decimal parsedPrice);

                            results.Add(new MediaSearchResult
                            {
                                IsbnOrEan = reader.GetString("IsbnOrEan"),
                                MediaType = reader.GetString("MediaType"),
                                Title = reader.GetString("Title"),
                                AuthorOrDirector = string.IsNullOrWhiteSpace(reader.GetString("AuthorOrDirector"))
                                    ? "Okänd"
                                    : reader.GetString("AuthorOrDirector").Trim(),
                                PublishYear = reader.GetInt32("Publish_year"),
                                Price = parsedPrice,
                                SabCode = reader.GetString("SabCode"),
                                CategoryDescription = reader.GetString("CategoryDescription"),
                                LengthOrPages = reader.GetString("LengthOrPages"),
                                Barcode = reader.IsDBNull(reader.GetOrdinal("Bar_code")) ? "Ingen kopia" : reader.GetString("Bar_code"),
                                IsBorrowed = reader.GetBoolean("Status")
                            });
                        }
                    }
                }
            }

            return results;
        }
    }
}