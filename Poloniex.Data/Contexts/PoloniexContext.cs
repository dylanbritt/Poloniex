using Poloniex.Core.Domain;
using System.Data.Entity;

namespace Poloniex.Data.Contexts
{
    public class PoloniexContext : DbContext
    {
        public PoloniexContext() : base("name=PoloniexContext")
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<CryptoCurrencyDataPoint> CryptoCurrencyDataPoints { get; set; }
    }
}
