using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Data
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<TopicModel> Topics { get; set; }
        public DbSet<QuestionModel> Questions { get; set; }
        public DbSet<RoomModel> Rooms { get; set; }
        public DbSet<PlayerModel> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuestionModel>()
                .HasOne(q => q.Room)
                .WithMany(r => r.Questions)
                .HasForeignKey(q => q.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
        }

    }
}
