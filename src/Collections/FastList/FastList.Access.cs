using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lidgren.Core
{
	public partial class FastList<T>
	{
		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref m_buffer[m_offset + index];
		}

		public ref T this[uint index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref m_buffer[m_offset + index];
		}

		/// <summary>
		/// Creates a new array; use this.Items for in-place access
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
		{
			if (m_count == 0)
				return System.Array.Empty<T>();
			return ReadOnlySpan.ToArray();
		}

		/// <summary>
		/// Returns and removes the last item in the list; returns false if list contains no items
		/// </summary>
		public bool TryPop(out T result)
		{
			var cnt = m_count;
			if (cnt == 0)
			{
				result = default;
				return false;
			}

			var index = m_offset + cnt - 1;
			result = m_buffer[index];
			m_buffer[index] = default;
			cnt--;
			if (cnt == 0)
				m_offset = 0;
			m_count = cnt;
			return true;
		}

		/// <summary>
		/// Returns and removes the last item in the list; throws if empty
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Pop()
		{
			var index = m_offset + m_count - 1;
			var result = m_buffer[index];
			m_buffer[index] = default;
			m_count--;
			return result;
		}

		/// <summary>
		/// If count > 0; removes first element in list, putting it in 'item' and returns true
		/// </summary>
		public bool TryDequeue(out T item)
		{
			if (m_count == 0)
			{
				item = default;
				return false;
			}
			item = m_buffer[m_offset];
			m_offset++;
			m_count--;
			if (m_count == 0)
				m_offset = 0;
			return true;
		}

		/// <summary>
		/// Copy items of list to destination; throws if destination too small
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Span<T> destination)
		{
			ReadOnlySpan.CopyTo(destination);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item)
		{
			return Array.IndexOf(m_buffer, item, m_offset, m_count) != -1;
			//return ReadOnlySpan.Contains(item);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(in T item)
		{
			//if (item is IEquatable<T>)
			//	return ReadOnlySpan.IndexOf(item);
			//else
			return Array.IndexOf(m_buffer, item, m_offset, m_count) - m_offset;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LastIndexOf(in T item)
		{
			int off = m_offset;
			int cnt = m_count;

			return Array.LastIndexOf(m_buffer, item, m_offset + m_count - 1, m_count) - m_offset;
			//return ReadOnlySpan.LastIndexOf(item);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEmpty() { return m_count == 0; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse()
		{
			if (m_count != 0)
				Array.Reverse<T>(m_buffer, m_offset, m_count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort()
		{
#if NET5_0
			this.Span.Sort();
#else
			Array.Sort<T>(m_buffer, m_offset, m_count);
#endif
		}

#if NET5_0
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(Comparison<T> comparison)
		{
			this.Span.Sort(comparison);
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(IComparer<T> comparer)
		{
#if NET5_0
			this.Span.Sort(comparer);
#else
			Array.Sort<T>(m_buffer, m_offset, m_count, comparer);
#endif
		}
	}
}
