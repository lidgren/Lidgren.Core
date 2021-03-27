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
			const int iterations = 5000000;
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
						Assert.Fail("Value " + val + " isn't 5, 6 or 7");
						break;
				}

				// ranged
				var r1 = (int)PRNG.NextUInt32();
				var r2 = (int)PRNG.NextUInt32();
				if (r1 < r2)
				{
					var rr = PRNG.Next(r1, r2);
					Assert.IsTrue(rr >= r1 && rr < r2, rr.ToString() + " is not between " + r1 + " and " + r2);
				}
				else if (r1 > r2)
				{
					var rr = PRNG.Next(r2, r1);
					Assert.IsTrue(rr >= r2 && rr < r1, rr.ToString() + " is not between " + r2 + " and " + r1);
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

			// make sure nextdouble() and nextfloat() don't generate numbers outside range
			var state = RandomSeed.GetUInt64();
			for (int r = 0; r < 10000000; r++)
			{
				double d = PRNG.NextDouble(ref state);
				Assert.IsTrue(d >= 0.0 && d < 1.0);

				float f = PRNG.NextFloat(ref state);
				Assert.IsTrue(f >= 0.0f && f < 1.0f);
			}
		}
	}
}
