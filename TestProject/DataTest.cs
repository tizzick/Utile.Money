using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using Fruitful.Money;

namespace TestProject
{
	// Using a Money property in a class will result in two columns in the database with EF4.1 or later:
	//   a 64 bit float (double) and a 16bit int (smallint)

	public class Transaction
	{
		public int TransactionId { get; set; }
		public Money Money { get; set; }
		public string Detail { get; set; }
	}

	public class TestContext : DbContext
	{
	    public TestContext() : base("TestContext")
	    {
	        
	    }
		public DbSet<Transaction> Transactions { get; set; }
	}

	public class TestsInitializer : DropCreateDatabaseAlways<TestContext>
	{
		protected override void Seed(TestContext ctx)
		{
			//add a transaction
			Transaction trx = new Transaction
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
	public class DataTest
	{
		[TestInitialize]
		public void Init()
		{
			Database.SetInitializer<TestContext>(new TestsInitializer());
		}

		[TestMethod]
		public void TestAddDelete()
		{
			// Arrange
			TestContext ctx = new TestContext();
			Transaction trx = new Transaction
			{
				Money = 1.23d,
				Detail = "Another Transaction"
			};
			ctx.Transactions.Add(trx);
			// Act
			ctx.SaveChanges();
			// Assert
			int count = ctx.Transactions.Count();
			var savedTrx = ctx.Transactions.OrderByDescending(t => t.TransactionId).First();
			Assert.AreEqual(2, count);
			Assert.AreEqual(savedTrx.Money, trx.Money);
			Assert.AreEqual(savedTrx.Detail, trx.Detail);
			// Act 2 (delete)
			ctx.Transactions.Remove(savedTrx);
			ctx.SaveChanges();
			// Assert 2
			count = ctx.Transactions.Count();
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		public void TestView()
		{
			TestContext ctx = new TestContext();
			var trx = ctx.Transactions.First();

			Assert.AreEqual(trx.Detail, "First Transaction");
			Assert.AreEqual(trx.Money, new Money(123456789012.3456m, CurrencyCodes.USD));
			Assert.AreEqual(trx.Money.InternalAmount, 123456789012.3456);
			Assert.AreEqual(trx.Money.Amount, 123456789012.35m);
			Assert.AreEqual(trx.Money.CurrencyCode, "USD");
		}
	}
}
