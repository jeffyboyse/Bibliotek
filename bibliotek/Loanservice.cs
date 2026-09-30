using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using bibliotek.Models;

namespace bibliotek.Services
{

    public interface ILoanService
    {
        Task<(bool Success, string Message)> BorrowCopyAsync(int userID, string barCode);
        Task<(bool Success, string Message)> ReturnCopyAsync(string barCode);
    }
    //encapsulated field for the database
    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;

        public LoanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> BorrowCopyAsync(int userID, string barCode)
        {
            var user = await _context.User.FindAsync(userID);
            if (user == null) return (false, "Användaren hittades inte.");

            var copy = await _context.Copy.FindAsync(barCode);
            if (copy == null) return (false, "Kopior för detta media finns tyvärr inte");

            if (copy.LoanID != null) return (false, "Kopian är redan utlånad");

            // 1. Skapa den nya låneposten
            var newLoan = new Loan
            {
                User_ID = userID, // Ändrat från UserID till User_ID
                Bar_code = barCode,
                Loaning_date = DateTime.Now,
                LastReturn_date = DateTime.Now.AddDays(21),
                Return_date = null
            };

            _context.Loan.Add(newLoan);
            await _context.SaveChangesAsync();

            // 2. Koppla det nya lånet till bokexemplaret
            copy.LoanID = newLoan.LoanID;
            copy.Status = 1; //1 = utlånad
            await _context.SaveChangesAsync();

            return (true, "Lånet har genomförts! Återlämas senast:" + newLoan.LastReturn_date.ToShortDateString());
        }

        public async Task<(bool Success, string Message)> ReturnCopyAsync(string barCode)
        {
            var copy = await _context.Copy.FindAsync(barCode);
            if (copy == null) return (false, "Bokexemplaret hittades inte.");
            if (copy.LoanID == null) return (false, "Boken är inte utlånad.");

            var loan = await _context.Loan.FindAsync(copy.LoanID);
            if (loan == null) return (false, "Lånet hittades inte.");

            DateTime today = DateTime.Now;
            loan.Return_date = today;
            copy.LoanID = null;
            copy.Status = 0; //0 = Tillgänglig

            await _context.SaveChangesAsync();
            string returnMessage = "Boken har återlämnats i tid!";

            if (today > loan.LastReturn_date)
            {
                var media = await _context.Media.FindAsync(copy.MediaID);
                decimal mediaValue = media?.MediaValue ?? 100m;  //Om media finns (inte är null), hämta MediaValue. Om media däremot är null, krascha inte programmet utan returnera bara null.
                decimal InvoiceAmount = mediaValue * 1.5m; // 1.5 * Mediavalue;

                var invoice = new Invoice
                {
                    LoanID = loan.LoanID,
                    Amount = InvoiceAmount.ToString("0.00"), //Formatterar beloppet
                    Created_date = today,
                    Last_due_date = today.AddDays(30),
                    Invoice_paid = false,
                    Paid_date = null
                };
                _context.Invoice.Add(invoice);
                returnMessage = $"Boken var försenad. En faktura på {InvoiceAmount} kr (1.5x bokens värde) har skapats.";

            }
            await _context.SaveChangesAsync();
            return (true, returnMessage);
        }

    }
}

