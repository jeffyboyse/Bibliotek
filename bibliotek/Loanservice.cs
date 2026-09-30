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
                Return_date = null
            };

            _context.Loan.Add(newLoan);
            await _context.SaveChangesAsync();

            // 2. Koppla det nya lånet till bokexemplaret
            copy.LoanID = newLoan.LoanID;
            await _context.SaveChangesAsync();

            return (true, "Lånet har genomförts!");
        }

        public async Task<(bool Success, string Message)> ReturnCopyAsync(string barCode)
        {
            var copy = await _context.Copy.FindAsync(barCode);
            if (copy == null) return (false, "Bokexemplaret hittades inte.");
            if (copy.LoanID == null) return (false, "Boken är inte utlånad.");

            var loan = await _context.Loan.FindAsync(copy.LoanID);
            if (loan == null) return (false, "Lånet hittades inte.");

            loan.Return_date = DateTime.Now;
            copy.LoanID = null;

            await _context.SaveChangesAsync();
            return (true, "Boken har återlämnats!");
        }

    }
}

