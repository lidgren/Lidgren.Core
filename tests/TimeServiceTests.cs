using System;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public partial class TimeServiceTests
	{
		[TestMethod]
		public void TestTimeService()
		{
			var wall = TimeService.Wall;

			bool ok;

			var str = TimeService.CompactDuration(1234567.5);
			Assert.AreEqual("342h56m7.5s", str);
			ok = TimeService.TryParseDuration(str, out var back);
			Assert.AreEqual(1234567.5, back);

			Assert.AreEqual("10m", TimeService.CompactDuration(600));
			Assert.AreEqual("10s", TimeService.CompactDuration(10));
			Assert.AreEqual("100ms", TimeService.CompactDuration(0.1));

			ok = TimeService.TryParseDuration("3h2m12s", out var seconds);
			var facit = (3 * 60 * 60);
			facit += (2 * 60);
			facit += 12;
			Assert.IsTrue(ok);
			Assert.AreEqual(facit, seconds);

			ok = TimeService.TryParseDuration("faulty string", out var _);
			Assert.IsFalse(ok);

			TestParseDuration("8", 8);
			TestParseDuration("8s", 8);
			TestParseDuration("-10m", -600);
			TestParseDuration("0m", 0);
			TestParseDuration("1m1s", 61);
			TestParseDuration("3h2m12s", (3 * 60 * 60) + (2 * 60) + 12);
			TestParseDuration("1.5m", 90);
		}

		private static void TestParseDuration(string str, double facit)
		{
			var ok = TimeService.TryParseDuration(str, out var result);
			Assert.IsTrue(ok);
			Assert.AreEqual(facit, result);
		}
	}
}
