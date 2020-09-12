using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class PRNGTests
	{
		[TestMethod]
		public void TestPRNG()
		{
			const int iterations = 10000000;
			Console.WriteLine();

			int trues = 0;
			int larger = 0;
			int smaller = 0;
			for (int i = 0; i < iterations; i++)
			{
				if (PRNG.NextBool())
					trues++;
				int val = PRNG.Next(5, 8); // 5, 6 or 7
				switch (val)
				{
					case 5:
						smaller++;
						break;
					case 6:
						break;
					case 7:
						larger++;
						break;
					default:
						Assert.Fail();
						break;
				}
			}

			var p = (double)trues / (double)iterations;
			Assert.IsTrue(p > 0.495 && p < 0.505);

			const double third = 1.0 / 3.0;
			const double offset = third * 0.05;
			const double low = third - offset;
			const double high = third + offset;

			p = (double)smaller / (double)iterations;
			Assert.IsTrue(p > low && p < high);

			p = (double)larger / (double)iterations;
			Assert.IsTrue(p > low && p < high);
		}
	}
}
