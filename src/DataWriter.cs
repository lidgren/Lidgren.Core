using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	public static class ByteEncodingExtensions
	{
		
	}

	/*

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(bool value)
		{
			Allocate(1)[0] = value ? (byte)1 : (byte)0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(ushort value)
		{
			var into = Allocate(2);
			into[0] = (byte)value;
			into[1] = (byte)(value >> 8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(uint value)
		{
			var into = Allocate(4);
			BinaryPrimitives.WriteUInt32LittleEndian(into, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(int value)
		{
			var into = Allocate(4);
			BinaryPrimitives.WriteInt32LittleEndian(into, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(ulong value)
		{
			var into = Allocate(8);
			BinaryPrimitives.WriteUInt64LittleEndian(into, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVariable(ulong value)
		{
			EnsureCapacity(9);
			while (value >= 0x80)
			{
				m_data[m_length++] = (byte)(value | 0x80);
				value >>= 7;
			}
			m_data[m_length++] = (byte)value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVariable(long value)
		{
			// zigzag encode
			var uval = (ulong)((value << 1) ^ (value >> 63));
			WriteVariable(uval);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(float value)
		{
#if NET5_0
			BinaryPrimitives.WriteSingleLittleEndian(Allocate(4), value);
#else
			SingleUIntUnion union;
			union.UIntValue = 0;
			union.SingleValue = value;
			Write(union.UIntValue);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteStruct<T>(ref T value, int byteLength) where T : struct
		{
			var into = Allocate(byteLength);
			CoreException.Assert(Unsafe.SizeOf<T>() == byteLength);
			MemoryMarshal.Write(into, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(double value)
		{
#if NET5_0
			var into = Allocate(8);
			BinaryPrimitives.WriteDoubleLittleEndian(into, value);
#else
			DoubleULongUnion union;
			union.ULongValue = 0;
			union.DoubleValue = value;
			Write(union.ULongValue);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(ReadOnlySpan<byte> data)
		{
			var dst = Allocate(data.Length);
			data.CopyTo(dst);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(byte data)
		{
			Allocate(1)[0] = data;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteString(ReadOnlySpan<char> str)
		{
			var sizeStore = Allocate(2);

			if (str.Length < 1)
			{
				// zero sized string; unfortunately can't distinguish between null or String.Empty
				sizeStore[0] = 0;
				sizeStore[1] = 0;
				return;
			}

			for (; ; )
			{
				try
				{
					var remaining = m_data.AsSpan(m_length);
					int bytesWritten = System.Text.Encoding.UTF8.GetBytes(str, remaining);
					m_length += bytesWritten;
					sizeStore[0] = (byte)bytesWritten;
					sizeStore[1] = (byte)(bytesWritten >> 8);
					return;
				}
				catch (IndexOutOfRangeException)
				{
					// ok, we ran out of bytes; grow and try again
					Grow(m_length + m_length + 2); // approximation
				}
			}
		}

		public static int ReadString(ReadOnlySpan<byte> data, Span<char> into)
		{
			int byteLen = (int)data[0] | ((int)data[1] << 8);
			return System.Text.Encoding.UTF8.GetChars(data, into);
		}

		/// <summary>
		/// Write length prefixed array of T
		/// </summary>
		public void WriteArray<T>(ReadOnlySpan<T> span) where T : struct
		{
			WriteVariable((uint)span.Length);
			if (span.Length == 0)
				return;
			var src = MemoryMarshal.AsBytes(span);
			var into = Allocate(src.Length);
			src.CopyTo(into);
		}
	}
	*/
}
