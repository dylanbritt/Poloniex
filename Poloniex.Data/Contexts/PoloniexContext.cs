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

            modelBuilder.Entity<MovingAverage>().Property(x => x.MovingAverageValue).HasPrecision(22, 12);
            modelBuilder.Entity<MovingAverage>().Property(x => x.LastClosingValue).HasPrecision(22, 12);

            modelBuilder.Entity<TradeSignalOrder>().Property(x => x.LastValueAtRequest).HasPrecision(22, 12);
            modelBuilder.Entity<TradeSignalOrder>().Property(x => x.LastValueAtProcessing).HasPrecision(22, 12);
            modelBuilder.Entity<TradeSignalOrder>().Property(x => x.PlaceValueTradedAt).HasPrecision(22, 12);
            modelBuilder.Entity<TradeSignalOrder>().Property(x => x.MoveValueTradedAt).HasPrecision(22, 12);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<CryptoCurrencyDataPoint> CryptoCurrencyDataPoints { get; set; }
        public DbSet<EventAction> EventActions { get; set; }
        public DbSet<GatherTask> GatherTasks { get; set; }
        public DbSet<MovingAverage> MovingAverages { get; set; }
        public DbSet<TaskLoop> TaskLoops { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TradeSignalEventAction> TradeSignalEventActions { get; set; }
        public DbSet<TradeSignalOrder> TradeSignalOrders { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
