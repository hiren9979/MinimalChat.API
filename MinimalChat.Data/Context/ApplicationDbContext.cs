using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Model;

namespace Minimal_chat_application.Context
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet for your custom User model
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<LogModel> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         

            // Your other configurations here

            base.OnModelCreating(modelBuilder);
        }
    }
}
