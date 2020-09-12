using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Symmetrix matrix where [3, 5] references the same data as [5, 3] - using ~half the storage of a full matrix
	/// </summary>
	public class SymmetricMatrix<T>
	{
		public readonly T[] Data;

		public SymmetricMatrix(int size)
		{
			Data = new T[GetLength(size)];
		}

		public void Clear()
		{
			Data.AsSpan().Clear();
		}

		public T this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return Data[GetIndex(x, y)];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				Data[GetIndex(x, y)] = value;
			}
		}

		/// <summary>
		/// Gets the necessary array length to store a symmetric matrix of size * size
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetLength(int size)
		{
			return ((size * size) + size) / 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIndex(int x, int y)
		{
			if (y > x)
				return (((y * y) + y) / 2) + x;
			else
				return (((x * x) + x) / 2) + y;
		}
	}
}
