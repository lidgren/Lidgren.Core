using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class TokenizerTests
	{
		[TestMethod]
		public void TestTokenizer()
		{
			TestSplit("hello this is a string", ' ');
			TestSplit("hello this is a string", '%');
			TestSplit("hello   this   is   a   string", ' ');
			TestSplit("  hello this is a string  ", ' ');
			TestSplit("    ", ' ');


			var str = "split-on,multiple things";
			var t = new Tokenizer<char>(str);

			var ok = t.GetNext("-, ".AsSpan(), out var token);
			Assert.IsTrue(ok);
			Assert.IsTrue(token.SequenceEqual("split"));

			ok = t.GetNext("-, ".AsSpan(), out token);
			Assert.IsTrue(ok);
			Assert.IsTrue(token.SequenceEqual("on"));

			ok = t.GetNext("-, ".AsSpan(), out token);
			Assert.IsTrue(ok);
			Assert.IsTrue(token.SequenceEqual("multiple"));

			ok = t.GetNext("-, ".AsSpan(), out token);
			Assert.IsTrue(ok);
			Assert.IsTrue(token.SequenceEqual("things"));

			ok = t.GetNext("-, ".AsSpan(), out token);
			Assert.IsFalse(ok);
		}

		private void TestSplit(string str, char delimiter)
		{
			var facit = str.Split(' ');

			var tok = new Tokenizer<char>(str);
			for (int i = 0; i < facit.Length; i++)
				Assert.IsTrue(tok.Next(' ').SequenceEqual(facit[i]));
			Assert.IsTrue(tok.Next(' ').IsEmpty);

			Span<Range> tokens = stackalloc Range[32];
			int numTokens = Tokenizer<char>.Split(str, ' ', tokens);
			Assert.AreEqual(facit.Length, numTokens);

			for (int i = 0; i < facit.Length; i++)
			{
				(var off, var len) = tokens[i].GetOffsetAndLength(str.Length);
				Assert.IsTrue(str.AsSpan(off, len).SequenceEqual(facit[i]));
			}
		}
	}
}
