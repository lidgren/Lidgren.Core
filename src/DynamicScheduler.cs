using System;
using System.Diagnostics;
using System.Threading;

namespace Lidgren.Core
{
	public enum ScheduleOrder
	{
		RunBefore = 0,
		AnyOrderConcurrent = 1,
		AnyOrderNotConcurrent = 2,
		RunAfter = 3,
	}

	public enum ScheduleHint
	{
		VeryEarly,
		Early,
		Default,
		Late,
		VeryLate,
	}

	public interface IScheduleItem<TItem, TArg>
	{
		string Name { get; }
		ScheduleHint Hint { get; }
		ScheduleOrder ScheduleAgainst(TItem other);
		void Execute(TArg argument);
	}

	public class DynamicScheduler<TItem, TArg> where TItem : IScheduleItem<TItem, TArg> where TArg : class
	{
		[DebuggerDisplay("{Item.Name} {NodsReceived}/{NodsRequired} nods")]
		private class ItemInfo : IDynamicOrdered
		{
			public int Index;

			public TItem Item;

			public int NodsRequired; // how many nods needed before we can run this job?
			public int NodsReceived; // how many nods has happened so far?

			public int EmitNodsCount; // number of nods this item emits
			public int EmitNodsIndex; // where in m_nodEmits to start looking

			public DynamicOrder OrderAgainst(object other)
			{
				var ot = (ItemInfo)other;
				var aOrder = Item.ScheduleAgainst(ot.Item);
				var bOrder = ot.Item.ScheduleAgainst(Item);
				aOrder = ScheduleOrderUtil.ResolveOrder(aOrder, bOrder, out var conflict);
				if (conflict)
					CoreException.Throw("Bad dynamic order");

				if (aOrder == ScheduleOrder.RunBefore)
					return DynamicOrder.RequireABeforeB;
				if (aOrder == ScheduleOrder.RunAfter)
					return DynamicOrder.RequireBBeforeA;

				// ok, soft order by hint
				var diff = (int)Item.Hint - (int)ot.Item.Hint;
				if (diff > 0)
					return DynamicOrder.PreferBBeforeA;
				if (diff < 0)
					return DynamicOrder.PreferABeforeB;
				return DynamicOrder.AnyOrder;
			}
		}

		private ItemInfo[] m_items;
		private TArg m_runArgument;
		private int[] m_nodEmits;
		private Action<object> m_executeItemAction;
		private SymmetricMatrixBool m_concurrencyPreventionMatrix;
		private int m_unprocessedItems;
		private Action<object> m_completedAction;
		private object m_completedArgument;

		private FastList<ItemInfo> m_running;

		public DynamicScheduler(ReadOnlySpan<TItem> scheduleItems)
		{
			m_executeItemAction = ExecuteScheduledItem;

			var items = new ItemInfo[scheduleItems.Length];
			for (int i = 0; i < items.Length; i++)
				items[i] = new ItemInfo() { Item = scheduleItems[i] };

			// sort dynamically
			var ok = DynamicOrdering.Perform<ItemInfo>(items, out _);
			if (!ok)
				CoreException.Throw("Failed to sort dynamically");

			m_concurrencyPreventionMatrix = new SymmetricMatrixBool(scheduleItems.Length);
			m_running = new FastList<ItemInfo>(16);

			// set up items (determine nods, concurrency etc)
			var nods = new FastList<int>(32);
			for (int aIdx = 0; aIdx < items.Length; aIdx++)
			{
				var a = items[aIdx];
				a.Index = aIdx; // set up index
				ref var aInfo = ref items[aIdx];
				aInfo.EmitNodsIndex = nods.Count;
				for (int bIdx = 0; bIdx < items.Length; bIdx++)
				{
					if (aIdx == bIdx)
						continue;

					// determine is aIdx nods to bIdx
					var b = items[bIdx];
					var aOrder = a.Item.ScheduleAgainst(b.Item);
					var bOrder = b.Item.ScheduleAgainst(a.Item);
					aOrder = ScheduleOrderUtil.ResolveOrder(aOrder, bOrder, out var conflict);
					if (aOrder == ScheduleOrder.RunBefore)
					{
						ref var bInfo = ref items[bIdx];
						bInfo.NodsRequired++;
						nods.Add(bIdx);
					}

					if (aOrder != ScheduleOrder.AnyOrderConcurrent)
						m_concurrencyPreventionMatrix[aIdx, bIdx] = true;
				}
				aInfo.EmitNodsCount = nods.Count - aInfo.EmitNodsIndex;
			}
			m_nodEmits = nods.ToArray();

			//for (int aIdx = 0; aIdx < items.Length; aIdx++)
			//{
			//	var a = items[aIdx];
			//	System.Diagnostics.Trace.WriteLine("Item: " + a.Item.Name);
			//	ref var aInfo = ref items[aIdx];
			//	if (aInfo.NodsRequired > 0)
			//		System.Diagnostics.Trace.WriteLine(a.Item.Name + " requires " + aInfo.NodsRequired + " nods");
			//	for (int i = 0; i < aInfo.EmitNodsCount; i++)
			//		System.Diagnostics.Trace.WriteLine(a.Item.Name + " nods to " + items[nods[aInfo.EmitNodsIndex + i]].Item.Name);
			//}

			m_items = items;
		}

