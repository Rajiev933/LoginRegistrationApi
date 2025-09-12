using LoginRegistrationApi.Models;
using LoginRegistrationApi.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoginRegistrationApi.Repository
{
    public class RegistrationRepo
    {
        private readonly AppDbContext _context;

        private readonly IPasswordHasher<UserModel> _passwordHasher;

        public RegistrationRepo(AppDbContext context, IPasswordHasher<UserModel> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> RegisterUserAsync(UserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // <-- Save to database

            return "Registration successful";
        }
        public async Task<bool> LoginUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
            if (user == null)
            {
                return false;
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
