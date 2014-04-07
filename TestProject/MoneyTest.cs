using Microsoft.VisualStudio.TestTools.UnitTesting;
using Useful.Money;

namespace TestProject
{
	[TestClass]
	public class MoneyTest
	{

		[TestMethod]
		public void Comparision()
		{
			// Money objects should be equal if their significant digits are the same
			Money money1 = new Money(5.130000000000001m);
			Money money2 = new Money(5.13m);
			Money money3 = new Money(5.12m);
			Assert.IsTrue(money1 == money2);
			Assert.IsTrue(money1.InternalAmount != money2.InternalAmount);
			Assert.IsTrue(money1 != money3);
			// Different Currencies aren't equal
			Money money4 = new Money(5.12m, CurrencyCodes.USD);
			Assert.IsTrue(money3 != money4);
		}

		[TestMethod]
		public void TestCreationOfBasicMoney()
		{
			//Locale specific formatting
			Money money1 = new Money(2000.1234567m, CurrencyCodes.USD);
			Assert.AreEqual("$2,000.12", money1.ToString());

			//Default currency
			Money money2 = new Money(3000m);
			Assert.AreEqual("ZAR", money2.CurrencyCode);
			Assert.AreEqual("R", money2.CurrencySymbol);
			Assert.AreEqual("South African Rand", money2.CurrencyName);
			Assert.AreEqual(2, money2.DecimalDigits);

			//Implicit casting of int, decimal and double to Money
			Money money3 = new Money(5.0d);
			Money money4 = new Money(5.0m);
			Money money5 = new Money(5);
			Money money6 = 5.0d;
			Money money7 = 5.0m;
			Money money8 = 5;
			Money money9 = 5.0f;
			Money money10 = 5.0;
			Assert.IsTrue(money3 == money4 && money4 == money5 && money5 == money6 && money6 == money7 && money7 == money8 && money8 == money9 && money9 == money10);

			//Generic 3char currency code formatting instead of locale based with symbols
			Assert.AreEqual("USD 2 000,12", money1.ToString(true));
			Assert.AreEqual("ZAR 3 000,00", money2.ToString(true));

		}

		[TestMethod]
		public void TestSignificantDecimalDigits()
		{
			Money money1 = new Money(13000123.3349m, CurrencyCodes.USD);
			Assert.AreEqual("$13,000,123.33", money1.ToString());
			// Can also use CurrencyCode string (catch code not found exception)
			Money money2 = new Money(13000123.335m, "USD");
			Assert.AreEqual("$13,000,123.34", money2.ToString());

			// Test Amount rounding
			Money money3 = 1.001m;
			Assert.AreEqual(0.40m, (money3 * 0.404).Amount);
			Assert.AreEqual(0.41m, (money3 * 0.40501).Amount);
			Assert.AreEqual(0.41m, (money3 * 0.404999999999999).Amount);
			money3 = 1.0;
			Assert.AreEqual(0.40m, (money3 * 0.404999999999999).Amount);

			//Different significant digits
			Money money4 = new Money(2.499m, CurrencyCodes.USD);
			Assert.AreEqual("$2.50", money4.ToString());

			Money money5 = new Money(2.499m, CurrencyCodes.JPY);
			Assert.AreEqual("¥2", money5.ToString());

			//Very large numbers
			//Double is used internally, only 16 digits of accuracy can be guaranteed
			Money money6 = 123456789012.34; //14 digits
			money6 *= 1.14; //will add another 2 digits of detail
			money6 /= 1.14;
			Assert.AreEqual(money6.Amount, 123456789012.34m);
		}

		[TestMethod]
		public void TestOperators()
		{
			Money money1 = new Money(20, CurrencyCodes.EUR);
			Assert.AreEqual("6,67 €", (money1 / 3).ToString());
			Assert.AreEqual("6,67 €", (money1 / 3m).ToString());
			Assert.AreEqual("6,67 €", (money1 / 3.0).ToString());
			Assert.AreEqual("0,00 €", (money1 * (1 / 3)).ToString());
			Assert.AreEqual("6,67 €", (money1 * (1m / 3m)).ToString());
			Assert.AreEqual("6,67 €", (money1 * (1d / 3d)).ToString());
			Assert.AreEqual("3,33 €", (money1 / 6).ToString());
			Assert.AreEqual("3,33 €", (money1 * (1.0 / 6.0)).ToString());

			// Operators use internal value
			Money money2 = new Money(0.01m);
			Assert.AreEqual("R 0,01", (money2 / 2).ToString());

			Money money3 = new Money(3, CurrencyCodes.EUR);
			Money money4 = new Money(1d / 3d, CurrencyCodes.EUR);
			Money money5 = new Money(6, CurrencyCodes.EUR);
			Money money6 = new Money(1d / 6d, CurrencyCodes.EUR);
			Assert.AreEqual("6,67 €", (money1 / money3).ToString());
			Assert.AreEqual("6,67 €", (money1 * money4).ToString());
			Assert.AreEqual("3,33 €", (money1 / money5).ToString());
			Assert.AreEqual("3,33 €", (money1 * money6).ToString());
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
			Money money1 = new Money(10);
			Money[] allocatedMoney1 = money1.Allocate(3);
			Money total1 = new Money();
			for (int i = 0; i < allocatedMoney1.Length; i++)
				total1 += allocatedMoney1[i];
			Assert.AreEqual("R 10,00", total1.ToString());
			Assert.AreEqual("R 3,34", allocatedMoney1[0].ToString());
			Assert.AreEqual("R 3,33", allocatedMoney1[1].ToString());
			Assert.AreEqual("R 3,33", allocatedMoney1[2].ToString());

			Money money2 = new Money(0.09m, CurrencyCodes.USD);
			Money[] allocatedMoney2 = money2.Allocate(5);
			Money total2 = new Money(CurrencyCodes.USD);
			for (int i = 0; i < allocatedMoney2.Length; i++)
				total2 += allocatedMoney2[i];
			Assert.AreEqual("$0.09", total2.ToString());
		}

	}
}
