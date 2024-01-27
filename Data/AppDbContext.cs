using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using StackOverflow.Models;

namespace StackOverflow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<PostViewModel> Posts { get; set; }

    }
}