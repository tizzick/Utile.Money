using System.Data.Entity;
using System.Linq;
using EntityFramework.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utile.Money;

namespace TestProject.CodeFirst.EFExtendedEntities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
       // public Money Money { get; set; }
        public string Detail { get; set; }
    }

    public class EFExtendedEntities : DbContext
    {
        public EFExtendedEntities() : base("EFExtendedEntities")
        {

        }
        public DbSet<Transaction> Transactions { get; set; }
    }

    public class TestsInitializer : DropCreateDatabaseAlways<EFExtendedEntities>
    {
        protected override void Seed(EFExtendedEntities ctx)
        {
            //add a transaction
            var trx = new Transaction
            {
              ///  Money = new Money(123456789012.3456m, CurrencyCodes.USD),
                Detail = "First Transaction"
            };
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            base.Seed(ctx);
        }
    }

    [TestClass]
    public class EFExtendedCodeFirstTest
    {
        [TestInitialize]
        public void Init()
        {
            Database.SetInitializer(new TestsInitializer());
        }

        [TestMethod]
        public void EFExtendedCodeFirst_add()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
           //     Money = 1.23d,
                Detail = "Another Transaction"
            };
            ctx.Transactions.Add(trx);
            // Act
            ctx.SaveChanges();
            // Assert
            var count = ctx.Transactions.Count();
            var savedTrx = ctx.Transactions.OrderByDescending(t => t.TransactionId).First();
            Assert.AreEqual(2, count);
     //       Assert.AreEqual(savedTrx.Money, trx.Money);
            Assert.AreEqual(savedTrx.Detail, trx.Detail);
        }

        [TestMethod]
        public void EFExtendedCodeFirst_delete()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
        //        Money = 1.23d,
                Detail = "Another Transaction"
            };
            var countBefore = ctx.Transactions.Count();
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            // Act
            var audit = ctx.BeginAudit();
            ctx.Transactions.Remove(trx);
            ctx.SaveChanges();
            var log = audit.LastLog;
            // Assert
            var count = ctx.Transactions.Count();
            Assert.AreEqual(countBefore, count);
     //       Assert.AreEqual(1, log.Entities.Count, "audit log recorded 1 change");
            
        }

        [TestMethod]
        public void EFExtendedCodeFirst_get_first()
        {
            var ctx = new EFExtendedEntities();
            var trx = ctx.Transactions.First();

            Assert.AreEqual(trx.Detail, "First Transaction");
     //       Assert.AreEqual(trx.Money, new Money(123456789012.3456m, CurrencyCodes.USD));
     //       Assert.AreEqual(trx.Money.InternalAmount, 123456789012.3456);
     //       Assert.AreEqual(trx.Money.Amount, 123456789012.35m);
     //       Assert.AreEqual(trx.Money.CurrencyCode, "USD");
        }

        [TestMethod]
        public void EFExtendedCodeFirst_toXml()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
     //           Money = 1.23d,
                Detail = "Another Transaction"
            };
            var priorCount = ctx.Transactions.Count();
            var audit = ctx.BeginAudit();
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            var log = audit.LastLog;
            //Act
            var xml = log.ToXml();

            // Assert
            var count = ctx.Transactions.Count();
            Assert.AreEqual(priorCount + 1, count, "assert object cound increased by one");
            Assert.IsFalse(string.IsNullOrEmpty(xml),"xml is not empty or null");
        }

    }
}
