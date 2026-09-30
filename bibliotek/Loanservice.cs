using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace bibliotekapp.Services
{
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

