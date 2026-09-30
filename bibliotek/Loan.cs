using System;

using System;

namespace bibliotek.Models
{
    public class Loan
    {
        public int LoanID { get; set; }
        public DateTime Loaning_date { get; set; }
        public DateTime? Return_date { get; set; }
        public int User_ID { get; set; }
        public string Bar_code { get; set; }
    }
}