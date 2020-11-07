using System;
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
		private T[] m_items = System.Array.Empty<T>();

		private int m_count;
		public int Count => m_count;

		public ReadOnlySpan<T> Span => new Span<T>(m_items, 0, m_count);
		public ReadOnlySpan<T> ReadOnlySpan => new ReadOnlySpan<T>(m_items, 0, m_count);

		public AddList()
		{
		}

		public AddList(int capacity)
		{
			m_items = new T[capacity];
		}

		// cold path, never inline
		[MethodImpl(MethodImplOptions.NoInlining)]
		private T[] Grow(int minAdd)
		{
			var oldLen = m_items.Length;
			var newLength = oldLen == 0 ? 4 : oldLen * 2;
			var min = m_count + minAdd;
			if (newLength < min)
				newLength = min;
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
				buffer = Grow(1);

			m_count = count + 1;
			return ref buffer[count];
		}

		public Span<T> AddRange(int numItems)
		{
			int rem = m_items.Length - m_count;
			if (rem < numItems)
				Grow(numItems);
			var span = m_items.AsSpan(m_count, numItems);
			m_count += numItems;
			return span;
		}

		public void AddRange(ReadOnlySpan<T> items)
		{
			var into = AddRange(items.Length);
			items.CopyTo(into);
		}

		public T[] ToArray()
		{
			var span = ReadOnlySpan;
			var retval = new T[span.Length];
			span.CopyTo(retval);
			return retval;
		}
	}
}
