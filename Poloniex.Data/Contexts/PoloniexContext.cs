using Poloniex.Core.Domain.Models;
using System.Data.Entity;

namespace Poloniex.Data.Contexts
{
    public class PoloniexContext : DbContext
    {
        public PoloniexContext() : base()
        //public PoloniexContext() : base("name=PoloniexContext")
        {

        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CryptoCurrencyDataPoint>().Property(x => x.ClosingValue).HasPrecision(22, 12);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CryptoCurrencyDataPoint> CryptoCurrencyDataPoints { get; set; }
        public DbSet<TaskLoop> TaskLoops { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<GatherTask> GatherTasks { get; set; }
    }
}
