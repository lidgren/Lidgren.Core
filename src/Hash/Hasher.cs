#nullable enable
using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Incremental hashing based on MurmurHash64A by Austin Appleby
	/// </summary>
	[StructLayout(LayoutKind.Auto)]
	public ref struct Hasher
	{
		private const ulong M = 0xc6a4a7935bd1e995ul;
		private const int R = 47;

		private ulong m_hash;
		private ulong m_unit;
		private int m_bitCount; // how many unconsumed bits are stored in m_unit

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Hasher Create()
		{
			return new Hasher(M);
		}

		public readonly Hasher Clone()
		{
			Hasher retval;
			retval.m_hash = m_hash;
			retval.m_unit = m_unit;
			retval.m_bitCount = m_bitCount;
			return retval;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Hasher(in ulong seed)
		{
			m_hash = seed;
			m_unit = 0;
			m_bitCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset(in ulong seed = M)
		{
			m_hash = seed;
			m_unit = 0;
			m_bitCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Consume(ulong data)
		{
			unchecked
			{
				data *= M;
				data ^= data >> R;
				data *= M;

				ulong hash = m_hash ^ data;
				hash *= M;
				m_hash = hash;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ConsumeUnit()
		{
			unchecked
			{
				var unit = m_unit * M;
				unit ^= unit >> R;
				unit *= M;

				var hash = m_hash ^ unit;
				hash *= M;

				m_unit = unit;
				m_hash = hash;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(byte data)
		{
			unchecked
			{
				int bc = m_bitCount;
				m_unit |= ((ulong)data << bc);
				bc = (bc + 8) & 63;
				if (bc == 0)
				{
					ConsumeUnit();
					m_unit = 0;
					m_bitCount = 0;
				}
				m_bitCount = bc;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(short value)
		{
			Add((ushort)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int value)
		{
			Add((uint)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(float value)
		{
			SingleUIntUnion union;
			union.UIntValue = 0;
			union.SingleValue = value;
			Add(union.UIntValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Vector2 value)
		{
			SinglesULongUnion union;
			union.ULongValue = 0;
			union.SingleValue0 = value.X;
			union.SingleValue1 = value.Y;
			Add(union.ULongValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Vector3 value)
		{
			SingleUIntUnion union;
			union.UIntValue = 0;
			union.SingleValue = value.X;
			Add(union.UIntValue);
			union.SingleValue = value.Y;
			Add(union.UIntValue);
			union.SingleValue = value.Z;
			Add(union.UIntValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Vector4 value)
		{
			SinglesULongUnion union;
			union.ULongValue = 0;
			union.SingleValue0 = value.X;
			union.SingleValue1 = value.Y;
			Add(union.ULongValue);
			union.SingleValue0 = value.Z;
			union.SingleValue1 = value.W;
			Add(union.ULongValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Quaternion value)
		{
			// TODO: check if using AddStructBytes<Quaternion> is faster
			SinglesULongUnion union;
			union.ULongValue = 0;
			union.SingleValue0 = value.X;
			union.SingleValue1 = value.Y;
			Add(union.ULongValue);
			union.SingleValue0 = value.Z;
			union.SingleValue1 = value.W;
			Add(union.ULongValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(float one, float two)
		{
			SinglesULongUnion union;
			union.ULongValue = 0;
			union.SingleValue0 = one;
			union.SingleValue1 = two;
			Add(union.ULongValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(double value)
		{
			DoubleULongUnion union;
			union.ULongValue = 0;
			union.DoubleValue = value;
			Add(union.ULongValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ushort data)
		{
			unchecked
			{
				if (m_bitCount == 56)
				{
					// special case; only room for one more byte in m_unit
					m_unit |= ((ulong)data << 56); // store lower byte; trunc top byte
					ConsumeUnit();
					m_unit = ((ulong)data >> 8); // store top byte
					m_bitCount = 8;
					return;
				}

				m_unit |= ((ulong)data << m_bitCount);
				m_bitCount = (m_bitCount + 16) & 63;
				if (m_bitCount == 0)
				{
					ConsumeUnit();
					m_unit = 0;
					m_bitCount = 0;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(uint data)
		{
			unchecked
			{
				int bc = m_bitCount;
				m_unit |= ((ulong)data << bc); // fill entire or trunc top
				if (bc < 32)
				{
					m_bitCount += 32;
					return;
				}

				ConsumeUnit();

				if (bc == 32)
				{
					m_unit = 0;
					m_bitCount = 0;
					return;
				}

				int nbc = 64 - bc;
				m_bitCount = nbc;
				m_unit = data >> nbc;
			}
		}

		public void Add(byte[] data, int offset, int length)
		{
			int end = offset + length;
			while (m_bitCount != 0 && offset != end)
				Add(data[offset++]);

			while (end - offset >= 8)
			{
				// add full ulongs
				var span = new ReadOnlySpan<byte>(data, offset, 8);
				ulong add = MemoryMarshal.Read<ulong>(span);
				Consume(add);
				offset += 8;
			}

			// add remaining bytes
			while (offset != end)
				Add(data[offset++]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ReadOnlySpan<int> data)
		{
			Add(MemoryMarshal.Cast<int, byte>(data));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ReadOnlySpan<uint> data)
		{
			Add(MemoryMarshal.Cast<uint, byte>(data));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ReadOnlySpan<ushort> data)
		{
			Add(MemoryMarshal.Cast<ushort, byte>(data));
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public void Add(ReadOnlySpan<byte> data)
		{
			unchecked
			{
				// add bytes until unit is empty
				var bc = m_bitCount;
				while (bc != 0)
				{
					if (data.Length < 1)
					{
						m_bitCount = bc;
						return;
					}
					m_unit |= ((ulong)data[0] << bc);
					data = data.Slice(1);
					bc = (bc + 8) & 63;
					if (bc == 0)
					{
						ConsumeUnit();
						m_unit = 0;
						m_bitCount = 0;
						break;
					}
				}

				int numChunks = Math.DivRem(data.Length, 8, out var remainder);
				int fastBytes = data.Length - remainder;
				if (numChunks > 0)
				{
					// add full ulongs
					ulong hash = m_hash;
					var chunks = MemoryMarshal.Cast<byte, ulong>(data.Slice(0, fastBytes));
					CoreException.Assert(numChunks == chunks.Length);
					while (chunks.Length > 0)
					{
						ulong bits = chunks[0];
						bits *= M;
						bits ^= bits >> R;
						bits *= M;
						hash ^= bits;
						hash *= M;
						chunks = chunks.Slice(1);
					}
					m_hash = hash;
				}

				// add remaining bytes; they will fit into unit
				CoreException.Assert(m_bitCount == 0);

				data = data.Slice(fastBytes);
				CoreException.Assert(data.Length >= 0 && data.Length <= 7);
				m_bitCount = data.Length * 8;
				switch (data.Length)
				{
					case 0:
						return;
					case 1:
						m_unit = data[0];
						return;
					case 2:
						m_unit = data[0] | (ulong)data[1] << 8;
						return;
					case 3:
						m_unit = data[0] | (ulong)data[1] << 8 | (ulong)data[2] << 16;
						return;
					case 4:
						m_unit = data[0] | (ulong)data[1] << 8 | (ulong)data[2] << 16 | (ulong)data[3] << 24;
						return;
					case 5:
						m_unit = data[0] | (ulong)data[1] << 8 | (ulong)data[2] << 16 | (ulong)data[3] << 24 | (ulong)data[4] << 32;
						return;
					case 6:
						m_unit = data[0] | (ulong)data[1] << 8 | (ulong)data[2] << 16 | (ulong)data[3] << 24 | (ulong)data[4] << 32 | (ulong)data[5] << 40;
						return;
					case 7:
						m_unit = data[0] | (ulong)data[1] << 8 | (ulong)data[2] << 16 | (ulong)data[3] << 24 | (ulong)data[4] << 32 | (ulong)data[5] << 40 | (ulong)data[6] << 48;
						return;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(bool value)
		{
			Add(value ? (byte)0b11001100 : (byte)0b10101010); // arbitrarily chosen numbers
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public void Add(ReadOnlySpan<char> str)
		{
			if (str.Length <= 0)
			{
				Add((byte)0);
				return;
			}

			var bytes = MemoryMarshal.Cast<char, byte>(str);
			Add(bytes);
		}

		public void AddStructBytes<T>(ref T item) where T : unmanaged
		{
			int sizeInBytes = Unsafe.SizeOf<T>();
			ReadOnlySpan<T> span = MemoryMarshal.CreateSpan<T>(ref item, sizeInBytes);
			ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes<T>(span);
			Add(bytes);
		}

		public void AddStructBytes<T>(ReadOnlySpan<T> items) where T : unmanaged
		{
			var bytes = MemoryMarshal.Cast<T, byte>(items);
			Add(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddLowerInvariant(ReadOnlySpan<char> str)
		{
			if (str.Length < 1)
			{
				Add((byte)0);
				return;
			}
			foreach (var c in str)
				Add((ushort)Char.ToLowerInvariant(c));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddLower(ReadOnlySpan<char> str)
		{
			if (str.Length < 1)
			{
				Add((byte)0);
				return;
			}
			foreach (var c in str)
				Add((ushort)Char.ToLower(c));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ulong data)
		{
			unchecked
			{
				int bc = m_bitCount;
				if (bc == 0)
				{
					Consume(data);
					return;
				}

				// fill using lower bytes
				m_unit |= (data << bc);
				ConsumeUnit();

				// replace unit with upper bytes
				// bitcount remains the same since we added a full 64 bit value
				m_unit = (data >> (64 - bc));
			}
		}

		/// <summary>
		/// Use this if the data you've added so far aligns to 64 bits
		/// </summary>
		public void AddAligned64(ulong data)
		{
			Consume(data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong Finalize64()
		{
			unchecked
			{
				ulong hash = m_hash;
				if (m_bitCount != 0)
				{
					hash ^= m_unit;
					hash *= M;
				}
				hash ^= hash >> R;
				hash *= M;
				hash ^= hash >> R;
				m_hash = hash;
				return hash;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint Finalize32()
		{
			return HashUtil.XorFold64To32(Finalize64());
		}
	}
}
