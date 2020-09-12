using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Special case SymmetrixMatrix of bool where each boolean is stored as a single bit
	/// </summary>
	public class SymmetricMatrixBool
	{
		private ulong[] m_data;

		public SymmetricMatrixBool(int size)
		{
			var len = ((size * size) + size) / 2;
			int units = 1 + ((len - 1) / 64);
			m_data = new ulong[units];
		}

		public void Clear()
		{
			m_data.AsSpan().Clear();
		}

		public bool this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var index = GetIndex(x, y);
				return (m_data[index >> 6] & (1ul << (index & 63))) != 0ul;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				var index = GetIndex(x, y);
				if (value)
					m_data[index >> 6] |= (1ul << (index & 63));
				else
					m_data[index >> 6] &= ~(1ul << (index & 63));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetIndex(int x, int y)
		{
			if (y > x)
				return (((y * y) + y) / 2) + x;
			else
				return (((x * x) + x) / 2) + y;
		}
	}
}
