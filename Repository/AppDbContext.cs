using LoginRegistrationApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LoginRegistrationApi.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<UserModel> Users { get; set; } = null!;
    }
}
