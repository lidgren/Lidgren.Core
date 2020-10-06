using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;
using System.Numerics;

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
					work.WriteVariableUInt64(numbers[i]);

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
					work.WriteVariableInt64(signed[i]);

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
			span.WriteVector2(new Vector2(1.0f, 2.0f));
			span.WriteLengthPrefixedArray<byte>(new byte[] { 43, 42, 41 });
			span.WriteVector3(new Vector3(1.0f, 2.0f, 3.0f));
			span.WriteVector4(new Vector4(1.0f, 2.0f, 3.0f, 4.0f));
			span.WriteQuaternion(new Quaternion(1.0f, 2.0f, 3.0f, 4.0f));
			span.WriteInt32(-120);
			span.WriteString("Pickle Rick");

			var len = arr.Length - span.Length;

			var rdr = new ReadOnlySpan<byte>(arr, 0, len);
			Assert.IsTrue(rdr.ReadBool());
			Assert.AreEqual(1.2, rdr.ReadDouble());
			Assert.AreEqual("Pararibulitis", rdr.ReadString());
			Assert.AreEqual(12, rdr.ReadUInt16());
			Assert.IsTrue(rdr.ReadVector2() == new Vector2(1.0f, 2.0f));
			var barr = rdr.ReadLengthPrefixedArray<byte>();
			Assert.AreEqual(3, barr.Length);
			Assert.AreEqual(43, barr[0]);
			Assert.AreEqual(42, barr[1]);
			Assert.AreEqual(41, barr[2]);
			Assert.IsTrue(rdr.ReadVector3() == new Vector3(1.0f, 2.0f, 3.0f));
			Assert.IsTrue(rdr.ReadVector4() == new Vector4(1.0f, 2.0f, 3.0f, 4.0f));
			Assert.IsTrue(rdr.ReadQuaternion() == new Quaternion(1.0f, 2.0f, 3.0f, 4.0f));
			Assert.AreEqual(-120, rdr.ReadInt32());

			var tmp = new char[32];
			var plen = rdr.ReadString(tmp.AsSpan());
			Assert.AreEqual("Pickle Rick".Length, plen);
			Assert.IsTrue(tmp.AsSpan(0, plen).SequenceEqual("Pickle Rick".AsSpan()));

			Assert.AreEqual(0, rdr.Length);
		}
	}
}
