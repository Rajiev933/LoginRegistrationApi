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
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // <-- Save to database

            return "Registration successful";
        }
        public async Task<UserModel?> GetUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return null;
            }

            // Verify password hash
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Success)
            {
                return user; // ✅ return the full user object
            }

            return null;
        }
        public async Task UpdateUserAsync(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

    }
}
