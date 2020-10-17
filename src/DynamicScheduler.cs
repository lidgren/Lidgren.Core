using System;
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
		private enum ItemStatus
		{
			Waiting,
			Running,
			Done
		}

		private ScheduleOrder[] m_order;
		private TItem[] m_items;
		private ItemStatus[] m_itemStatus;
		private TArg m_runArgument;

		public DynamicScheduler(ReadOnlySpan<TItem> scheduleItems)
		{
			var items = scheduleItems.ToArray();
			m_itemStatus = new ItemStatus[items.Length];
			m_order = new ScheduleOrder[items.Length * items.Length];

			Array.Sort(items, (a, b) =>
			{
				int aval = (int)a.Hint;
				int bval = (int)b.Hint;
				if (aval > bval)
					return 1;
				if (aval < bval)
					return -1;
				return 0;
			});

			// create matrices
			for (int a = 0; a < items.Length; a++)
			{
				for (int b = 0; b < items.Length; b++)
				{
					if (a == b)
						continue;

					var order = items[a].ScheduleAgainst(items[b]);
					var antiorder = items[b].ScheduleAgainst(items[a]);
					var resolvedOrder = ResolveOrder(order, antiorder, out bool conflict);
					if (conflict)
						CoreException.Throw("Clashing schedule; {items[a].Item.ToString()} and {items[b].Item.ToString()} both wants to run " + order);
					m_order[a * items.Length + b] = resolvedOrder;
				}
			}

			m_items = items;
		}

		private Action<object> m_onCompleted;
		private object m_onCompletedArgument;
		private int m_completedItemsCount;

		public void Execute(TArg argument, Action<object> completed, object completionArgument)
		{
			m_onCompleted = completed;
			m_onCompletedArgument = completionArgument;
			m_completedItemsCount = 0;

			m_itemStatus.AsSpan().Clear();
			m_runArgument = argument;
			FireReadyJobs();
		}

		private void FireReadyJobs()
		{
			var items = m_items;
			var itemStatus = m_itemStatus;

			lock (m_itemStatus)
			{
				// loop over all items
				for (int a = 0; a < itemStatus.Length; a++)
				{
					// only consider items that are waiting
					var status = itemStatus[a];
					if (status != ItemStatus.Waiting)
						continue;

					int aoff = a * items.Length;
					bool canARun = true;

					// compare to all other items; can this be run?
					for (int b = 0; b < itemStatus.Length; b++)
					{
						if (a == b)
							continue;
						var bstatus = itemStatus[b];
						var order = m_order[aoff + b];

						if (order == ScheduleOrder.AnyOrderConcurrent)
							continue;
						if (order == ScheduleOrder.RunAfter) // A has to run after B
						{
							if (bstatus != ItemStatus.Done)
							{
								canARun = false;
								break;
							}
							continue;
						}
						if (order == ScheduleOrder.RunBefore) // A has to run before B
							continue;
						if (order == ScheduleOrder.AnyOrderNotConcurrent)
						{
							if (bstatus == ItemStatus.Running)
							{
								canARun = false;
								break;
							}
						}
					}

					if (canARun)
					{
						// YES! Run this item
						var item = items[a];
						itemStatus[a] = ItemStatus.Running;
						JobService.Enqueue(item.Name, RunItem, item);
					}
				}
			}
		}

		private void RunItem(object ob)
		{
			// yay; perform work
			var item = (IScheduleItem<TItem, TArg>)ob;
			item.Execute(m_runArgument);
			var completed = Interlocked.Increment(ref m_completedItemsCount);

			var idx = Array.IndexOf(m_items, item);

			lock (m_itemStatus)
			{
				// work completed might mean some other can now run
				CoreException.Assert(m_itemStatus[idx] == ItemStatus.Running);
				m_itemStatus[idx] = ItemStatus.Done;
			}

			if (completed == m_items.Length)
			{
				// ALL DONE!
				m_onCompleted(m_onCompletedArgument);
				return;
			}

			FireReadyJobs();
		}

		private ScheduleOrder ResolveOrder(ScheduleOrder order, ScheduleOrder antiorder, out bool conflict)
		{
			//                                   A:RunBefore       A:AnyOrderConcurrent    A:AnyOrderNotConcurrent   A:RunAfter
			// 
			// B:RunBefore                       CONFLICT         RunAfter                RunAfter                   RunAfter
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
