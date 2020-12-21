using System;

namespace Lidgren.Core
{
	public enum DynamicOrder
	{
		RequireABeforeB,
		PreferABeforeB,
		AnyOrder,
		PreferBBeforeA,
		RequireBBeforeA,
	}

	public interface IDynamicOrdered
	{
		DynamicOrder OrderAgainst(object other);
	}

	/// <summary>
	/// Sort ordering of items making individual ordering requests
	/// </summary>
	public static class DynamicOrdering
	{
		public static bool Perform<T>(Span<T> items, out string error) where T : IDynamicOrdered
		{
			int len = items.Length;

			var order = new FastList<int>(len);

			// first; determine number of dependencies for each item; to use as subscore when ordering
			var dependenciesPerItem = new int[len];
			for (int aidx = 0; aidx < len; aidx++)
			{
				for (int bidx = 0; bidx < len; bidx++)
				{
					if (aidx == bidx)
						continue;
					var cmp = GetOrder(items[aidx], items[bidx]);
					if (cmp == DynamicOrder.RequireABeforeB)
						dependenciesPerItem[bidx]++;
				}
			}

			for (int insertOriginalIndex = 0; insertOriginalIndex < len; insertOriginalIndex++)
			{
				if (order.Count == 0)
				{
					order.Add(insertOriginalIndex);
					continue;
				}

				ref readonly var item = ref items[insertOriginalIndex];

				var myDeps = dependenciesPerItem[insertOriginalIndex];

				// find best insertion point for this item
				int bestInsertionIndex = -1;
				int bestScore = -1;
				int bestPrecedingDeps = -1;

				//
				// test all insertion points; which is the best one
				//
				for (int testInsertIndex = 0; testInsertIndex < order.Count + 1; testInsertIndex++) // +1 to test insertion point efter all items
				{
					int score = 0;
					int numPrecedingDeps = 0;
					bool invalid = false;

					// score by those that would come before this insertion point
					for (int b = 0; b < testInsertIndex; b++)
					{
						int beforeIndex = order[b];
						var cmp = GetOrder(item, items[beforeIndex]);
						switch (cmp)
						{
							case DynamicOrder.RequireABeforeB:
								invalid = true;
								break;
							case DynamicOrder.AnyOrder:
								break;
							case DynamicOrder.PreferABeforeB:
								score--;
								break;
							case DynamicOrder.RequireBBeforeA:
							case DynamicOrder.PreferBBeforeA:
								score++;
								break;
						}
						numPrecedingDeps += dependenciesPerItem[beforeIndex];
					}
					if (invalid)
						continue;

					// check afters
					for (int a = testInsertIndex; a < order.Count; a++)
					{
						int afterIndex = order[a];
						var cmp = GetOrder(item, items[afterIndex]);
						switch (cmp)
						{
							case DynamicOrder.RequireBBeforeA:
								invalid = true;
								break;
							case DynamicOrder.PreferBBeforeA:
								score--;
								break;
							case DynamicOrder.PreferABeforeB:
							case DynamicOrder.RequireABeforeB:
								score++;
								break;
							case DynamicOrder.AnyOrder:
								break;
						}
						numPrecedingDeps += myDeps;
					}
					if (invalid)
						continue;

					if ((score > bestScore) || (score == bestScore && numPrecedingDeps < bestPrecedingDeps))
					{
						bestScore = score;
						bestPrecedingDeps = numPrecedingDeps;
						bestInsertionIndex = testInsertIndex;
					}
				}

				if (bestInsertionIndex == -1)
				{
					error = "Failed to insert " + item + " for dynamic ordering";
					return false;
				}

				order.Insert(bestInsertionIndex, insertOriginalIndex);

				//Console.WriteLine("Inserting " + items[insertOriginalIndex] + " at position " + bestInsertionIndex);
				//for (int i = 0; i < order.Count; i++)
				//	Console.WriteLine("  " + i + ": " + items[order[i]]);
			}

			// create ordered array
			T[] result = new T[items.Length];
			for (int i = 0; i < result.Length; i++)
				result[i] = items[order[i]];
			result.AsSpan().CopyTo(items);

			error = "";
			return true;
		}

		public static DynamicOrder GetOrder(IDynamicOrdered a, IDynamicOrdered b)
		{
			var AtoB = a.OrderAgainst(b);
			var BtoA = b.OrderAgainst(a);
			return Reconcile(AtoB, BtoA);
		}

		/// <summary>
		/// Returns dynamic order A -> B
		/// </summary>
		public static DynamicOrder Reconcile(DynamicOrder AtoB, DynamicOrder BtoA)
		{
			switch (AtoB)
			{
				case DynamicOrder.AnyOrder:
					return Reverse(BtoA);

				case DynamicOrder.RequireABeforeB:
					CoreException.Assert(BtoA != DynamicOrder.RequireABeforeB, "Inconsistent dynamic ordering");
					return AtoB;

				case DynamicOrder.RequireBBeforeA:
					CoreException.Assert(BtoA != DynamicOrder.RequireBBeforeA, "Inconsistent dynamic ordering");
					return AtoB;

				case DynamicOrder.PreferABeforeB:
					if (BtoA == DynamicOrder.PreferABeforeB)
						return DynamicOrder.AnyOrder; // clash in preferrence; but it's ok
					if (BtoA == DynamicOrder.RequireABeforeB)
						return DynamicOrder.RequireBBeforeA; // require takes precedence
					return AtoB;

				case DynamicOrder.PreferBBeforeA:
					if (BtoA == DynamicOrder.PreferBBeforeA)
						return DynamicOrder.AnyOrder; // clash in preferrence; but it's ok
					if (BtoA == DynamicOrder.RequireBBeforeA)
						return DynamicOrder.RequireABeforeB; // require takes precedence
					return AtoB;

				default:
					CoreException.ThrowNotImplemented();
					return DynamicOrder.AnyOrder;
			}
		}

		private static DynamicOrder Reverse(DynamicOrder cmp)
		{
			switch (cmp)
			{
				case DynamicOrder.RequireABeforeB:
					return DynamicOrder.RequireBBeforeA;
				case DynamicOrder.PreferABeforeB:
					return DynamicOrder.PreferBBeforeA;
				case DynamicOrder.AnyOrder:
					return DynamicOrder.AnyOrder;
				case DynamicOrder.PreferBBeforeA:
					return DynamicOrder.PreferABeforeB;
				case DynamicOrder.RequireBBeforeA:
					return DynamicOrder.RequireABeforeB;
				default:
					CoreException.ThrowNotImplemented();
					return DynamicOrder.AnyOrder;
			}
		}
	}
}
