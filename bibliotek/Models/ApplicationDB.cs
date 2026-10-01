using Microsoft.EntityFrameworkCore;

namespace bibliotek.Models
{
    public class ApplicationDbContext : DbContext
    {
        // 1. Ny tom konstruktor (löser CS7036)
        public ApplicationDbContext() { }

        // Den befintliga konstruktorn (behålls)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // 2. Koppling till databasen när ingen DbContextOptions skickas med
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "Server=127.0.0.1;Database=bibliotek;Uid=root;Pwd=hemligt-losenord;";
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        }

        public DbSet<User> User { get; set; }
        public DbSet<Copy> Copy { get; set; }
        public DbSet<Loan> Loan { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Copy Primary Key since it uses Bar_code (string)
            modelBuilder.Entity<Copy>()
                .HasKey(c => c.Bar_code);

            // Configure Loan Primary Key
            modelBuilder.Entity<Loan>()
                .HasKey(l => l.LoanID);

            // Configure Invoice Primary Key
            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.InvoiceID);
        }
    }
}