		public void Execute(TArg argument, Action<object> completed, object completionArgument)
		{
			using var _ = new Timing("SetupScheduler");

			CoreException.Assert(m_unprocessedItems == 0);
			CoreException.Assert(m_completedAction == null);
			//System.Diagnostics.Trace.WriteLine("Starting scheduler");

			m_runArgument = argument;
			var items = m_items.AsSpan();

			m_completedAction = completed;
			m_completedArgument = completionArgument;
			m_unprocessedItems = items.Length;

			// reset all items
			CoreException.Assert(m_running.Count == 0);
			for (int i = 0; i < items.Length; i++)
			{
				ref var item = ref items[i];
				item.NodsReceived = 0;
			}

			// kick off all jobs with no dependencies
			lock (m_serial)
			{
				for (int i = 0; i < items.Length; i++)
				{
					ref var item = ref items[i];
					if (item.NodsRequired == 0)
					{
						if (CanRunNow(item))
						{
							lock (m_running)
								m_running.Add(item);
							JobService.Enqueue(item.Item.Name, ExecuteScheduledItem, item);
						}
						else
						{
							m_concurrentBlockedItems.Add(item);
						}
					}
				}
			}
		}

		private object m_serial = new object();
		private FastList<ItemInfo> m_concurrentBlockedItems = new FastList<ItemInfo>(16);

