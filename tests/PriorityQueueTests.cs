using System;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public partial class PriorityQueueTests
	{
		[TestMethod]
		public void TestPriorityQueue()
		{
			var queue = new PriorityQueue<int, string>(4);

			queue.Enqueue(5, "five");
			queue.Enqueue(3, "three");
			queue.Enqueue(8, "eight");
			queue.Enqueue(1, "one");
			queue.Enqueue(3, "three-again");

			bool ok = queue.TryDequeue(out var str);
			Assert.IsTrue(ok);
			Assert.AreEqual("one", str);

			ok = queue.TryDequeue(out str);
			Assert.IsTrue(ok);
			Assert.IsTrue(str == "three" || str == "three-again");
			ok = queue.TryDequeue(out str);
			Assert.IsTrue(ok);
			Assert.IsTrue(str == "three" || str == "three-again");

			ok = queue.PeekPriority(out var nextPrio);
			Assert.IsTrue(ok);
			Assert.AreEqual(5, nextPrio);

			ok = queue.TryDequeue(out str);
			Assert.IsTrue(ok);
			Assert.AreEqual("five", str);

			Assert.AreEqual(1, queue.Count);

			queue.Enqueue(6, "six");

			ok = queue.TryDequeue(out str);
			Assert.IsTrue(ok);
			Assert.AreEqual("six", str);

			ok = queue.TryDequeue(out str);
			Assert.IsTrue(ok);
			Assert.AreEqual("eight", str);

			ok = queue.TryDequeue(out str);
			Assert.IsFalse(ok);
			Assert.AreEqual(0, queue.Count);
		}
	}
}
