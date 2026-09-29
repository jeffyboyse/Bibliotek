using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace bibliotekapp.Services
{
    //Interface, defines what the class should do
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
    }




}

