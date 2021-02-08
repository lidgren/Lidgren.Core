using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class FastListTests
	{
		[TestMethod]
		public void TestFastList()
		{
			var facit = new List<int>(4);
			var fast = new FastList<int>();
			var repro = new FastList<int>();

			int[] range = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			int[] smallRange = new int[] { 11, 12, 13 };

			Predicate<int> match = (a) =>
			{
				return a > 50000;
			};

			// run a million operations
			for (int rounds = 0; rounds < 1000000; rounds++)
			{
				// random operations!
				var op = PRNG.Next(0, 19);
				int removeIndex = PRNG.Next(0, Math.Max(1, facit.Count));
				var value = PRNG.Next(0, 100000);
				switch (op)
				{
					case 0:
						facit.Add(value);
						fast.Add(value);
						repro.Add(value);
						break;
					case 1:
						{
							facit.Add(value);
							ref var slot2 = ref fast.Add();
							slot2 = value;

							Verify(facit, fast);

							ref var slot3 = ref repro.Add();
							slot3 = value;
							break;
						}
					case 2:
						{
							facit.Add(value);
							ref var slot2 = ref fast.AddUninitialized();
							slot2 = value;
							ref var slot3 = ref repro.AddUninitialized();
							slot3 = value;
							break;
						}
					case 3:
						if (facit.Count > 0)
						{
							facit.RemoveAt(removeIndex);
							fast.RemoveAt(removeIndex);
							repro.RemoveAt(removeIndex);
						}
						break;
					case 4:
						if (facit.Count > 0)
						{
							int removeValue = facit[removeIndex];
							facit.Remove(removeValue);
							fast.Remove(removeValue);
							Verify(facit, fast);
							repro.Remove(removeValue);
						}
						break;
					case 5:
						facit.Clear();
						fast.Clear();
						repro.Clear();
						break;
					case 6:
						facit.Clear();
						fast.Clear();
						repro.Clear();
						break;
					case 7:
						{
							facit.AddRange(range);
							fast.AddRange(range);
							repro.AddRange(range);
						}
						break;
					case 8:
						{
							facit.AddRange(range);

							var span2 = fast.AddRangeUninitialized(range.Length);
							range.AsSpan().CopyTo(span2);

							var span3 = repro.AddRangeUninitialized(range.Length);
							range.AsSpan().CopyTo(span3);
						}
						break;
					case 9:
						{
							if (facit.Count > 0)
							{
								int index = PRNG.Next(0, facit.Count + 1); // can insert after all existing indices as well
								facit.Insert(index, value);
								fast.Insert(index, value);
								repro.Insert(index, value);
							}
						}
						break;
					case 10:
						{
							if (facit.Count > 0)
							{
								int index = PRNG.Next(0, facit.Count);
								facit.Insert(index, value);
								ref var slot2 = ref fast.InsertUninitialized(index);
								slot2 = value;
								ref var slot3 = ref repro.InsertUninitialized(index);
								slot3 = value;
							}
						}
						break;
					case 11:
						if (facit.Count > 0)
						{
							int index = PRNG.Next(0, facit.Count);
							facit[index] = value;
							fast[index] = value;
							repro[index] = value;
						}
						break;
					case 12:
						if (facit.Count > 0)
						{
							facit.RemoveAll(match);
							fast.RemoveAll(match);
							repro.RemoveAll(match);
						}
						break;
					case 13:
						facit.Add(value);
						fast.Add(value);
						repro.Add(value);
						break;
					case 14:
						facit.Insert(0, value);
						ref var zeroIns = ref fast.InsertUninitialized(0);
						zeroIns = value;
						ref var zeroIns2 = ref repro.InsertUninitialized(0);
						zeroIns2 = value;
						break;
					case 15:
						if (facit.Count > 0)
						{
							int lastValue = facit[facit.Count - 1];
							facit.RemoveAt(facit.Count - 1);
							int cmp = fast.Pop();
							Assert.IsTrue(lastValue == cmp);

							Verify(facit, fast);

							repro.Pop();
						}
						break;

					case 16:
						{
							facit.AddRange(smallRange);
							var span = fast.AddRangeUninitialized(smallRange.Length);
							smallRange.CopyTo(span);
							var span2 = repro.AddRangeUninitialized(smallRange.Length);
							smallRange.CopyTo(span2);
						}
						break;

					case 17:
						if (facit.Count > 0)
						{
							int lastValue = facit[facit.Count - 1];
							facit.RemoveAt(facit.Count - 1);
							bool ok = fast.TryPop(out var cmp);
							Assert.IsTrue(ok);
							Assert.IsTrue(lastValue == cmp);

							Verify(facit, fast);

							repro.TryPop(out var cmp2);
						}
						else
						{
							bool ok = fast.TryPop(out var unused);
							Assert.IsTrue(ok == false);
						}
						break;

					case 18:
						if (facit.Count > 3)
						{
							int idx = PRNG.Next(0, facit.Count);
							int maxRem = facit.Count - idx;
							int remCnt = PRNG.Next(0, maxRem + 1);

							facit.RemoveRange(idx, remCnt);
							fast.RemoveRange(idx, remCnt);
							Verify(facit, fast);
							repro.RemoveRange(idx, remCnt);
						}
						break;
				}

				// verify fast
				Verify(facit, fast);

				// check random index
				if (facit.Count > 0)
				{
					int rndIdx = PRNG.Next(0, facit.Count);
					var val = facit[rndIdx];

					// verify indexer
					Assert.IsTrue(fast[rndIdx] == val);

					// verify indexof
					var correct = facit.IndexOf(val);
					Assert.IsTrue(fast.IndexOf(val) == correct);

					// verify lastindexof
					correct = facit.LastIndexOf(val);
					Assert.IsTrue(fast.LastIndexOf(val) == correct);

					// verify contains
					Assert.IsTrue(facit.Contains(val));
					Assert.IsTrue(fast.Contains(val));
				}
			}
		}

		private void Verify(List<int> facit, FastList<int> fast)
		{
			Assert.AreEqual(facit.Count, fast.Count);
			var fastItems = fast.ReadOnlySpan;
			for (int i = 0; i < fastItems.Length; i++)
				Assert.AreEqual(facit[i], fastItems[i]);
			Assert.AreEqual(fast.ReadOnlySpan.Length, fast.Count);
			Assert.IsTrue(fast.Capacity >= fast.Count);

			for (int i = 0; i < fastItems.Length; i++)
				Assert.AreEqual(facit[i], fastItems[i]);
		}

		[TestMethod]
		public void TestAddList()
		{
			var add = new AddList<int>();
			add.Add(12);
			add.Add(13);
			add.Add(14);
			Assert.AreEqual(3, add.Count);
			var span = add.ReadOnlySpan;
			Assert.AreEqual(12, span[0]);
			Assert.AreEqual(13, span[1]);
			Assert.AreEqual(14, span[2]);
			Assert.AreEqual(3, span.Length);
		}

		[TestMethod]
		public void TestGroupBy()
		{
			var list = new FastList<int>();
			list.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 9, 8, 7 });

			var dict = list.GroupBy<string>((x) => ((x & 1) == 0) ? "even" : "odd");

			Assert.AreEqual(2, dict.Count);

			var even = dict["even"];
			Assert.IsNotNull(even);
			Assert.AreEqual(4, even.Count);
			Assert.IsTrue(even.Contains(2));
			Assert.IsTrue(even.Contains(4));
			Assert.IsTrue(even.Contains(6));
			Assert.IsTrue(even.Contains(8));

			var odd = dict["odd"];
			Assert.IsNotNull(odd);
			Assert.AreEqual(5, odd.Count);
			Assert.IsTrue(odd.Contains(1));
			Assert.IsTrue(odd.Contains(3));
			Assert.IsTrue(odd.Contains(5));
			Assert.IsTrue(odd.Contains(7));
			Assert.IsTrue(odd.Contains(9));
		}

		[TestMethod]
		public void TestInsertRange()
		{
			var tmp = new List<uint>(32);
			var tmpArr = new uint[32];

			for (int run = 0; run < 10000; run++)
			{
				var cap = PRNG.Next(2, 7);
				var fast = new FastList<uint>(cap);
				var list = new List<uint>(cap);

				var numOps = PRNG.Next(2, 4);
				for (int o = 0; o < numOps; o++)
				{
					tmp.Clear();
					var tmpLen = PRNG.Next(1, 9);
					for (int n = 0; n < tmpLen; n++)
						tmp.Add(PRNG.NextUInt32());

					var insIdx = list.Count == 0 ? 0 : PRNG.Next(0, list.Count);

					// go!
					tmp.CopyTo(tmpArr);
					fast.InsertRange(insIdx, tmpArr.AsSpan(0, tmpLen));
					list.InsertRange(insIdx, tmp);

					// verify
					Assert.AreEqual(list.Count, fast.Count);
					for (int v = 0; v < list.Count; v++)
						Assert.AreEqual(list[v], fast[v]);
				}
			}
		}
	}
}
