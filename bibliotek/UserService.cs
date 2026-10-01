using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using bibliotek.Models;

namespace bibliotek.Services
{
    public interface IUserService
    {
        Task<(bool Success, string Message, User User)> LoginAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterUserAsync(User newUser, string rawPassword);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, User User)> LoginAsync(string email, string password)
        {
            // 1. Sök efter användaren på e-post
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return (false, "Felaktig e-post eller lösenord.", null);
            }

            // 2. Verifiera lösenordet mot BCrypt-hashen i databasen
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            if (!isPasswordValid)
            {
                return (false, "Felaktig e-post eller lösenord.", null);
            }

            // 3. Inloggning lyckades!
            return (true, $"Välkommen {user.FirstName}!", user);
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(User newUser, string rawPassword)
        {
            var existingUser = await _context.User
                .AnyAsync(u => u.Email.ToLower() == newUser.Email.ToLower());

            if (existingUser)
            {
                return (false, "En användare med denna e-post finns redan.");
            }

            // Hasha lösenordet innan det sparas i databasen
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return (true, "Användaren har registrerats!");
        }
    }
}