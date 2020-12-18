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

				var expected = "Hullo World" + Environment.NewLine + "Hullo 12 Worlds" + Environment.NewLine;
				Assert.AreEqual(expected.Length, bdr.Length);
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual(expected));

				numReplaced = bdr.Replace("Hullo", "Hi");
				Assert.AreEqual(2, numReplaced);
				expected = expected.Replace("Hullo", "Hi");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual(expected));

				numReplaced = bdr.Replace("Hi", "");
				Assert.AreEqual(2, numReplaced);
				expected = expected.Replace("Hi", "");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual(expected));

				bdr.Replace("World", "World");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual(expected));
				bdr.Replace("Florka", "--");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual(expected));
				bdr.Replace("Florka", "Florka");
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual(expected));

				bdr.Clear();
				Assert.AreEqual(0, bdr.Length);
				Assert.AreEqual("", bdr.ToString());
				bdr.Append("Koko");
				bdr.Append('s');
				Assert.AreEqual("Kokos".Length, bdr.Length);
				Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("Kokos"));
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

				var expected = "Hullo World" + Environment.NewLine + "Hullo 12 Worlds" + Environment.NewLine;
				Assert.AreEqual(expected.Length, sbdr.Length);
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual(expected));

				numReplaced = sbdr.Replace("Hullo", "Hi");
				Assert.AreEqual(2, numReplaced);
				expected = expected.Replace("Hullo", "Hi");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual(expected));

				numReplaced = sbdr.Replace("Hi", "");
				Assert.AreEqual(2, numReplaced);
				expected = expected.Replace("Hi", "");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual(expected));

				sbdr.Replace("World", "World");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual(expected));
				sbdr.Replace("Florka", "--");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual(expected));
				sbdr.Replace("Florka", "Florka");
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual(expected));

				sbdr.Clear();
				Assert.AreEqual(0, sbdr.Length);
				Assert.AreEqual("", sbdr.ToString());
				sbdr.Append("Koko");
				sbdr.Append('s');
				Assert.AreEqual("Kokos".Length, sbdr.Length);
				Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("Kokos"));
			}
		}
	}
}
