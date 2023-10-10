using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Model;
using MinimalChat.Domain.Interface;
using MinimalChat.Domain.Model;

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

        public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GroupChat>()
               .HasMany(gc => gc.Members)
               .WithMany(gm => gm.GroupChat)
               .UsingEntity<Dictionary<string, object>>(
                   "GroupMemberJoin",
                   j => j.HasOne<GroupMember>().WithMany(),
                   j => j.HasOne<GroupChat>().WithMany(),
                   j =>
                   {
                       j.HasKey("GroupId", "UserId");
                       j.ToTable("GroupMembers"); // Name of the junction table
                   });

            // Other configurations for your entities...
        }
    }


}
