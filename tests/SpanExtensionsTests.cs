using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class SpanExtensionsTests
	{
		[TestMethod]
		public void Test7BitEncoding()
		{
			byte[] arr = new byte[1024];

			// test variable lengths
			var numbers = new ulong[7];
			numbers = new ulong[] { 1, 25, 137, 255, 256, 10000, 1000000 };

			ulong state = RandomSeed.GetUInt64();
			for (int runs = 0; runs < 250000; runs++)
			{
				var work = arr.AsSpan();
				for (int i = 0; i < numbers.Length; i++)
					work.WriteVariable(numbers[i]);

				int resultLength = arr.Length - work.Length;

				ReadOnlySpan<byte> res = arr.AsSpan(0, resultLength);

				for (int i = 0; i < numbers.Length; i++)
				{
					var nr = res.ReadVariableUInt64();
					Assert.AreEqual(numbers[i], nr);
				}

				// re-randomize
				numbers[0] = (ulong)PRNG.Next(ref state, 0, 6);
				numbers[1] = (ulong)PRNG.Next(ref state, 7, 300);
				numbers[2] = (ulong)PRNG.Next(ref state, 300, 5000);
				numbers[3] = (ulong)PRNG.Next(ref state, 5000, 50000);
				numbers[4] = (ulong)PRNG.Next(ref state, 50000, 500000);
				numbers[5] = (ulong)PRNG.NextUInt64(ref state);
				numbers[6] = (ulong)PRNG.NextUInt64(ref state);
			}

			// signed
			var signed = new long[7];
			for (int runs = 0; runs < 250000; runs++)
			{
				// re-randomize
				signed[0] = (long)PRNG.Next(ref state, -5, 5);
				signed[1] = (long)PRNG.Next(ref state, -100, 100);
				signed[2] = (long)PRNG.Next(ref state, -300, 300);
				signed[3] = (long)PRNG.Next(ref state, -5000, 5000);
				signed[4] = (long)PRNG.Next(ref state, -70000, 70000);
				signed[5] = (long)PRNG.NextUInt64(ref state);
				signed[6] = (long)PRNG.NextUInt64(ref state);

				var work = arr.AsSpan();
				for (int i = 0; i < signed.Length; i++)
					work.WriteVariable(signed[i]);

				int resultLength = arr.Length - work.Length;

				ReadOnlySpan<byte> res = arr.AsSpan(0, resultLength);

				for (int i = 0; i < signed.Length; i++)
				{
					var nr = res.ReadVariableInt64();
					Assert.AreEqual(signed[i], nr);
				}
			}
		}

		[TestMethod]
		public void TestSpanExtensions()
		{
			var arr = new byte[1024];

			var span = arr.AsSpan();
			span.WriteBool(true);
			span.WriteDouble(1.2);
			span.WriteString("Pararibulitis");
			span.WriteUInt16(12);
			span.WriteInt32(-120);
			span.WriteString("Pickle Rick");

			var len = arr.Length - span.Length;

			var rdr = new ReadOnlySpan<byte>(arr, 0, len);
			Assert.IsTrue(rdr.ReadBool());
			Assert.AreEqual(1.2, rdr.ReadDouble());
			Assert.AreEqual("Pararibulitis", rdr.ReadString());
			Assert.AreEqual(12, rdr.ReadUInt16());
			Assert.AreEqual(-120, rdr.ReadInt32());

			var tmp = new char[32];
			var plen = rdr.ReadString(tmp.AsSpan());
			Assert.AreEqual("Pickle Rick".Length, plen);
			Assert.IsTrue(tmp.AsSpan(0, plen).SequenceEqual("Pickle Rick".AsSpan()));

			Assert.AreEqual(0, rdr.Length);
		}
	}
}
