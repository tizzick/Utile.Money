using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading;
using EntityFramework.Audit;
using EntityFramework.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utile.Money;

namespace TestProject.CodeFirst.EFExtendedEntities
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Money Money { get; set; }
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
                Money = new Money(123456789012.3456m, CurrencyCodes.USD),
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
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;
            Database.SetInitializer(new TestsInitializer());
        }

        [TestMethod]
        public void EFExtendedCodeFirst_add()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
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
            var savedTrx = ctx.Transactions.OrderByDescending(t => t.Id).First();
            Assert.AreEqual(2, count);
            Assert.AreEqual(savedTrx.Money, trx.Money);
            Assert.AreEqual(savedTrx.Detail, trx.Detail);
        }

        [TestMethod]
        public void EFExtendedCodeFirst_delete()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
                Money = 1.23d,
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
            Assert.AreEqual(1, log.Entities.Count, "audit log recorded 1 change");
            
        }

        [TestMethod]
        public void EFExtendedCodeFirst_get_first()
        {
            var ctx = new EFExtendedEntities();
            var trx = ctx.Transactions.First();

            Assert.AreEqual(trx.Detail, "First Transaction");
            Assert.AreEqual(trx.Money, new Money(123456789012.3456m, CurrencyCodes.USD));
            Assert.AreEqual(trx.Money.InternalAmount, 123456789012.3456);
            Assert.AreEqual(trx.Money.Amount, 123456789012.35m);
            Assert.AreEqual(trx.Money.CurrencyCode, "USD");
        }

        [TestMethod]
        public void EFExtendedCodeFirst_toXml()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
                Money = 1.23d,
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

        [TestMethod]
        public void EFExtendedCodeFirst_Edit()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
                Money = 1.23d,
                Detail = "Another Transaction To be edited"
            };
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();

            //Act
            trx.Money = 10d;
            var t = ctx.Set<Transaction>().FirstOrDefault(x => x.Id == trx.Id);
            ctx.Entry(t).CurrentValues.SetValues(trx);
            ctx.Entry(t).State = EntityState.Modified;
            ctx.SaveChanges();
            
            // Assert
            Assert.IsTrue(t?.Money.InternalAmount == 10d, "assert the object was updated correctly");
        }

        [TestMethod]
        public void EFExtendedCodeFirst_Cast_current_value()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction
            {
                Money = 1.23d,
                Detail = "Another Transaction To be edited"
            };
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            var audit = ctx.BeginAudit();
            trx.Money = 10d;
            var t = ctx.Set<Transaction>().FirstOrDefault(x => x.Id == trx.Id);
            ctx.Entry(t).CurrentValues.SetValues(trx);
            ctx.Entry(t).State = EntityState.Modified;
            ctx.SaveChanges();
            var log = audit.LastLog;
            var entity = log.Entities[0];
            //Act
            var current = (Transaction)entity.Current;
            // Assert
            Assert.AreEqual(current.Money.InternalAmount, trx.Money.InternalAmount, "money internal amounts are equal");
            Assert.AreEqual(current.Money.ISOCode, trx.Money.ISOCode, "money isocode are equal");
             
        }
        
        [TestMethod]
        public void EFExtendedCodeFirst_Cast_Money_Property_current_value()
        {
            // Arrange
            var ctx = new EFExtendedEntities();
            var trx = new Transaction // model
            {
                Money = new Money(1.23d),//ComplexType
                Detail = "Another Transaction To be edited"
            };
            ctx.Transactions.Add(trx);
            ctx.SaveChanges();
            var audit = ctx.BeginAudit();
            trx.Money = 10d;
            var t = ctx.Set<Transaction>().FirstOrDefault(x => x.Id == trx.Id);
            ctx.Entry(t).CurrentValues.SetValues(trx);
            ctx.Entry(t).State = EntityState.Modified;
            ctx.SaveChanges();
            var log = audit.LastLog;
            var entity = log.Entities[0];


            //Act
            var current = (Money)(DbDataRecord)entity.Properties[2].Current;
            // Assert
            Assert.AreEqual(current.InternalAmount, trx.Money.InternalAmount, "money internal amounts are equal");
            Assert.AreEqual(current.ISOCode, trx.Money.ISOCode, "money isocode are equal");

        }

    }
}
