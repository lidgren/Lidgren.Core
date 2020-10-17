using System;
using System.Diagnostics;

namespace Lidgren.Core
{
	/// <summary>
	/// Heap based priority queue
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

			while (idx > 0)
			{
				int parentIndex = (idx - 1) / 2;
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
			var root = m_entries[cnt];
			m_count = cnt;

			int i = 0;
			while (i * 2 + 1 < cnt)
			{
				int a = i * 2 + 1;
				int b = i * 2 + 2;
				int c = b < cnt && m_entries[b].Prio.CompareTo(m_entries[a].Prio) < 0 ? b : a;

				ref readonly var cmp = ref m_entries[c];
				if (cmp.Prio.CompareTo(root.Prio) >= 0)
					break;
				m_entries[i] = cmp;
				i = c;
			}

			if (cnt > 0)
				m_entries[i] = root;
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
