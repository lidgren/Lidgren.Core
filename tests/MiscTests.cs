using System;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public partial class MiscTests
	{
		[TestMethod]
		public void TestFourCC()
		{
			var v1 = new FourCC("avc1");
			Assert.AreEqual(0x31637661u, v1.Value);
			Assert.AreEqual("avc1", v1.ToString());

			var v2 = new FourCC('a', 'v', 'c', '1');
			Assert.AreEqual(0x31637661u, v2.Value);
			Assert.AreEqual("avc1", v2.ToString());

			var v3 = new FourCC(new byte[] { (byte)'a', (byte)'v', (byte)'c', (byte)'1' });
			Assert.AreEqual(0x31637661u, v3.Value);
			Assert.AreEqual("avc1", v3.ToString());

			var v4 = new FourCC(0x31637661u);
			Assert.AreEqual(0x31637661u, v4.Value);
			Assert.AreEqual("avc1", v4.ToString());
		}

		[TestMethod]
		public void TestRandomSeed()
		{
			// since it's random; these != 0 checks MAY THEORETICALLY fail; but the risk should be 1 in uint.maxvalue
			var a = RandomSeed.GetUInt32();
			Assert.IsTrue(a != 0);
			var b = RandomSeed.GetUInt64();
			Assert.IsTrue(b != 0);
			var expanded = RandomSeed.ExpandSeed(123);
			Assert.IsTrue((expanded & 0b11111111_11111111_11111111_11111111_00000000_00000000_00000000_00000000ul) != 0);
		}

		[TestMethod]
		public void TestHasher()
		{
			var hasher = Hasher.Create();
			hasher.Add((byte)0);
			hasher.Add((uint)42 << 5);
			hasher.Add((ulong)42 << 48);
			hasher.Add("string");
			hasher.Add('c');
			hasher.Add(new byte[] { 1, 2, 3, 4 });
			hasher.Finalize64();

			var h1 = Hasher.Create();
			h1.Add((byte)42);
			h1.Add((byte)43);
			h1.Add((byte)44);
			var a1 = h1.Finalize64();

			var h2 = Hasher.Create();
			h2.Add(new byte[] { (byte)42, (byte)43, (byte)44 });
			var a2 = h2.Finalize64();

			Assert.AreEqual(a1, a2);
		}

		[TestMethod]
		public void TestModulo()
		{
			Assert.AreEqual(MathUtils.Modulo(4.0f, 5.0f), 4.0f);
			Assert.AreEqual(MathUtils.Modulo(5.0f, 5.0f), 0.0f);
			Assert.AreEqual(MathUtils.Modulo(6.0f, 5.0f), 1.0f);
			Assert.AreEqual(MathUtils.Modulo(12.0f, 5.0f), 2.0f);
			Assert.AreEqual(MathUtils.Modulo(-1.0f, 5.0f), 4.0f);
			Assert.AreEqual(MathUtils.Modulo(-10.0f, 5.0f), 0.0f);
			Assert.AreEqual(MathUtils.Modulo(-11.0f, 5.0f), 4.0f);

			Assert.AreEqual(MathUtils.Modulo(0.0f, -5.0f), 0.0f);
			Assert.AreEqual(MathUtils.Modulo(-1.0f, -5.0f), -1.0f);
			Assert.AreEqual(MathUtils.Modulo(-6.0f, -5.0f), -1.0f);
			Assert.AreEqual(MathUtils.Modulo(2.0f, -5.0f), -3.0f);

			Assert.AreEqual(MathUtils.Modulo(4, 5), 4);
			Assert.AreEqual(MathUtils.Modulo(5, 5), 0);
			Assert.AreEqual(MathUtils.Modulo(6, 5), 1);
			Assert.AreEqual(MathUtils.Modulo(12, 5), 2);
			Assert.AreEqual(MathUtils.Modulo(-1, 5), 4);
			Assert.AreEqual(MathUtils.Modulo(-10, 5), 0);
			Assert.AreEqual(MathUtils.Modulo(-11, 5), 4);

			Assert.AreEqual(MathUtils.Modulo(0, -5), 0);
			Assert.AreEqual(MathUtils.Modulo(-1, -5), -1);
			Assert.AreEqual(MathUtils.Modulo(-6, -5), -1);
			Assert.AreEqual(MathUtils.Modulo(2, -5), -3);
		}

		[TestMethod]
		public void TestBlockSwap()
		{
			var span = "1234BBB".ToCharArray().AsSpan();
			span.SwapBlocks(4);
			Assert.AreEqual("BBB1234", span.ToString());

			span = "12abcdefg".ToCharArray().AsSpan();
			span.SwapBlocks(1);
			Assert.AreEqual("2abcdefg1", span.ToString());

			span = "1234".ToCharArray().AsSpan();
			span.SwapBlocks(0);
			Assert.AreEqual("1234", span.ToString());

			span = "1234".ToCharArray().AsSpan();
			span.SwapBlocks(4);
			Assert.AreEqual("1234", span.ToString());

			span = "1234".ToCharArray().AsSpan();
			span.SwapBlocks(3);
			Assert.AreEqual("4123", span.ToString());
		}
	}
}
