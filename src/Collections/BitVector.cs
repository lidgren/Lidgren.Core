using System;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Arbitrarily long bit vector
	/// </summary>
	public sealed class BitVector
	{
		private const int c_bitsPerUnit = 64;
		private const int c_bitsMask = 63;
		private const int c_unitBitCount = 6;

		private ulong[] m_data;
		public ulong[] Data => m_data;

		public BitVector(int minCapacity = c_bitsPerUnit)
		{
			int numUnits = UnitsToHoldBits(minCapacity);
			m_data = new ulong[numUnits];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int UnitsToHoldBits(int bits)
		{
			return ((bits - 1) / c_bitsPerUnit) + 1;
		}

		public bool this[int index]
		{
			get { return Get(index); }
			set { Set(index, value); }
		}

		/// <summary>
		/// Clear all values to false
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			m_data.AsSpan().Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAll(bool value)
		{
			if (value)
				m_data.AsSpan().Fill(~0ul);
			else
				m_data.AsSpan().Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get(int index)
		{
			int dataIndex = index >> c_unitBitCount;
			ulong value = m_data[dataIndex];
			int bitIndex = index & c_bitsMask;
			return ((value >> bitIndex) & 1ul) != 0ul;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Flip(int index)
		{
			int dataIndex = index >> c_unitBitCount;
			int bitIndex = index & c_bitsMask;
			ulong mask = 1ul << bitIndex;
			m_data[dataIndex] ^= mask;
		}

		/// <summary>
		/// Set index to true
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int index)
		{
			int dataIndex = index >> c_unitBitCount;
			int bitIndex = index & c_bitsMask;
			ulong mask = 1ul << bitIndex;
			m_data[dataIndex] |= mask;
		}

		/// <summary>
		/// Set index to false
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int index)
		{
			int dataIndex = index >> c_unitBitCount;
			int bitIndex = index & c_bitsMask;
			ulong mask = 1ul << bitIndex;
			m_data[dataIndex] &= ~mask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int index, bool value)
		{
			int dataIndex = index >> c_unitBitCount;
			int bitIndex = index & c_bitsMask;
			ulong mask = 1ul << bitIndex;
			if (value)
				m_data[dataIndex] |= mask;
			else
				m_data[dataIndex] &= ~mask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CountSetBits()
		{
			int retval = 0;
			for (int i = 0; i < m_data.Length; i++)
				retval += BitOperations.PopCount(m_data[i]);
			return retval;
		}
	}
}
