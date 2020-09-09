using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public partial class FastList<T>
	{
		//
		// foreach support; however, iterating over myList.ReadOnlySpan instead is recommended
		//

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FastListIterator GetEnumerator()
		{
			FastListIterator it;
			it.Span = new Span<T>(m_buffer, m_offset, m_count);
			it.Index = 0;
			return it;
		}

		public ref struct FastListIterator
		{
			public ReadOnlySpan<T> Span;
			public int Index;

			public ref readonly T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get
				{
					return ref Span[Index - 1];
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				Index++;
				return Index <= Span.Length;
			}
		}
	}
}
