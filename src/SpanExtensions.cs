﻿using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	public static partial class SpanExtensions
	{
		/// <summary>
		/// Swap places of two ranges of a span, in-place
		/// Example:
		///   (pivot 4) 1234BBB becomes BBB1234
		///   (pivot 2) AABBBBB becomes BBBBBAA
		/// </summary>
		public static void SwapBlocks<T>(this Span<T> span, int pivot) where T : struct
		{
			span.Slice(0, pivot).Reverse();
			span.Slice(pivot, span.Length - pivot).Reverse();
			span.Reverse();
		}

		//
		// Byte encoding below; all methods "consumes" span space as they're written to/from
		//

		/// <summary>
		/// Returns new span of size count from the data, then reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<byte> ReadBytes(ref this ReadOnlySpan<byte> span, int count)
		{
			var retval = span.Slice(0, count);
			span = span.Slice(count);
			return retval;
		}

		/// <summary>
		/// Returns new span of size count from the data, then reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReadBytes(ref this ReadOnlySpan<byte> span, Span<byte> into)
		{
			span.Slice(0, into.Length).CopyTo(into);
			span = span.Slice(into.Length);
		}

		/// <summary>
		/// Writes a bool as a single byte and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteBool(ref this Span<byte> span, bool value)
		{
			span[0] = value ? (byte)1 : (byte)0;
			span = span.Slice(1);
		}

		/// <summary>
		/// Reads a bool from a single byte and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBool(ref this ReadOnlySpan<byte> span)
		{
			var retval = span[0] != 0 ? true : false;
			span = span.Slice(1);
			return retval;
		}

		/// <summary>
		/// Writes UInt16 as two bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUInt16(ref this Span<byte> span, ushort value)
		{
			BinaryPrimitives.WriteUInt16LittleEndian(span, value);
			span = span.Slice(2);
		}

		/// <summary>
		/// Reads UInt16 from two bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort ReadUInt16(ref this ReadOnlySpan<byte> span)
		{
			var retval = BinaryPrimitives.ReadUInt16LittleEndian(span);
			span = span.Slice(2);
			return retval;
		}

		/// <summary>
		/// Writes Int16 as two bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt16(ref this Span<byte> span, short value)
		{
			BinaryPrimitives.WriteInt16LittleEndian(span, value);
			span = span.Slice(2);
		}

		/// <summary>
		/// Reads Int16 from two bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short ReadInt16(ref this ReadOnlySpan<byte> span)
		{
			var retval = BinaryPrimitives.ReadInt16LittleEndian(span);
			span = span.Slice(2);
			return retval;
		}

		/// <summary>
		/// Writes UInt32 as four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUInt32(ref this Span<byte> span, uint value)
		{
			BinaryPrimitives.WriteUInt32LittleEndian(span, value);
			span = span.Slice(4);
		}

		/// <summary>
		/// Reads UInt32 from four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint ReadUInt32(ref this ReadOnlySpan<byte> span)
		{
			var retval = BinaryPrimitives.ReadUInt32LittleEndian(span);
			span = span.Slice(4);
			return retval;
		}

		/// <summary>
		/// Writes Int32 as four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt32(ref this Span<byte> span, int value)
		{
			BinaryPrimitives.WriteInt32LittleEndian(span, value);
			span = span.Slice(4);
		}

		/// <summary>
		/// Reads Int32 from four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt32(ref this ReadOnlySpan<byte> span)
		{
			var retval = BinaryPrimitives.ReadInt32LittleEndian(span);
			span = span.Slice(4);
			return retval;
		}

		/// <summary>
		/// Writes UInt64 as eight bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUInt64(ref this Span<byte> span, ulong value)
		{
			BinaryPrimitives.WriteUInt64LittleEndian(span, value);
			span = span.Slice(8);
		}

		/// <summary>
		/// Reads UInt64 from eight bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong ReadUInt64(ref this ReadOnlySpan<byte> span)
		{
			var retval = BinaryPrimitives.ReadUInt64LittleEndian(span);
			span = span.Slice(8);
			return retval;
		}

		/// <summary>
		/// Writes UInt64 as eight bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt64(ref this Span<byte> span, long value)
		{
			BinaryPrimitives.WriteInt64LittleEndian(span, value);
			span = span.Slice(8);
		}

		/// <summary>
		/// Reads UInt64 from eight bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ReadInt64(ref this ReadOnlySpan<byte> span)
		{
			var retval = BinaryPrimitives.ReadInt64LittleEndian(span);
			span = span.Slice(8);
			return retval;
		}

		/// <summary>
		/// Writes a Single as four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteSingle(ref this Span<byte> span, float value)
		{
#if NET5_0_OR_GREATER
			BinaryPrimitives.WriteSingleLittleEndian(span, value);
			span = span.Slice(4);
#else
			SingleUIntUnion union;
			union.UIntValue = 0;
			union.SingleValue = value;
			WriteUInt32(ref span, union.UIntValue);
#endif
		}

		/// <summary>
		/// Reads Single from four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ReadSingle(ref this ReadOnlySpan<byte> span)
		{
#if NET5_0_OR_GREATER
			var retval = BinaryPrimitives.ReadSingleLittleEndian(span);
			span = span.Slice(4);
			return retval;
#else
			SingleUIntUnion union;
			union.SingleValue = 0;
			union.UIntValue = ReadUInt32(ref span);
			return union.SingleValue;
#endif
		}

		/// <summary>
		/// Writes a Double as eight bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteDouble(ref this Span<byte> span, double value)
		{
#if NET5_0_OR_GREATER
			BinaryPrimitives.WriteDoubleLittleEndian(span, value);
			span = span.Slice(8);
#else
			DoubleULongUnion union;
			union.ULongValue = 0;
			union.DoubleValue = value;
			WriteUInt64(ref span, union.ULongValue);
#endif
		}

		/// <summary>
		/// Reads Single from four bytes and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double ReadDouble(ref this ReadOnlySpan<byte> span)
		{
#if NET5_0_OR_GREATER
			var retval = BinaryPrimitives.ReadDoubleLittleEndian(span);
			span = span.Slice(8);
			return retval;
#else
			DoubleULongUnion union;
			union.DoubleValue = 0;
			union.ULongValue = ReadUInt64(ref span);
			return union.DoubleValue;
#endif
		}

		/// <summary>
		/// Writes a length-prefixed character array; returns true on success and false on not enough space in span (in which case it does not reduce span)
		/// </summary>
		public static bool WriteString(ref this Span<byte> span, ReadOnlySpan<char> str)
		{
			if (span.Length < 2)
				return false;

			if (str.Length < 1)
			{
				// zero sized string; unfortunately can't distinguish between null or String.Empty
				span[0] = 0;
				span[1] = 0;
				span = span.Slice(2);
				return true;
			}

			int remaining = span.Length;
			if (remaining < 2 + str.Length)
				return false; // assume we need at least one byte per character

			try
			{
				var work = span.Slice(2);
				if (remaining < str.Length)
					return false; // assume we need at least one byte per character

				int bytesWritten = System.Text.Encoding.UTF8.GetBytes(str, work);
				span[0] = (byte)bytesWritten;
				span[1] = (byte)(bytesWritten >> 8);
				span = span.Slice(2 + bytesWritten);
				return true;
			}
			catch
			{
				// ugh, exception, screw performance - verify exception is indeed not enough space
				int needBytes = 2 + System.Text.Encoding.UTF8.GetByteCount(str);
				if (needBytes > remaining)
					return false; // space issue; just return false

				// NOT a space issue - rethrow
				throw;
			}
		}

		/// <summary>
		/// Reads a length-prefixed character array written using WriteString() and reduces span to remaining data; returns number of characters read, or -1 if not enough space in 'into'
		/// </summary>
		public static int ReadString(ref this ReadOnlySpan<byte> data, Span<char> into)
		{
			int byteLen = (int)data[0] | ((int)data[1] << 8);
			var numChars = System.Text.Encoding.UTF8.GetCharCount(data.Slice(2, byteLen));
			if (into.Length < numChars)
				return -1;
			System.Text.Encoding.UTF8.GetChars(data.Slice(2), into);
			data = data.Slice(2 + byteLen);
			return numChars;
		}

		/// <summary>
		/// Reads a length-prefixed character array written using WriteString() and reduces span to remaining data
		/// </summary>
		public static string ReadString(ref this ReadOnlySpan<byte> data)
		{
			int byteLen = (int)data[0] | ((int)data[1] << 8);
			var retval = System.Text.Encoding.UTF8.GetString(data.Slice(2, byteLen));
			data = data.Slice(2 + byteLen);
			return retval;
		}

		/// <summary>
		/// Write an unmanaged struct (ie containing no reference types) and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(ref this Span<byte> span, T value) where T : unmanaged
		{
			MemoryMarshal.Write<T>(span, ref value);
			span = span.Slice(Unsafe.SizeOf<T>());
		}

		/// <summary>
		/// Reads an unmanaged struct (ie containing no reference types) and reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(ref this ReadOnlySpan<byte> data) where T : unmanaged
		{
			var retval = MemoryMarshal.Read<T>(data);
			data = data.Slice(Unsafe.SizeOf<T>());
			return retval;
		}

		public static void WriteLengthPrefixedArray<TArrItem>(ref this Span<byte> span, ReadOnlySpan<TArrItem> items) where TArrItem : unmanaged
		{
			span.WriteVariableUInt32((uint)items.Length);
			if (items.Length == 0)
				return;
			var src = MemoryMarshal.AsBytes(items);
			src.CopyTo(span);
			span = span.Slice(src.Length);
		}

		/// <summary>
		/// Reads a length prefixed array written by WriteLengthPrefixedArray()
		/// </summary>
		public static TArrItem[] ReadLengthPrefixedArray<TArrItem>(ref this ReadOnlySpan<byte> data) where TArrItem : unmanaged
		{
			var itemsCount = (int)data.ReadVariableUInt32();
			var itemSize = Unsafe.SizeOf<TArrItem>();
			var numBytes = itemsCount * itemSize;

			var src = MemoryMarshal.Cast<byte, TArrItem>(data.Slice(0, numBytes));
			var retval = new TArrItem[itemsCount];
			src.CopyTo(retval);

			data = data.Slice(numBytes);
			return retval;
		}

		/// <summary>
		/// Reads Vector2 reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ReadVector2(ref this ReadOnlySpan<byte> data)
		{
#if NET5_0_OR_GREATER
			Vector2 retval;
			retval.X = BinaryPrimitives.ReadSingleLittleEndian(data);
			retval.Y = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));
			data = data.Slice(8);
			return retval;
