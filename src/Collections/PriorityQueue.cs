using System;
using System.Diagnostics;

namespace Lidgren.Core
{
	/// <summary>
	/// Priority queue using a min binary heap
	/// </summary>
	public class PriorityQueue<TPriority, TItem> where TPriority : struct, IComparable<TPriority>
	{
		[DebuggerDisplay("{Prio} {Item}")]
		private struct Entry
		{
			public TPriority Prio;
			public TItem Item;
		}
		private Entry[] m_entries;

		private int m_count;
		public int Count => m_count;

		public PriorityQueue(int initialCapacity)
		{
			m_entries = new Entry[initialCapacity];
		}

		//private bool Validate()
		//{
		//	var ok = PeekPriority(out var nextPrio);
		//	if (ok)
		//	{
		//		for (int i = 0; i < m_count; i++)
		//		{
		//			ref var entry = ref m_entries[i];
		//			if (entry.Prio.CompareTo(nextPrio) < 0)
		//				return false;
		//			// CoreException.Throw("Queue corrupt");
		//
		//			// check heapness
		//			var left = (2 * i) + 1;
		//			if (left < m_count)
		//			{
		//				// verify left child is larger or equal
		//				if (m_entries[left].Prio.CompareTo(entry.Prio) < 0)
		//					return false;
		//			}
		//
		//			var right = (2 * i) + 2;
		//			if (right < m_count)
		//			{
		//				// verify left child is larger or equal
		//				if (m_entries[right].Prio.CompareTo(entry.Prio) < 0)
		//					return false;
		//			}
		//		}
		//	}
		//	return true;
		//}

		/// <summary>
		/// Enqueue an item with a certain priority
		/// </summary>
		public void Enqueue(TPriority prio, TItem item)
		{
			var cnt = m_count;
			if (cnt >= m_entries.Length)
				Array.Resize(ref m_entries, cnt * 2);

			Entry entry;
			entry.Prio = prio;
			entry.Item = item;

			m_entries[cnt] = entry;
			int idx = cnt;
			cnt++;
			m_count = cnt;

			// bubble up
			while (idx > 0)
			{
				int parentIndex = (idx - 1) >> 1;
				ref readonly var parent = ref m_entries[parentIndex];
				if (parent.Prio.CompareTo(prio) <= 0)
					break;
				m_entries[idx] = parent;
				idx = parentIndex;
			}

			if (cnt > 0)
				m_entries[idx] = entry;
		}

		/// <summary>
		/// Peek what the next TryDequeue would result in
		/// </summary>
		public bool Peek(out TPriority nextDequeuedPrio, out TItem nextDequeuedItem)
		{
			if (m_count == 0)
			{
				nextDequeuedPrio = default;
				nextDequeuedItem = default;
				return false;
			}

			ref readonly var peek = ref m_entries[0];
			nextDequeuedPrio = peek.Prio;
			nextDequeuedItem = peek.Item;
			return true;
		}

		/// <summary>
		/// Peek what the next TryDequeue would result in
		/// </summary>
		public bool PeekPriority(out TPriority nextDequeuedPrio)
		{
			if (m_count == 0)
			{
				nextDequeuedPrio = default;
				return false;
			}

			ref readonly var peek = ref m_entries[0];
			nextDequeuedPrio = peek.Prio;
			return true;
		}

		/// <summary>
		/// Try dequeue the item with the lowest priority value; returns false if queue is empty or if the lowest priority is higher than threshold
		/// </summary>
		public bool TryDequeue(out TItem retval, TPriority threshold)
		{
			if (m_count == 0)
			{
				retval = default;
				return false;
			}

			ref readonly var peek = ref m_entries[0];
			if (peek.Prio.CompareTo(threshold) > 0)
			{
				retval = default;
				return false;
			}

			return TryDequeue(out retval);
		}

		/// <summary>
		/// Dequeues the item with the lowest priority value; throws exception if queue is empty
		/// </summary>
		public TItem Dequeue()
		{
			if (m_count == 0)
				CoreException.Throw("Queue is empty");
			var ok = TryDequeue(out var retval);
			return retval;
		}

		/// <summary>
		/// Try dequeue the item with the lowest priority value; returns false if queue is empty
		/// </summary>
		public bool TryDequeue(out TItem retval)
		{
			if (m_count == 0)
			{
				retval = default;
				return false;
			}

			retval = m_entries[0].Item;

			int cnt = m_count - 1;
			var replacement = m_entries[cnt];
			m_count = cnt;
			if (cnt == 0)
				return true;

			// pull up
			int index = 0;
			for (; ; )
			{
				var leftChildIndex = index * 2 + 1;
				if (leftChildIndex >= cnt)
					break;
				int rightChildIndex = index * 2 + 2;
				int swapIndex = rightChildIndex < cnt && m_entries[rightChildIndex].Prio.CompareTo(m_entries[leftChildIndex].Prio) < 0 ? rightChildIndex : leftChildIndex;

				ref readonly var cmp = ref m_entries[swapIndex];
				if (cmp.Prio.CompareTo(replacement.Prio) >= 0)
					break;

				m_entries[index] = cmp;
				index = swapIndex;
			}

			m_entries[index] = replacement;
			return true;
		}

		private int GetItemIndex(in TItem item)
		{
			int count = m_count;
			for (int i = 0; i < count; i++)
			{
				ref var entry = ref m_entries[i];
				if (item is null)
				{
					if (entry.Item is null)
						return i;
					continue;
				}
				if (item.Equals(entry.Item))
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Remove first item found from queue
		/// </summary>
		public bool Remove(in TItem item)
		{
			var index = GetItemIndex(item);
			if (index == -1)
				return false;

			int cnt = m_count - 1;
			m_count = cnt;
			if (index == cnt)
				return true; // just drop it off the end

			if (cnt == 1)
			{
				// we removed top item
				CoreException.Assert(index == 0);
				m_entries[0] = m_entries[1];
				return true;
			}

			var replacement = m_entries[cnt];

			while (index > 0)
			{
				// check parent for upwards travelling
				var parentIndex = (index - 1) / 2;
				ref readonly var parentEntry = ref m_entries[parentIndex];
				if (replacement.Prio.CompareTo(parentEntry.Prio) < 0)
				{
					// need to swap up
					m_entries[index] = parentEntry;
					index = parentIndex;
					continue; // loop back; may need to travel higher yet
				}
				break;
			}

			// ok, check children
			for (; ; )
			{
				var leftChildIndex = index * 2 + 1;
				if (leftChildIndex >= cnt)
					break;
				int rightChildIndex = index * 2 + 2;

				ref readonly var leftChildEntry = ref m_entries[leftChildIndex];
				if (rightChildIndex < cnt)
				{
					ref readonly var rightChildEntry = ref m_entries[rightChildIndex];
					if (leftChildEntry.Prio.CompareTo(rightChildEntry.Prio) >= 0)
					{
						// right child is smaller
						if (rightChildEntry.Prio.CompareTo(replacement.Prio) < 0)
						{
							m_entries[index] = rightChildEntry;
							index = rightChildIndex;
							continue;
						}
						break;
					}
				}

				// left child is smaller
				if (leftChildEntry.Prio.CompareTo(replacement.Prio) < 0)
				{
					m_entries[index] = leftChildEntry;
					index = leftChildIndex;
					continue;
				}
				break;
			}

			m_entries[index] = replacement;
			return true;
		}

		/// <summary>
		/// Remove all items from the queue
		/// </summary>
		public void Clear()
		{
			m_count = 0;
			m_entries.AsSpan().Clear();
		}
	}
}
