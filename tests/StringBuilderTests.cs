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
			Span<char> buf = stackalloc char[128];

			var bdr = new FixedStringBuilder(buf);
			bdr.AppendLine("Hello World");
			bdr.Append("Hello");
			bdr.Append(' ');
			bdr.Append(12);
			bdr.Append(' ');
			bdr.AppendLine("Worlds");
			Assert.AreEqual("Hello World\nHello 12 Worlds\n".Length, bdr.Length);
			Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("Hello World\nHello 12 Worlds\n"));
			bdr.Clear();
			bdr.Append("Koko");
			Assert.AreEqual("Koko".Length, bdr.Length);
			Assert.IsTrue(bdr.ReadOnlySpan.SequenceEqual("Koko"));

			var sbdr = new ValueStringBuilder(8);
			sbdr.AppendLine("Hello World");
			sbdr.Append("Hello");
			sbdr.Append(' ');
			sbdr.Append(12);
			sbdr.Append(' ');
			sbdr.AppendLine("Worlds");
			Assert.AreEqual("Hello World\nHello 12 Worlds\n".Length, sbdr.Length);
			Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("Hello World\nHello 12 Worlds\n"));
			sbdr.Clear();
			sbdr.Append("Koko");
			Assert.AreEqual("Koko".Length, sbdr.Length);
			Assert.IsTrue(sbdr.ReadOnlySpan.SequenceEqual("Koko"));
		}
	}
}
