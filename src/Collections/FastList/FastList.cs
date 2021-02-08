using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// List of T alternative with the following properties
	/// * Underlying array is accessible (remember to respect m_offset and Count property)
	/// * ref return methods
	/// * pop/trypop
	/// * Very little validation; except in DEBUG
	/// * Best iterated over myFastList.ReadOnlySpan
	/// </summary>
	[DebuggerDisplay("FastList<{typeof(T).Name}> {Count}/{Capacity}")]
	public partial class FastList<T>
	{
		private T[] m_buffer = System.Array.Empty<T>();
		private int m_offset;
		private int m_count;

		public int Count
		{
			get { return m_count; }
			set
			{
				CoreException.Assert(value <= m_buffer.Length);
				m_count = value;
				if (m_offset + m_count > m_buffer.Length)
					Compact(); // ensure Span can operate
			}
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get { return m_buffer.Length; }
			[MethodImpl(MethodImplOptions.NoInlining)]
			set
			{
				if (value == 0)
				{
					m_buffer = System.Array.Empty<T>();
					m_offset = 0;
					m_count = 0;
					return;
				}

				if (value == m_buffer.Length)
					return;

				var newItems = new T[value];
				m_count = Math.Min(m_count, value);
				if (m_count > 0)
					ReadOnlySpan.Slice(0, m_count).CopyTo(newItems);
				m_buffer = newItems;
				m_offset = 0;
			}
		}

		// cold path, never inline
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Compact()
		{
			CoreException.Assert(m_offset > 0, "Unnecessary compact!");
			var buffer = m_buffer;
			var off = m_offset;
			var cnt = m_count;
			// TODO: compare to aliased Span.CopyTo
			for (int i = 0; i < cnt; i++)
				buffer[i] = buffer[off + i];
			m_offset = 0;
		}

		/// <summary>
		/// May compact buffer if it's offset
		/// </summary>
		public T[] GetBuffer()
		{
			if (m_offset != 0)
				Compact();
			return m_buffer;
		}

		/// <summary>
		/// Replace backing items array and set count
		/// </summary>
		public void ReplaceBuffer(T[] items, int count)
		{
			m_buffer = items;
			m_offset = 0;
			m_count = count;
		}

		/// <summary>
		/// Get readonly span of items in list
		/// </summary>
		public ReadOnlySpan<T> ReadOnlySpan => new ReadOnlySpan<T>(m_buffer, m_offset, m_count);

		/// <summary>
		/// Get span of items in list
		/// </summary>
		public Span<T> Span => new Span<T>(m_buffer, m_offset, m_count);

		/// <summary>
		/// Get memory span of items in list
		/// </summary>
		public Memory<T> Memory => new Memory<T>(m_buffer, m_offset, m_count);

		public FastList(int initialCapacity = 0)
		{
			if (initialCapacity == 0)
				m_buffer = System.Array.Empty<T>();
			else
				m_buffer = new T[initialCapacity];
			m_count = 0;
			m_offset = 0;
		}

		public FastList(ReadOnlySpan<T> copyItems)
		{
			m_buffer = copyItems.ToArray();
			m_count = copyItems.Length;
			m_offset = 0;
		}

		private FastList()
		{
		}

		public static FastList<T> CreateFromBackingArray(T[] arr, int initialCount)
		{
			var retval = new FastList<T>();
			retval.m_count = initialCount;
			retval.m_buffer = arr;
			retval.m_offset = 0;
			return retval;
		}

		public static implicit operator ReadOnlySpan<T>(FastList<T> list)
		{
			return list.ReadOnlySpan;
		}
	}
}
