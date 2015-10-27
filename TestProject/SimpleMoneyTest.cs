using Microsoft.VisualStudio.TestTools.UnitTesting;
using Useful.Money;
using System;
using System.Globalization;

namespace TestProject
{
	[TestClass]
	public class SimpleMoneyTest
	{

		[TestMethod]
        public void Comparision()
        {
            //get the current culture number of significant digits
            //todo after upgrading to .net 4.5 or higher use CultureInfo.DefaultThreadCurrentCulture to set the current thread culture instead of using significantDigits
            var significantDigits = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits;
            // SimpleMoney objects should be equal if their significant digits are the same
            SimpleMoney money1 = new SimpleMoney(5.130000000000001m);
            SimpleMoney money2 = new SimpleMoney(5.13m);
            SimpleMoney money3 = new SimpleMoney(5.12m);
            Assert.IsTrue(significantDigits < 15 ? money1 == money2 : money1 != money2);
            Assert.IsTrue(money1.InternalAmount != money2.InternalAmount);
            Assert.IsTrue(significantDigits > 1 ? money1 != money3 : money1 == money3);

        }

        [TestMethod]
		public void TestCreationOfBasicSimpleMoney()
        {
            var currentSymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            var currentRegion = new RegionInfo(CultureInfo.CurrentCulture.LCID);
            var currentCode = currentRegion.ISOCurrencySymbol;
            var currentEnglishName = currentRegion.CurrencyEnglishName;
            var currentDigits = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits;


            //Default currency
            SimpleMoney money2 = new SimpleMoney(3000m);
			Assert.AreEqual(currentCode, money2.CurrencyCode);
			Assert.AreEqual(currentSymbol, money2.CurrencySymbol);
			Assert.AreEqual(currentEnglishName, money2.CurrencyName);
			Assert.AreEqual(currentDigits, money2.DecimalDigits);

			//Implicit casting of int, decimal and double to SimpleMoney
			SimpleMoney money3 = new SimpleMoney(5.0d);
			SimpleMoney money4 = new SimpleMoney(5.0m);
			SimpleMoney money5 = new SimpleMoney(5);
			SimpleMoney money6 = 5.0d;
			SimpleMoney money7 = 5.0m;
			SimpleMoney money8 = 5;
			SimpleMoney money9 = 5.0f;
			SimpleMoney money10 = 5.0;
			Assert.IsTrue(money3 == money4 && money4 == money5 && money5 == money6 && money6 == money7 && money7 == money8 && money8 == money9 && money9 == money10);
		}

		[TestMethod]
		public void TestSignificantDecimalDigits()
		{
			SimpleMoney money1 = new SimpleMoney(13000123.3349m);
			Assert.AreEqual("R 13 000 123,33", money1.ToString());
			// Can also use CurrencyCode string (catch code not found exception)
			SimpleMoney money2 = new SimpleMoney(13000123.335m);
			Assert.AreEqual("R 13 000 123,34", money2.ToString());

			// Test Amount rounding
			SimpleMoney money3 = 1.001m;
			Assert.AreEqual(0.40m, (money3 * 0.404).Amount);
			Assert.AreEqual(0.41m, (money3 * 0.40501).Amount);
			Assert.AreEqual(0.41m, (money3 * 0.404999999999999).Amount);
			money3 = 1.0;
			Assert.AreEqual(0.40m, (money3 * 0.404999999999999).Amount);

			//Very large numbers
			//Double is used internally, only 16 digits of accuracy can be guaranteed
			SimpleMoney money6 = 123456789012.34; //14 digits
			money6 *= 1.14; //will add another 2 digits of detail
			money6 /= 1.14;
			Assert.AreEqual(money6.Amount, 123456789012.34m);
		}

		[TestMethod]
		public void TestOperators()
		{
			SimpleMoney money1 = new SimpleMoney(20);
			Assert.AreEqual("R 6,67", (money1 / 3).ToString());
			Assert.AreEqual("R 6,67", (money1 / 3m).ToString());
			Assert.AreEqual("R 6,67", (money1 / 3.0).ToString());
			Assert.AreEqual("R 0,00", (money1 * (1 / 3)).ToString());
			Assert.AreEqual("R 6,67", (money1 * (1m / 3m)).ToString());
			Assert.AreEqual("R 6,67", (money1 * (1d / 3d)).ToString());
			Assert.AreEqual("R 3,33", (money1 / 6).ToString());
			Assert.AreEqual("R 3,33", (money1 * (1.0 / 6.0)).ToString());

			// Operators use internal value
			SimpleMoney money2 = new SimpleMoney(0.01m);
			Assert.AreEqual("R 0,01", (money2 / 2).ToString());

			SimpleMoney money3 = new SimpleMoney(3);
			SimpleMoney money4 = new SimpleMoney(1d / 3d);
			SimpleMoney money5 = new SimpleMoney(6);
			SimpleMoney money6 = new SimpleMoney(1d / 6d);
			Assert.AreEqual("R 6,67", (money1 / money3).ToString());
			Assert.AreEqual("R 6,67", (money1 * money4).ToString());
			Assert.AreEqual("R 3,33", (money1 / money5).ToString());
			Assert.AreEqual("R 3,33", (money1 * money6).ToString());
			Assert.IsTrue((money3 + money5).Amount == 9);
			Assert.IsTrue((money5 - money3).Amount == 3);
			//Using implicit casting
			Assert.IsTrue(money3 == 3);
			Assert.IsTrue(money5 - money3 == 3);
			Assert.IsTrue(money3 + 3d == money5);
			Assert.IsTrue(money5 - 3 == money3);
			Assert.IsTrue(money5 + 2 == 8);
			Assert.IsTrue(money5 - 2 == 4d);
			Assert.IsTrue(2m + money3.Amount == 5);
		}

		[TestMethod]
		public void TestAllocation()
		{
			SimpleMoney money1 = new SimpleMoney(10);
			SimpleMoney[] allocatedMoney1 = money1.Allocate(3);
			SimpleMoney total1 = new SimpleMoney();
			for (int i = 0; i < allocatedMoney1.Length; i++)
				total1 += allocatedMoney1[i];
			Assert.AreEqual("R 10,00", total1.ToString());
			Assert.AreEqual("R 3,34", allocatedMoney1[0].ToString());
			Assert.AreEqual("R 3,33", allocatedMoney1[1].ToString());
			Assert.AreEqual("R 3,33", allocatedMoney1[2].ToString());

			SimpleMoney money2 = new SimpleMoney(0.09m);
			SimpleMoney[] allocatedMoney2 = money2.Allocate(5);
			SimpleMoney total2 = new SimpleMoney();
			for (int i = 0; i < allocatedMoney2.Length; i++)
				total2 += allocatedMoney2[i];
			Assert.AreEqual("R 0,09", total2.ToString());
		}

	}
}
