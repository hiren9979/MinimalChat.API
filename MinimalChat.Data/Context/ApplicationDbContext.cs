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
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => new { gm.GroupChatId, gm.UserId });

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.GroupChat)
                .WithMany(gc => gc.GroupMembers)
                .HasForeignKey(gm => gm.GroupChatId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMembers)
                .HasForeignKey(gm => gm.UserId);
        }
    }


}
