using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<TopicModel> Topics { get; set; }
        public DbSet<QuestionModel> Questions { get; set; }
    }
}
