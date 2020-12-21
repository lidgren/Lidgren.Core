using System;
using System.Collections.Generic;
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

			queue.Clear();
			queue.Enqueue(3, "3");
			queue.Enqueue(1, "1");
			queue.Enqueue(2, "2");
			ok = queue.Remove("22");
			Assert.IsFalse(ok);
			Assert.AreEqual(3, queue.Count);
			ok = queue.Remove("2");
			Assert.IsTrue(ok);

			Assert.AreEqual("1", queue.Dequeue());
			Assert.AreEqual("3", queue.Dequeue());
			ok = queue.TryDequeue(out _);
			Assert.IsFalse(ok);

			var facit = new List<(int, string)>();
			queue.Clear();
			for(int op = 0; op < 1000000; op++)
			{
				switch (PRNG.Next(0, 8))
				{
					case 0:
						queue.Clear();
						facit.Clear();
						break;
					case 1:
					case 2:
					case 3:
					case 4:
						// enqueue
						int add = PRNG.Next(0, 50);
						queue.Enqueue(add, add.ToString());
						facit.Add((add, add.ToString()));
						break;
					case 5:
						// remove
						int rem = PRNG.Next(0, 50);
						var fok = Remove(facit, rem.ToString());
						var qok = queue.Remove(rem.ToString());
						Assert.AreEqual(fok, qok);
						break;
					case 6:
					case 7:
						// dequeue
						if (facit.Count <= 0)
							continue;
						var fitem = Dequeue(facit);
						var qitem = queue.Dequeue();
						Assert.AreEqual(fitem, qitem);
						break;
				}

				Assert.AreEqual(facit.Count, queue.Count);
				if (facit.Count > 0)
				{
					ok = queue.Peek(out var peekprio, out var peekitem);
					Assert.IsTrue(ok);
					Peek(facit, out var facitpeekprio, out var facitpeekitem);

					Assert.AreEqual(facitpeekprio, peekprio);
					Assert.AreEqual(facitpeekitem, peekitem);
				}
			}
		}

		private void Peek(List<(int, string)> facit, out int facitpeekprio, out string facitpeekitem)
		{
			int lowestIndex = -1;
			int lowestPrio = int.MaxValue;
			for (int i = 0; i < facit.Count; i++)
			{
				var prio = facit[i].Item1;
				if (prio < lowestPrio)
				{
					lowestPrio = prio;
					lowestIndex = i;
				}
			}

			Assert.IsTrue(lowestIndex != -1);

			facitpeekprio = facit[lowestIndex].Item1;
			facitpeekitem = facit[lowestIndex].Item2;
		}

		private string Dequeue(List<(int, string)> facit)
		{
			int lowestIndex = -1;
			int lowestPrio = int.MaxValue;
			for (int i = 0; i < facit.Count; i++)
			{
				if (facit[i].Item1 < lowestPrio)
				{
					lowestPrio = facit[i].Item1;
					lowestIndex = i;
				}
			}

			var retval = facit[lowestIndex].Item2;
			facit.RemoveAt(lowestIndex);
			return retval;
		}

		private bool Remove(List<(int, string)> facit, string item)
		{
			for (int i = 0; i < facit.Count; i++)
			{
				if (facit[i].Item2 == item)
				{
					facit.RemoveAt(i);
					return true;
				}
			}
			return false;
		}
	}
}
