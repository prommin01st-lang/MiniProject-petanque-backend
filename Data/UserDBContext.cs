using JWTdemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTdemo
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set;  }
    }
}