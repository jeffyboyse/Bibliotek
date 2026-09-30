using System;
namespace bibliotek.Models
{
        public class Copy
        {
            public string Bar_code { get; set; } // Primärnyckel (String, ej null)
         
            public int Status { get; set; }

            public int? LoanID { get; set; } // Främmandenyckel (int? tillåter null när boken står i hyllan)

            public int MediaID { get; set; } // Främmande nyckel med media

        }
    
}

