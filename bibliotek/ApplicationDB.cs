using System;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace bibliotek.Models
{
    public class ApplicationDbContext : DbContext
    {
        // De här DbSet / klasserna motsvarar tabeller i MySQL-databasen
        public DbSet<Copy> Copy { get; set; }
        public DbSet<Loan> Loan { get; set; }
        public DbSet<User> User { get; set; }

        // Här är kopplingen till databasen
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Server=localhost;Port=3306;Database=bibliotek;UserID=root;Password=hemligt-losenord;";

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        // Här så säger vi till Entity Framework vilka primära nycklar som tabellerna har
        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            modelbuilder.Entity<Copy>().HasKey(c => c.Bar_code);
            modelbuilder.Entity<Loan>().HasKey(l => l.LoanID);
            modelbuilder.Entity<User>().HasKey(u => u.User_ID);
        }
    }
}
