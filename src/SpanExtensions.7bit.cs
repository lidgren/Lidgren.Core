using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	//
	// Variable length integer encoding "7bit encoding"
	//
	// Encodes using 1 byte:    2^0 >= x <  2^7    (0 to 127)  
	// Encodes using 2 bytes:   2^7 >= x < 2^14    (128 - 16383)  
	// Encodes using 3 bytes:  2^14 >= x < 2^21    (16384 - 2097151)  
	// Encodes using 4 bytes:  2^21 >= x < 2^28    (2097152 - 268435455)  
	// Encodes using 5 bytes:  2^28 >= x < 2^35    (268435456 - 34359738367)
	// Encodes using 6 bytes:  2^35 >= x < 2^42    (34359738368 - 4398046511103)
	// Encodes using 7 bytes:  2^42 >= x < 2^49    (4398046511104 - 562949953421311)
	// Encodes using 8 bytes:  2^49 >= x < 2^56    (562949953421312 - 72057594037927935)
	// Encodes using 9 bytes:  2^56 >= x <= 2^64   (72057594037927936 - 18446744073709551615)
	//
	public static partial class SpanExtensions
	{
		/// <summary>
		/// Writes UInt64 as a 7 bit encoded number (variable length: 1 to 9 bytes) and reduces span to remaining data
		/// </summary>
		public static void WriteVariable(ref this Span<byte> span, ulong value)
		{
			if (value < 0x80)
			{
				span[0] = (byte)value;
				span = span.Slice(1);
				return;
			}

			span[0] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[1] = (byte)value;
				span = span.Slice(2);
				return;
			}

			span[1] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[2] = (byte)value;
				span = span.Slice(3);
				return;
			}

			span[2] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[3] = (byte)value;
				span = span.Slice(4);
				return;
			}

			span[3] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[4] = (byte)value;
				span = span.Slice(5);
				return;
			}

			span[4] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[5] = (byte)value;
				span = span.Slice(6);
				return;
			}

			span[5] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[6] = (byte)value;
				span = span.Slice(7);
				return;
			}

			span[6] = (byte)(value | 0x80);
			value >>= 7;

			if (value < 0x80)
			{
				span[7] = (byte)value;
				span = span.Slice(8);
				return;
			}

			span[7] = (byte)(value | 0x80);
			value >>= 7;

			span[8] = (byte)value;
			span = span.Slice(9);
			return;
		}

		/// <summary>
		/// Writes Int64 as a 7 bit encoded number (variable length: 1 to 9 bytes) and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteVariable(ref this Span<byte> span, long value)
		{
			// zigzag encode
			var uval = (ulong)((value << 1) ^ (value >> 63));
			WriteVariable(ref span, uval);
		}

		/// <summary>
		/// Reads UInt64 from eight bytes and reduces span to remaining data
		/// </summary>
		public static ulong ReadVariableUInt64(ref this ReadOnlySpan<byte> data)
		{
			ulong value = data[0];
			if ((value & 0x80) == 0)
			{
				data = data.Slice(1);
				return value;
			}
			value &= 0x7F;

			ulong chunk = data[1];
			if ((chunk & 0x80) == 0)
			{
				data = data.Slice(2);
				return value | (chunk << 7);
			}
			value |= (chunk & 0x7F) << 7;

			ulong another = data[2];
			if ((another & 0x80) == 0)
			{
				data = data.Slice(3);
				return value | (another << 14);
			}
			value |= (another & 0x7F) << 14;

			chunk = data[3];
			if ((chunk & 0x80) == 0)
			{
				data = data.Slice(4);
				return value | (chunk << 21);
			}
			value |= (chunk & 0x7F) << 21;

			another = data[4];
			if ((another & 0x80) == 0)
			{
				data = data.Slice(5);
				return value | (another << 28);
			}
			value |= (another & 0x7F) << 28;

			chunk = data[5];
			if ((chunk & 0x80) == 0)
			{
				data = data.Slice(6);
				return value | (chunk << 35);
			}
			value |= (chunk & 0x7F) << 35;

			another = data[6];
			if ((another & 0x80) == 0)
			{
				data = data.Slice(7);
				return value | (another << 42);
			}
			value |= (another & 0x7F) << 42;

			chunk = data[7];
			if ((chunk & 0x80) == 0)
			{
				data = data.Slice(8);
				return value | (chunk << 49);
			}
			value |= (chunk & 0x7F) << 49;

			value |= (ulong)data[8] << 56; // full 8 bits in this byte
			data = data.Slice(9);
			return value;
		}

		private const long InvInt64Msb = ~(((long)1) << 63);

		/// <summary>
		/// Reads UInt64 from eight bytes and reduces span to remaining data
		/// </summary>
		public static long ReadVariableInt64(ref this ReadOnlySpan<byte> data)
		{
			long value = (long)ReadVariableUInt64(ref data);

			// zigzag decode
			return (-(value & 0x01L)) ^ ((value >> 1) & InvInt64Msb);
		}
	}
}
