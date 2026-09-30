using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotek.Models
{
    [Table("Invoice")]
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public string Amount { get; set; }
        public DateTime Created_date { get; set; }
        public DateTime Last_due_date {  get; set; }
        public DateTime? Paid_date { get; set; }
        public bool Invoice_paid { get; set; }
        public int LoanID { get; set; }
    }
}