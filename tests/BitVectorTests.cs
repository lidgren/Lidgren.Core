using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class BitVectorTests
	{
		[TestMethod]
		public void TestBitVector()
		{
			var facit = new bool[9 * 64];
			for (int r = 1; r < 8; r++)
			{
				var sz = r * 64;
				var bv = new BitVector(sz);
				facit.AsSpan().Clear();

				for (int op = 0; op < 10000; op++)
				{
					var a = PRNG.Next(0, sz);
					var t = PRNG.Next(0, 14);
					switch (t)
					{
						case 0:
							bv.Clear();
							facit.AsSpan().Clear();
							break;
						case 1:
							bv.Set(a, true);
							facit[a] = true;
							break;
						case 2:
						case 3:
							bv.Set(a);
							facit[a] = true;
							break;
						case 4:
						case 5:
						case 6:
							bv[a] = true;
							facit[a] = true;
							break;
						case 7:
						case 8:
							bv.Clear(a);
							facit[a] = false;
							break;
						case 9:
							bv.Set(a, false);
							facit[a] = false;
							break;
						case 10:
							bv[a] = false;
							facit[a] = false;
							break;
						case 11:
						case 12:
						case 13:
							bv.Flip(a);
							if (facit[a])
								facit[a] = false;
							else
								facit[a] = true;
							break;
					}

					// verify
					var cnt = 0;
					for (int i = 0; i < sz; i++)
					{
						Assert.AreEqual(facit[i], bv[i]);
						if (facit[i])
							cnt++;
					}
					Assert.AreEqual(cnt, bv.CountSetBits());
				}
			}
		}
	}
}
