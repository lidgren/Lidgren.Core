using System;
using System.Text;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public partial class StringUtilsTests
	{
		[TestMethod]
		public void TestStringUtils()
		{
			Assert.AreEqual(11, StringUtils.HexCharToInteger('B'));

			var cbuf = new char[128];

			var testNumbers = new ulong[]
			{
				0, 1, 2, 100, 1000, 10000, 100000, 10000000,
				(ulong)PRNG.Next(0, 10000),
				(ulong)PRNG.Next(0, 10000),
				(ulong)PRNG.Next(0, 10000),
				(ulong)PRNG.Next(0, 10000),
				(ulong)PRNG.Next(0, 10000),
				(ulong)PRNG.Next(0, 10000),
				(ulong)PRNG.Next(0, 10000)
			};

			foreach(var val in testNumbers)
			{
				var sans = val.ToString("X");
				var str = (PRNG.NextBool() ? "0x" : "") + sans;

				Assert.AreEqual(val, StringUtils.FromHex(str));

				var utfBytes = Encoding.UTF8.GetBytes(str);
				Assert.AreEqual(val, StringUtils.FromUTF8Hex(utfBytes));

				var cnt = StringUtils.ToHex(val, cbuf);
				Assert.AreEqual(sans.Length, cnt);
				Assert.IsTrue(sans.AsSpan().SequenceEqual(cbuf.AsSpan(0, cnt)));
			}

			var arr = new byte[64];
			PRNG.NextBytes(arr);

			var facit = BitConverter.ToString(arr).Replace("-", "");
			var my = StringUtils.ToHex(arr);
			Assert.AreEqual(facit, my);

			var dvalstr = "0.00000000000123";
			var dval = double.Parse(dvalstr, System.Globalization.CultureInfo.InvariantCulture);
			var lots = StringUtils.DoubleToString(dval);

			// this is "1.23E-12"
			var defaultToString = dval.ToString(System.Globalization.CultureInfo.InvariantCulture);

			Assert.AreEqual(dvalstr, StringUtils.DoubleToString(dval));
		}
	}
}
