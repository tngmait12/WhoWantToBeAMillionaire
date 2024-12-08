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

    }
}
