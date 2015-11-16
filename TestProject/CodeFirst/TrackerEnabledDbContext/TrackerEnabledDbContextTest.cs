using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext;
using Utile.Money;

namespace TestProject.CodeFirst.TrackerEnabledDbContext
{
    [TrackChanges]
    public class Transaction
    {
        public int TransactionId { get; set; }
        public Money Money { get; set; }
        public string Detail { get; set; }
    }

    public class TrackerEnabledDbContextEntities : TrackerContext
    {
        public TrackerEnabledDbContextEntities() : base("TrackerEnabledDbContextEntities")
        {

        }
        public DbSet<Transaction> Transactions { get; set; }
    }

    public class TrackerEnabledDbContextInitializer : DropCreateDatabaseAlways<TrackerEnabledDbContextEntities>
    {
        protected override void Seed(TrackerEnabledDbContextEntities ctx)
        {
            //add a transaction
            var trx = new Transaction
            {
                Money = new Money(123456789012.3456m, CurrencyCodes.USD),
                Detail = "First Transaction"
            };
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            base.Seed(ctx);
        }
    }

    [TestClass]
    public class TrackerEnabledDbContextTest
    {
        [TestInitialize]
        public void Init()
        {
            Database.SetInitializer(new TrackerEnabledDbContextInitializer());
        }

        [TestMethod]
        public void TrackerEnabledDbContext_add()
        {
            // Arrange
            var ctx = new TrackerEnabledDbContextEntities();
            var trx = new Transaction
            {
                Money = 1.23d,
                Detail = "Another Transaction"
            };
            ctx.Transactions.Add(trx);
            // Act
            ctx.SaveChanges("TrackerEnabledDbContext_add");
            // Assert
            var count = ctx.Transactions.Count();
            var savedTrx = ctx.Transactions.OrderByDescending(t => t.TransactionId).First();
            Assert.AreEqual(2, count);
            Assert.AreEqual(savedTrx.Money, trx.Money);
            Assert.AreEqual(savedTrx.Detail, trx.Detail);
        }

        [TestMethod]
        public void TrackerEnabledDbContext_delete()
        {
            // Arrange
            var ctx = new TrackerEnabledDbContextEntities();
            var trx = new Transaction
            {
                Money = 1.23d,
                Detail = "Another Transaction"
            };
            var countBefore = ctx.Transactions.Count();
            ctx.Transactions.Add(trx);
            ctx.SaveChanges("TrackerEnabledDbContext_delete");
            // Act
            ctx.Transactions.Remove(trx);
            ctx.SaveChanges("TrackerEnabledDbContext_delete");
            // Assert
            var count = ctx.Transactions.Count();
            Assert.AreEqual(countBefore, count);
        }

        [TestMethod]
        public void EF6CodeFirst_get_first()
        {
            var ctx = new TrackerEnabledDbContextEntities();
            var trx = ctx.Transactions.First();

            Assert.AreEqual(trx.Detail, "First Transaction");
            Assert.AreEqual(trx.Money, new Money(123456789012.3456m, CurrencyCodes.USD));
            Assert.AreEqual(trx.Money.InternalAmount, 123456789012.3456);
            Assert.AreEqual(trx.Money.Amount, 123456789012.35m);
            Assert.AreEqual(trx.Money.CurrencyCode, "USD");
        }
    }
}
