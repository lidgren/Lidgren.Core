using System;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public partial class StringBuilderTests
	{
		[TestMethod]
		public void TestStringBuilders()
		{
			{
				Span<char> buf = stackalloc char[128];
				var bdr = new FixedStringBuilder(buf);
				bdr.AppendLine("Hello World");
				bdr.Append("Hello");
				bdr.Append(' ');
				bdr.Append(12);
				bdr.Append(' ');
				bdr.AppendLine("Worlds");
				int numReplaced = bdr.Replace('e', 'u');
				Assert.AreEqual(2, numReplaced);
				Assert.AreEqual("Hullo World\nHullo 12 Worlds\n".Length, bdr.Length);
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("Hullo World\nHullo 12 Worlds\n"));

				bdr.Replace("Hullo", "Hi");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("Hi World\nHi 12 Worlds\n"));

				bdr.Replace("Hi ", "");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));

				bdr.Replace("World", "World");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));
				bdr.Replace("Florka", "--");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));
				bdr.Replace("Florka", "Florka");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));

				bdr.Clear();
				bdr.Append("Koko");
				Assert.AreEqual("Koko".Length, bdr.Length);
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("Koko"));
			}

			{
				var sbdr = new ValueStringBuilder(8);
				sbdr.AppendLine("Hello World");
				sbdr.Append("Hello");
				sbdr.Append(' ');
				sbdr.Append(12);
				sbdr.Append(' ');
				sbdr.AppendLine("Worlds");
				var numReplaced = sbdr.Replace('e', 'u');
				Assert.AreEqual(2, numReplaced);
				Assert.AreEqual("Hullo World\nHullo 12 Worlds\n".Length, sbdr.Length);
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("Hullo World\nHullo 12 Worlds\n"));

				sbdr.Replace("Hullo", "Hi");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("Hi World\nHi 12 Worlds\n"));

				sbdr.Replace("Hi ", "");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));

				sbdr.Replace("World", "World");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));
				sbdr.Replace("Florka", "--");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));
				sbdr.Replace("Florka", "Florka");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("World\n12 Worlds\n"));

				sbdr.Clear();
				sbdr.Append("Koko");
				Assert.AreEqual("Koko".Length, sbdr.Length);
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("Koko"));
			}
		}
	}
}
