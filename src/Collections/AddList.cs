using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Add only list
	/// </summary>
	[DebuggerDisplay("AddList<{typeof(T).Name}> {Count}/{Capacity}")]
	public class AddList<T>
	{
		private static readonly T[] s_emptyArray = new T[] { };

		private T[] m_items = s_emptyArray;

		private int m_count;
		public int Count => m_count;

		public ReadOnlySpan<T> Span => new Span<T>(m_items, 0, m_count);
		public ReadOnlySpan<T> ReadOnlySpan => new ReadOnlySpan<T>(m_items, 0, m_count);

		// cold path, never inline
		[MethodImpl(MethodImplOptions.NoInlining)]
		private T[] Grow()
		{
			var oldLen = m_items.Length;
			var newLength = oldLen == 0 ? 4 : oldLen * 2;
			var newBuffer = new T[newLength];

			var old = this.ReadOnlySpan;
			old.CopyTo(newBuffer);

			m_items = newBuffer;
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
			var buffer = m_items;

			if (count == buffer.Length)
				buffer = Grow();

			m_count = count + 1;
			return ref buffer[count];
		}
	}
}
