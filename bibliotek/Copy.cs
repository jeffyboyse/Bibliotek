using System;
namespace bibliotek.Models
{
    public class Copy
    {
        public string Bar_code {  get; set; }

        public int Status { get; set; }

        //Främmandenyckel koppling

        public int LoanID {  get; set; }
    }
}

