using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utile.Money;

namespace TestProject
{
	[TestClass]
	public class ConversionTest
	{
		[TestMethod]
		public void TestConversion()
		{
			var money1 = new Money(12.34, CurrencyCodes.USD);
			var money2 = new Money(12.34, CurrencyCodes.ZAR);

			Money money3 = money1.Convert(CurrencyCodes.ZAR, 7.8);

			Assert.AreEqual(money3.CurrencyCode, money2.CurrencyCode);
			Assert.AreNotEqual(money3, money2);
			Assert.IsTrue(money3 > money2);
			// No way to check if Rands are equal to dollars
			Assert.IsTrue(money1 != money3);
		}

		[TestMethod]
		public void TestConverter()
		{
			var money1 = new Money(12.34, CurrencyCodes.USD);
			var money2 = new Money(12.34, CurrencyCodes.ZAR);
			Money.Converter = new SampleConverter();

			Money money3 = money1.Convert(CurrencyCodes.ZAR);

			Assert.AreEqual(money3.CurrencyCode, money2.CurrencyCode);
			Assert.AreNotEqual(money3, money2);
			Assert.IsTrue(money3 > money2);
			Assert.IsTrue(money1 != money3);
			Assert.AreNotEqual(money3, money1);

			// comparing apples to oranges possible with Converter!
			// Will only return a match if the Converter has the same rate for from -> to and (inverted) to -> from
			Money.AllowImplicitConversion = true;
			double m1to3 = Money.Converter.GetRate(money1.CurrencyCode, money3.CurrencyCode, DateTime.Now);
			double m3to1 = Money.Converter.GetRate(money3.CurrencyCode, money1.CurrencyCode, DateTime.Now);
			if (m1to3 == 1d / m3to1)
			{
				Assert.IsTrue(money3 == money1);
				Assert.IsTrue(money1 == money3);
				Assert.AreEqual(money3, money1);
			}
			else
			{
				Assert.IsFalse(money3 == money1);
				Assert.IsFalse(money1 == money3);
				Assert.AreNotEqual(money3, money1);
			}
		}

		[TestMethod]
		public void TestOperations()
		{
			var money1 = new Money(12.34, CurrencyCodes.USD);
			var money2 = new Money(12.34, CurrencyCodes.ZAR);
			Money.Converter = new SampleConverter();
			Money.AllowImplicitConversion = true;

			// adding oranges to apples gives you apples
			var money3 = money1 + money2;
			Assert.AreEqual("USD", money3.CurrencyCode);

			// left side is ZAR and right side is USD, money3 gets converted back to ZAR
			// the same converter should return the same inverted rates
			double m1to3 = Money.Converter.GetRate(money1.CurrencyCode, money3.CurrencyCode, DateTime.Now);
			double m3to1 = Money.Converter.GetRate(money3.CurrencyCode, money1.CurrencyCode, DateTime.Now);
			if (m1to3 == 1d / m3to1)
				Assert.AreEqual(money2, money3 - money1);
			else
				Assert.AreNotEqual(money2, money3 - money1);
			// Mix up ZAR and USD. moneys converted only one way
			Assert.AreEqual(money1, money3 - money2);

			// Should fail if allowImplicitconversion is false (default)
			Money.AllowImplicitConversion = false;
			try
			{
				money3 = money1 + money2;
				Assert.Fail("Money type exception was not thrown");
			}
			catch (InvalidOperationException e)
			{
				Assert.AreEqual("Money type mismatch", e.Message);
			}
		}
	}
}
