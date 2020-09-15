using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

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
	}
}
