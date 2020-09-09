using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public partial class FastList<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				this.Span.Clear();
			m_count = 0;
			m_offset = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int minimumCapacity)
		{
			if (m_buffer.Length < minimumCapacity)
				m_buffer = new T[minimumCapacity];
			else if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				this.Span.Clear();
			m_count = 0;
			m_offset = 0;
		}

		/// <summary>
		/// Removes first instance of item in list; returns true if found and removed
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(in T item)
		{
			var idx = IndexOf(item);
			if (idx == -1)
				return false;
			RemoveAt(idx);
			return true;
		}

		/// <summary>
		/// Removes first instance of item in list; returns true if found and removed; does NOT maintain order of list after removal
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveUnordered(in T item)
		{
			var idx = IndexOf(item);
			if (idx == -1)
				return false;
			RemoveAtUnordered(idx);
			return true;
		}

		/// <summary>
		/// Removes item at index; maintaining list order
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			CoreException.Assert(index >= 0 && index < m_count);

			int offset = m_offset;
			int countAfterRemove = m_count - 1;
			m_count = countAfterRemove;

			if (index == 0)
			{
				if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
					m_buffer[offset] = default(T);
				if (countAfterRemove == 0)
					m_offset = 0; // effectively a clear
				else
					m_offset = offset + 1;
				return;
			}

			int copyLen = countAfterRemove - index;
			if (copyLen > 0)
			{
				var buffer = m_buffer;
				for (int i = 0; i < copyLen; i++)
				{
					int c = offset + index + i;
					buffer[c] = buffer[c + 1];
				}
			}
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				m_buffer[offset + countAfterRemove] = default(T);
		}

		/// <summary>
		/// Removes item at index; not maintaining list order
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAtUnordered(int index)
		{
			CoreException.Assert(index >= 0 && index < m_count);

			int offset = m_offset;
			int countAfterRemove = m_count--;

			if (index == 0)
			{
				if (countAfterRemove == 0)
					m_offset = 0; // effectively a clear
				else
					m_offset = offset + 1;
				return;
			}

			// simply swap last item into place of removed item
			m_buffer[offset + index] = m_buffer[offset + countAfterRemove];

			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				m_buffer[offset + countAfterRemove] = default(T);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveRange(int index, int count)
		{
			int localSpanCount = m_count;

			if (index + count > localSpanCount)
				CoreException.Throw("Bad range");

			if (count == 0)
				return;

			// clear out the data if necessary
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				Span.Clear();
			m_count -= count;

			if (count == localSpanCount)
			{
				// removing entire span
				m_offset = 0;
				return;
			}

			if (index + count == localSpanCount)
			{
				// removing a tailing part of span
				return;
			}

			if (index == 0)
			{
				// removing first part of span
				m_offset += count;
				return;
			}

			// removing a middle section of span
			// contract latter part
			int copyLen = localSpanCount - (index + count);
			if (copyLen > 0)
			{
				var buffer = m_buffer;
				var offset = m_offset + index;
				for (int i = 0; i < copyLen; i++)
				{
					int c = offset + i;
					buffer[c] = buffer[c + count];
				}
			}
		}

		/// <summary>
		/// Returns all items matching predicate; returns number of items removed
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int RemoveAll(Predicate<T> match)
		{
			int offset = m_offset;
			int count = m_count;

			int idx = 0;
			while (idx < count && !match(m_buffer[offset + idx]))
				idx++;

			if (idx >= count)
				return 0;

			int i = idx + 1;
			while (i < count)
			{
				while (i < count && match(m_buffer[offset + i]))
					i++;
				if (i < count)
					m_buffer[offset + idx++] = m_buffer[offset + i++];
			}
			Array.Clear(m_buffer, offset + idx, count - idx);
			int result = count - idx;
			m_count = idx;
			return result;
		}

		/// <summary>
		/// Creates a new FastList with all matching items; or NULL if nothing matches
		/// </summary>
		public FastList<T> Find(Predicate<T> match)
		{
			FastList<T> retval = null;

			int offset = m_offset;
			int count = m_count;

			int idx = 0;
			while (idx < count)
			{
				if (match(m_buffer[offset + idx]) == true)
				{
					if (retval is null)
						retval = new FastList<T>(count - idx);
					retval.Add(m_buffer[offset + idx]);
				}
				idx++;
			}
			return retval;
		}
	}
}
