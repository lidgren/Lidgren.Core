using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class ColorTests
	{
		[TestMethod]
		public void TestColor()
		{
			var col = new Color((byte)1, (byte)2, (byte)3, (byte)4);
			Assert.AreEqual(1, col.R);
			Assert.AreEqual(2, col.G);
			Assert.AreEqual(3, col.B);
			Assert.AreEqual(4, col.A);

			col.GetRGBA(out var red, out var green, out var blue, out var alpha);
			Assert.AreEqual(1, red);
			Assert.AreEqual(2, green);
			Assert.AreEqual(3, blue);
			Assert.AreEqual(4, alpha);

			var rt = new Color(col.RGBA);
			Assert.AreEqual(rt.RGBA, col.RGBA);
			Assert.AreEqual(rt, col);

			var hex = col.ToHex();
			Assert.AreEqual("01020304", hex);

			col = Color.FromHex("#FF881177");
			Assert.AreEqual(0xFF, col.R);
			Assert.AreEqual(0x88, col.G);
			Assert.AreEqual(0x11, col.B);
			Assert.AreEqual(0x77, col.A);

			Color.Random(0.5f);

			var c1 = Color.FromHex("#FFA50033");
			var c2 = new Color(255, 165, 0, 51);
			Assert.AreEqual(c1, c2);

			var redcol = Color.Red;
			Assert.AreEqual(255, redcol.R);
			Assert.AreEqual(0, redcol.G);
			Assert.AreEqual(0, redcol.B);
			Assert.AreEqual(255, redcol.A);
		}
	}
}
