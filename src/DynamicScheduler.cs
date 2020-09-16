#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lidgren.Core
{
	public interface IDynamicScheduled : IDynamicOrdered
	{
		string Name { get; }
		bool CanRunConcurrently(object other);
		void Execute(object? argument);
	}

	public class DynamicScheduler<T> where T : IDynamicScheduled
	{
		private readonly T[] m_items;
		private readonly RuntimeItem[] m_runtimeItems;
		private readonly SymmetricMatrixBool m_concurrencyAllowed;

		private struct RuntimeItem
		{
			public int RequireReleases; // how many other items needs to complete before we can go
			public int RemainingRequiredReleased; // when this reaches 0; we're good to go
			public HashSet<int> Releases; // when I complete; which other items to I release?
			public bool Started;
		}

		public DynamicScheduler(ReadOnlySpan<T> items)
		{
			m_items = items.ToArray();

			// first, just order them
			bool ok = DynamicOrdering.Perform<T>(m_items, out var error);
			if (!ok)
				CoreException.Throw(error);

			for (int i = 0; i < m_items.Length; i++)
				StdOut.WriteLine("   " + m_items[i].Name, ConsoleColor.DarkCyan);

			m_runtimeItems = new RuntimeItem[m_items.Length];
			m_concurrencyAllowed = new SymmetricMatrixBool(items.Length);
			for (int a = 0; a < m_items.Length; a++)
			{
				for (int b = a + 1; b < m_items.Length; b++)
				{
					var AtoB = DynamicOrdering.GetOrder(m_items[a], m_items[b]);

					var canRunConcurrently1 = m_items[a].CanRunConcurrently(m_items[b]);
					var canRunConcurrently2 = m_items[b].CanRunConcurrently(m_items[a]);

					// set up concurrency
					var crc = canRunConcurrently1 && canRunConcurrently2;
					m_concurrencyAllowed[a, b] = crc;

					// set up releases
					if (AtoB == DynamicOrder.RequireABeforeB)
					{
						if (m_runtimeItems[a].Releases == null)
							m_runtimeItems[a].Releases = new HashSet<int>();
						m_runtimeItems[a].Releases.Add(b);
					}
					else if (AtoB == DynamicOrder.RequireBBeforeA)
					{
						if (m_runtimeItems[b].Releases == null)
							m_runtimeItems[b].Releases = new HashSet<int>();
						m_runtimeItems[b].Releases.Add(a);
					}
				}
			}

			// update RequireReleases for all items now
			for (int i = 0; i < m_runtimeItems.Length; i++)
			{
				ref var item = ref m_runtimeItems[i];
				if (item.Releases != null)
				{
					foreach (var r in item.Releases)
						m_runtimeItems[r].RequireReleases++;
				}
			}
		}

		private int m_workerIndex; // helper for each worker to get an index

		public void Execute(string jobName, object? argument, int maxConcurrency = int.MaxValue, Action<object?>? continuation = null, object? continuationArgument = null)
		{
			CoreException.Assert(JobService.IsInitialized);

			lock (m_runtimeItems)
			{
				for (int i = 0; i < m_runtimeItems.Length; i++)
				{
					ref var item = ref m_runtimeItems[i];
					item.Started = false;
					item.RemainingRequiredReleased = item.RequireReleases;
				}
			}

			// start wide job
			m_workerIndex = 0;
			JobService.EnqueueWideBlock(maxConcurrency, jobName, ExecuteWorker, argument);
		}

		internal void ExecuteWorker(object? argument)
		{
			int myIndex = Interlocked.Increment(ref m_workerIndex) - 1;

			for (; ; )
			{
				int runItemIndex = -1;
				int unstartedCount = 0;
				lock (m_runtimeItems)
				{
					// find scheduled item due for execution
					for (int i = 0; i < m_runtimeItems.Length; i++)
					{
						ref var item = ref m_runtimeItems[i];
						if (item.Started)
							continue;
						unstartedCount++;
						if (item.RemainingRequiredReleased > 0)
							continue;

						// found item!
						item.Started = true;
						runItemIndex = i;
						break;
					}
				}

				if (runItemIndex < 0)
				{
					// ok, no item for me to execute right now

					if (unstartedCount == 0)
						return; // all items have been started; exit this worker

					if (myIndex >= unstartedCount)
						return; // enough workers exist to handle remaining items; exit this worker

					// ok, relax and try again in a while
					Thread.Sleep(0);
					continue;
				}

				while (runItemIndex >= 0)
				{
					// yay
					ref readonly var item = ref m_items[runItemIndex];

					using (new Timing(item.Name))
						item.Execute(argument);

					// do releases
					int nextRuntimeItem = -1;
					lock (m_runtimeItems)
					{
						ref var rt = ref m_runtimeItems[runItemIndex];
						if (rt.Releases != null)
						{
							foreach (var idx in rt.Releases)
							{
								int result = Interlocked.Decrement(ref m_runtimeItems[idx].RemainingRequiredReleased);
								if (result == 0 && nextRuntimeItem == -1)
								{
									// yay; proceed to this item immediately
									nextRuntimeItem = idx;
									m_runtimeItems[nextRuntimeItem].Started = true;
								}
							}
						}
					}
					runItemIndex = nextRuntimeItem;
				}

				// nothing to immediately continue to; lets loop back and look for other items
				continue;
			}
		}
	}
}
