using System;

namespace bibliotek.Models
{
    public class Loan
    {
        public int LoanID { get; set; }

        public DateTime Return_date {  get; set; }

        public DateTime Loaning_date { get; set; }

        public DateTime LastReturn_date {  get; set; } // Null om boken inte lämnats tillbaka än


        public int UserID { get; set; }
        
        public string Bar_code {  get; set; }
    }
}