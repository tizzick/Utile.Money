using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utile.Money;

namespace TestProject.CodeFirst.EF6Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public Money Money { get; set; }
        public string Detail { get; set; }
    }

    public class EF6Entities : DbContext
    {
        public EF6Entities() : base("EF6Entities")
        {

        }
        public DbSet<Transaction> Transactions { get; set; }
    }

    public class TestsInitializer : DropCreateDatabaseAlways<EF6Entities>
    {
        protected override void Seed(EF6Entities ctx)
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
    public class EF6CodeFirstTest
    {
        [TestInitialize]
        public void Init()
        {
            Database.SetInitializer(new TestsInitializer());
        }

        [TestMethod]
        public void EF6CodeFirst_add()
        {
            // Arrange
            var ctx = new EF6Entities();
            var trx = new Transaction
            {
                Money = 1.23d,
                Detail = "Another Transaction"
            };
            ctx.Transactions.Add(trx);
            // Act
            ctx.SaveChanges();
            // Assert
            var count = ctx.Transactions.Count();
            var savedTrx = ctx.Transactions.OrderByDescending(t => t.TransactionId).First();
            Assert.AreEqual(2, count);
            Assert.AreEqual(savedTrx.Money, trx.Money);
            Assert.AreEqual(savedTrx.Detail, trx.Detail);
        }

        [TestMethod]
        public void EF6CodeFirst_delete()
        {
            // Arrange
            var ctx = new EF6Entities();
            var trx = new Transaction
            {
                Money = 1.23d,
                Detail = "Another Transaction"
            };
            var countBefore = ctx.Transactions.Count();
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            // Act
            ctx.Transactions.Remove(trx);
            ctx.SaveChanges();
            // Assert
            var count = ctx.Transactions.Count();
            Assert.AreEqual(countBefore, count);
        }

        [TestMethod]
        public void EF6CodeFirst_get_first()
        {
            var ctx = new EF6Entities();
            var trx = ctx.Transactions.First();

            Assert.AreEqual(trx.Detail, "First Transaction");
            Assert.AreEqual(trx.Money, new Money(123456789012.3456m, CurrencyCodes.USD));
            Assert.AreEqual(trx.Money.InternalAmount, 123456789012.3456);
            Assert.AreEqual(trx.Money.Amount, 123456789012.35m);
            Assert.AreEqual(trx.Money.CurrencyCode, "USD");
        }
    }
}