#else
			Vector2 retval;
			retval.X = data.ReadSingle();
			retval.Y = data.ReadSingle();
			return retval;
#endif
		}

		/// <summary>
		/// Reads Vector3 reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ReadVector3(ref this ReadOnlySpan<byte> data)
		{
#if NET5_0_OR_GREATER
			Vector3 retval;
			retval.X = BinaryPrimitives.ReadSingleLittleEndian(data);
			retval.Y = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));
			retval.Z = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(8));
			data = data.Slice(12);
			return retval;
#else
			Vector3 retval;
			retval.X = data.ReadSingle();
			retval.Y = data.ReadSingle();
			retval.Z = data.ReadSingle();
			return retval;
#endif
		}

		/// <summary>
		/// Reads Vector4 reduces span to remaining data
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 ReadVector4(ref this ReadOnlySpan<byte> data)
		{
#if NET5_0_OR_GREATER
			Vector4 retval;
			retval.X = BinaryPrimitives.ReadSingleLittleEndian(data);
			retval.Y = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));
			retval.Z = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(8));
			retval.W = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(12));
			data = data.Slice(16);
			return retval;
#else
			Vector4 retval;
			retval.X = data.ReadSingle();
			retval.Y = data.ReadSingle();
			retval.Z = data.ReadSingle();
			retval.W = data.ReadSingle();
			return retval;
#endif
		}
	}
}
