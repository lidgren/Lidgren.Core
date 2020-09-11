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
		public void TestSpanExtensions()
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

		[TestMethod]
		public void TestTimeServiceDuration()
		{
			var ok = TimeService.ParseDuration("3h2m12s", out var seconds);

			var facit = (3 * 60 * 60);
			facit += (2 * 60);
			facit += 12;

			Assert.IsTrue(ok);
			Assert.AreEqual(facit, seconds);
		}
	}
}
