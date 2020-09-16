using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public partial class FastList<T>
	{
		// cold path, never inline
		[MethodImpl(MethodImplOptions.NoInlining)]
		private T[] Grow(int minAddCount)
		{
			int newSize = Math.Max(m_count + minAddCount, m_buffer.Length * 2);
			var old = this.ReadOnlySpan;
			var newBuffer = new T[newSize];
			old.CopyTo(newBuffer);
			m_offset = 0;
			m_buffer = newBuffer;
			return newBuffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int toAddItemsCount)
		{
			if (m_count + toAddItemsCount > m_buffer.Length)
				Grow(toAddItemsCount);
		}

		// cold path, never inline
		[MethodImpl(MethodImplOptions.NoInlining)]
		private T[] Grow()
		{
			var oldLen = m_buffer.Length;
			var newLength = oldLen == 0 ? 4 : oldLen * 2;
			var newBuffer = new T[newLength];

			var old = this.ReadOnlySpan;
			old.CopyTo(newBuffer);

			m_buffer = newBuffer;
			m_offset = 0;
			return newBuffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(in T item)
		{
			ref var loc = ref AddUninitialized();
			loc = item;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Add()
		{
			ref var loc = ref AddUninitialized();
			loc = default(T);
			return ref loc;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T AddUninitialized()
		{
			int count = m_count;
			var buffer = m_buffer;
			if (count == buffer.Length)
				buffer = Grow();
			int idx = m_offset + count;
			if (idx >= buffer.Length)
			{
				Compact();
				idx = count;
			}
			m_count = count + 1;
			return ref buffer[idx];
		}

		/// <summary>
		/// Reserves items at end of list and returns span
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> AddRange(int numItems)
		{
			var span = AddRangeUninitialized(numItems);
			span.Clear();
			return span;
		}

		/// <summary>
		/// Reserves items at end of list and returns span; may contain uninitialized values
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> AddRangeUninitialized(int numItems)
		{
			var buffer = m_buffer;
			int cnt = m_count;
			if (cnt + numItems >= buffer.Length)
				buffer = Grow(numItems);
			if (m_offset + cnt + numItems > buffer.Length)
				Compact();
			var retval = new Span<T>(buffer, m_offset + cnt, numItems);
			m_count = cnt + numItems;
			return retval;
		}

		/// <summary>
		/// Adds items to end of list
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange(ReadOnlySpan<T> addItems)
		{
			var span = AddRangeUninitialized(addItems.Length);
			addItems.CopyTo(span);
		}

		/// <summary>
		/// Adds items to end of list; using IEnumerable (much slower than adding spans)
		/// </summary>
		public void AddRangeSlow(IEnumerable<T> addItems)
		{
			foreach (var item in addItems)
				Add(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(int index, in T value)
		{
			ref var destination = ref InsertUninitialized(index);
			destination = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T InsertUninitialized(int index)
		{
			int count = m_count;
			CoreException.Assert(index <= count);
			var buffer = m_buffer;
			if (count >= buffer.Length)
				buffer = Grow();
			var offset = m_offset;
			if (offset > 0)
			{
				if (index == 0)
				{
					// yay
					m_count = count + 1;
					offset = offset - 1;
					m_offset = offset;
					return ref buffer[offset];
				}

				if (offset + count == buffer.Length)
				{
					// need to compact
					Compact();
					offset = 0;
				}
			}

			int bufferIndex = offset + index;

			int copyLen = count - index;
			if (copyLen > 0)
			{
				// push forward
				var cache = buffer[bufferIndex];
				for (int i = 0; i < copyLen; i++)
				{
					// swap tmp and destination
					int dstBufIdx = bufferIndex + i + 1;
					var tmp = buffer[dstBufIdx];
					buffer[dstBufIdx] = cache;
					cache = tmp;
				}
			}
			m_count = count + 1;
			return ref m_buffer[bufferIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Insert(int index)
		{
			ref T destination = ref InsertUninitialized(index);
			destination = default;
			return ref destination;
		}

		public void InsertRange(int index, ReadOnlySpan<T> items)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds 'numItems' copies of 'item' to list
		/// </summary>
		public void Fill(int numItems, in T item)
		{
			var span = AddRangeUninitialized(numItems);
			span.Fill(item);
		}
	}
}
