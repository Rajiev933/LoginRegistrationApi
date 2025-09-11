using LoginRegistrationApi.Models;
using LoginRegistrationApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace LoginRegistrationApi.Repository
{
    public class RegistrationRepo
    {
        private readonly AppDbContext _context;

        public RegistrationRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> RegisterUserAsync(UserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // <-- Save to database

            return "Registration successful";
        }
    }
}
