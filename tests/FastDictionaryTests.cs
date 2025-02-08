using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;
using System.Collections.Generic;

namespace UnitTests
{

	[TestClass]
	public class FastDictionaryTests
	{
		[TestMethod]
		public void TestFastDictionary()
		{
			var facit = new Dictionary<int, string>();
			var fast = new FastDictionary<int, string>();

			for (int r = 0; r < 1000000; r++)
			{
				var rnd = PRNG.Next(0, 13);
				switch (rnd)
				{
					case 0:
						facit.Clear();
						fast.Clear();
						break;
					case 1:
					case 2:
						{
							// add/change using indexer
							int key;
							if (facit.Count > 2 && PRNG.Next(0, 100) < 20)
								key = GetRandomKey(facit);
							else
								key = PRNG.Next(1, 1000);
							facit[key] = key.ToString();
							fast[key] = key.ToString();
							break;
						}
					case 3:
					case 4:
						// add using Add()
						int b = PRNG.Next(1, 1000);
						if (facit.ContainsKey(b) == false)
						{
							facit.Add(b, b.ToString());
							fast.Add(b, b.ToString());
						}
						break;
					case 5:
					case 6:
						{
							// add using GetOrInit()
							int c = PRNG.Next(0, 1000);
							bool exists = facit.ContainsKey(c);
							ref var str = ref fast.GetOrInit(c, out bool wasCreated);
							Assert.AreEqual(exists, !wasCreated);
							str = c.ToString();
							if (!exists)
								facit[c] = c.ToString();
							break;
						}
					case 7:
					case 8:
						{
							// remove existing
							if (facit.Count < 1)
								break;
							var key = GetRandomKey(facit);
							var ok = facit.Remove(key);
							Assert.IsTrue(ok);
							ok = fast.Remove(key);
							Assert.IsTrue(ok);
							break;
						}
					case 9:
					case 10:
					case 11:
						{
							// check existing
							if (facit.Count == 0)
								break;
							var key = GetRandomKey(facit);
							Assert.IsTrue(facit.ContainsKey(key));
							Assert.IsTrue(fast.ContainsKey(key));

							bool ok = fast.TryGetValue(key, out var val);
							Assert.IsTrue(ok);
							Assert.AreEqual(key.ToString(), val);

							ref var str = ref fast.TryGetRef(key, out bool wasFound);
							Assert.IsTrue(wasFound);
							Assert.AreEqual(key.ToString(), str);
						}
						break;
					case 12:
						{
							// check non-existing
							var key = int.MaxValue;
							Assert.IsFalse(facit.ContainsKey(key));
							Assert.IsFalse(fast.ContainsKey(key));

							bool ok = fast.TryGetValue(key, out var val);
							Assert.IsFalse(ok);

							ref var str = ref fast.TryGetRef(key, out bool wasFound);
							Assert.IsFalse(wasFound);
						}
						break;
				}

#if DEBUG
				fast.Validate();
#endif
				Assert.AreEqual(facit.Count, fast.Count);
				foreach (var key in facit.Keys)
					Assert.IsTrue(fast.ContainsKey(key));
				foreach (var value in facit.Values)
					Assert.IsTrue(fast.ContainsValue(value));
				foreach (var key in fast.Keys)
					Assert.IsTrue(facit.ContainsKey(key));
				foreach (var value in fast.Values)
					Assert.IsTrue(facit.ContainsValue(value));
			}
		}

		public int GetRandomKey(Dictionary<int, string> facit)
		{
			Assert.IsTrue(facit.Count > 0);

			int idx = PRNG.Next(0, facit.Count);
			int num = 0;
			foreach (var key in facit.Keys)
			{
				if (idx == num)
					return key;
				num++;
			}
			Assert.Fail();
			return -1;
		}
	}
}
