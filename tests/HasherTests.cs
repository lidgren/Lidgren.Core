using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;
using System.Collections.Generic;

namespace UnitTests
{
	[TestClass]
	public class HasherTests
	{
		[TestMethod]
		public void TestHasher()
		{
			var data = new byte[240];
			for (int i = 0; i < data.Length; i++)
				data[i] = (byte)i;

			var seen32 = new HashSet<uint>();
			var seen64 = new HashSet<ulong>();

			var hasher = Hasher.Create();
			for (int len = 1; len < data.Length; len++)
			{
				hasher.Reset();
				hasher.Add(data.AsSpan(0, len));
				var h32 = hasher.Finalize32();
				Assert.IsFalse(seen32.Contains(h32));

				hasher.Reset();
				hasher.Add(data.AsSpan(0, len));
				var h64 = hasher.Finalize64();
				Assert.IsFalse(seen64.Contains(h64));
			}

			{
				hasher.Reset();
				hasher.Add("abc");
				var one = hasher.Finalize64();
				hasher.Reset();
				hasher.AddLower("AbC");
				var two = hasher.Finalize64();
				Assert.AreEqual(one, two);
			}

			{
				hasher.Reset();
				hasher.Add((byte)1);
				hasher.Add((byte)2);
				var one = hasher.Finalize64();

				hasher.Reset();
				hasher.Add((byte)2);
				hasher.Add((byte)1);
				var two = hasher.Finalize64();

				Assert.AreNotEqual(one, two);
			}

		}
	}
}