		private void ExecuteScheduledItem(object ob)
		{
			var item = (ItemInfo)ob;

			//System.Diagnostics.Trace.WriteLine("Executing " + item.Item.Name);

			// go!
			item.Item.Execute(m_runArgument);

			using (new Timing("scheduling"))
			{
				// set done state
				lock (m_running)
				{
					bool removed = m_running.Remove(item);
					CoreException.Assert(removed);
				}

				var unprocessedLeft = Interlocked.Decrement(ref m_unprocessedItems);
				CoreException.Assert(unprocessedLeft >= 0);

				if (unprocessedLeft == 0)
				{
					var ca = m_completedAction;
					if (ca != null)
					{
						var arg = m_completedArgument;
						m_completedAction = null;
						m_completedArgument = null;

						// schedule on jobservice instead of running directly; mostly for prettier trace
						JobService.Enqueue(ca, arg);
					}
					//System.Diagnostics.Trace.WriteLine("Schedule done!");
					return;
				}

				var items = m_items;

				Span<int> nodReleasedItems = stackalloc int[32];
				int numNodReleasedItems = 0;

				// emit nods
				for (int n = 0; n < item.EmitNodsCount; n++)
				{
					var noddedIdx = m_nodEmits[item.EmitNodsIndex + n];
					ref var noddedItem = ref items[noddedIdx];

					var nodSum = Interlocked.Increment(ref noddedItem.NodsReceived);

					//System.Diagnostics.Trace.WriteLine(item.Item.Name + " nods to " + noddedItem.Item.Name);

					CoreException.Assert(noddedItem.NodsRequired > 0);
					CoreException.Assert(nodSum <= noddedItem.NodsRequired);
					if (nodSum == noddedItem.NodsRequired)
					{
						CoreException.Assert(m_concurrentBlockedItems.Contains(noddedItem) == false);

						// store for checking later; under lock
						nodReleasedItems[numNodReleasedItems++] = noddedItem.Index;
					}
				}

				// check readyJobs if out finishing has enabled a ready job to run
				using (new LockTiming("chk", m_serial))
				{
					var concBlocked = m_concurrentBlockedItems;

					// first check all previously blocked items
					for (int i = 0; i < concBlocked.Count; i++)
					{
						var testItem = concBlocked[i];
						bool canRunNow = CanRunNow(testItem);
						if (canRunNow)
						{
							// no concurrency issue - run it!
							lock (m_running)
								m_running.Add(testItem);
							JobService.Enqueue(testItem.Item.Name, ExecuteScheduledItem, testItem);
							concBlocked.RemoveAt(i);
							i--;
						}
					}

					// then check all newly nodded items
					int numkicked = 0;
					for (int i = 0; i < numNodReleasedItems; i++)
					{
						var testItemIndex = nodReleasedItems[i];
						var testItem = items[testItemIndex];
						bool canRunNow = CanRunNow(testItem);
						if (canRunNow)
						{
							// no concurrency issue - run it!
							lock (m_running)
								m_running.Add(testItem);
							numkicked++;
							JobService.Enqueue(testItem.Item.Name, ExecuteScheduledItem, testItem);
						}
						else
						{
							concBlocked.Add(testItem);
						}
					}
				}
			}
		}

		// must be run under m_serial lock
		private bool CanRunNow(ItemInfo item)
		{
#if DEBUG
			CoreException.Assert(Monitor.IsEntered(m_serial));
#endif
			// check all running items for concurrency prohibitions
			var running = m_running;
			SymmetricMatrixBool concMatrix = m_concurrencyPreventionMatrix;
			lock (running)
			{
				for (int i = 0; i < running.Count; i++)
				{
					ref readonly var other = ref running[i];
					CoreException.Assert(other != item); // should be prevented by state not being Running

					// check for concurrency issue
					if (concMatrix[other.Index, item.Index] == true)
						return false;
				}
			}
			return true;
		}
	}

	public static class ScheduleOrderUtil
	{
		public static ScheduleOrder ResolveOrder(ScheduleOrder order, ScheduleOrder antiorder, out bool conflict)
		{
			//                                   A:RunBefore       A:AnyOrderConcurrent    A:AnyOrderNotConcurrent   A:RunAfter
			// B:RunBefore                       CONFLICT          RunAfter                RunAfter                   RunAfter
			// B:AnyOrderConcurrent              RunBefore         AnyOrderConcurrent      AnyOrderNotConcurrent     RunAfter
			// B:AnyOrderNotConcurrent           RunBefore         AnyOrderNotConcurrent   AnyOrderNotConcurrent     RunAfter
			// B:RunAfter                        RunBefore         RunBefore               RunBefore                 CONFLICT
			//
			conflict = false;
			switch (order)
			{
				case ScheduleOrder.AnyOrderConcurrent:
					if (antiorder == ScheduleOrder.RunAfter)
						return ScheduleOrder.RunBefore;
					if (antiorder == ScheduleOrder.RunBefore)
						return ScheduleOrder.RunAfter;
					return antiorder;

				case ScheduleOrder.AnyOrderNotConcurrent:
					if (antiorder == ScheduleOrder.RunAfter)
						return ScheduleOrder.RunBefore;
					if (antiorder == ScheduleOrder.RunBefore)
						return ScheduleOrder.RunAfter;
					return ScheduleOrder.AnyOrderNotConcurrent;

				case ScheduleOrder.RunBefore:
				case ScheduleOrder.RunAfter:
				default:
					if (antiorder == order)
						conflict = true;
					return order;
			}
		}
	}
}
