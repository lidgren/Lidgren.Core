﻿#nullable enable
using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Expanding buffer using ArrayPool{byte} backing; get results using ReadOnlySpan and remember to dispose
	/// </summary>
	public struct DataWriter : IDisposable
	{
		private byte[] m_buffer;
		private int m_length;

		public DataWriter(int initialCapacity)
		{
			m_buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
			m_length = 0;
		}

		public void Dispose()
		{
			if (m_buffer != null)
				ArrayPool<byte>.Shared.Return(m_buffer);
		}

		public Span<byte> Span => m_buffer.AsSpan(0, m_length);
		public ReadOnlySpan<byte> ReadOnlySpan => m_buffer.AsSpan(0, m_length);

		public void Clear()
		{
			m_length = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<byte> Allocate(int numBytes)
		{
			int len = m_length;
			int remaining = m_buffer.Length - len;
			if (numBytes > remaining)
				Grow(len + numBytes);
			m_length = len + numBytes;
			return m_buffer.AsSpan(len, numBytes);
		}

		/// <summary>
		/// Grow capacity to be able to add this many bytes without re-allocating
		/// </summary>
		public void EnsureCapacity(int size)
		{
			int minTotalSize = m_length + size;
			if (m_buffer.Length > minTotalSize)
				return; // we're good
			Grow(minTotalSize);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Grow(int minSize)
		{
			// grow more carefully than doubling; esp. since ArrayPool may return longer
			int curLen = m_buffer.Length;
			int grow = 16 + (curLen / 4);
			int newSize = Math.Max(curLen + grow, minSize);

			var newBuf = ArrayPool<byte>.Shared.Rent(newSize);
			ReadOnlySpan.CopyTo(newBuf);

			ArrayPool<byte>.Shared.Return(m_buffer);
			m_buffer = newBuf;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteBool(bool value)
		{
			Allocate(1)[0] = value ? (byte)1 : (byte)0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteUInt16(ushort value)
		{
			var into = Allocate(2);
			into.WriteUInt16(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteUInt32(uint value)
		{
			var into = Allocate(4);
			into.WriteUInt32(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteInt32(int value)
		{
			var into = Allocate(4);
			into.WriteInt32(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteUInt64(ulong value)
		{
			var into = Allocate(8);
			into.WriteUInt64(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVariableUInt64(ulong value)
		{
			var into = Allocate(9); // worst case
			into.WriteVariableUInt64(value);
			m_length -= into.Length; // return unused space
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVariableInt64(long value)
		{
			var into = Allocate(9); // worst case
			into.WriteVariableInt64(value);
			m_length -= into.Length; // return unused space
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVariableUInt32(uint value)
		{
			var into = Allocate(5); // worst case
			into.WriteVariableUInt64(value);
			m_length -= into.Length; // return unused space
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVariableInt32(int value)
		{
			var into = Allocate(5); // worst case
			into.WriteVariableInt64(value);
			m_length -= into.Length; // return unused space
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteSingle(float value)
		{
			var into = Allocate(4);
			into.WriteSingle(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteFloat(float value)
		{
			var into = Allocate(4);
			into.WriteSingle(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteDouble(double value)
		{
			var into = Allocate(8);
			into.WriteDouble(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteBytes(ReadOnlySpan<byte> data)
		{
			var into = Allocate(data.Length);
			data.CopyTo(into);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteByte(byte value)
		{
			Allocate(1)[0] = value;
		}

		public void WriteString(ReadOnlySpan<char> str)
		{
			int curLen = m_length;
			var minCap = curLen + 2 + str.Length;

			// grow?
			if (minCap > m_buffer.Length)
				Grow(minCap);

			for (; ; )
			{
				var span = m_buffer.AsSpan(curLen);
				bool ok = span.WriteString(str);
				if (ok)
				{
					m_length = m_buffer.Length - span.Length;
					return;
				}
				Grow(curLen + 2 + (str.Length * 2));
			}
		}

		/// <summary>
		/// Write length prefixed array of T
		/// </summary>
		public void WriteLengthPrefixedArray<T>(ReadOnlySpan<T> span) where T : unmanaged
		{
			WriteVariableUInt32((uint)span.Length);
			if (span.Length == 0)
				return;
			var src = MemoryMarshal.AsBytes(span);
			var into = Allocate(src.Length);
			src.CopyTo(into);
		}

		/// <summary>
		/// Write an unmanaged struct (ie containing no reference types)
		/// </summary>
		public void Write<T>(T value) where T : unmanaged
		{
			var into = Allocate(Unsafe.SizeOf<T>());
			MemoryMarshal.Write<T>(into, ref value);
		}

		public void WriteVector2(in Vector2 value)
		{
			var into = Allocate(8);
			var xslice = into.Slice(0, 4);
			xslice.WriteSingle(value.X);
			var yslice = into.Slice(4, 4);
			yslice.WriteSingle(value.Y);
		}

		public void WriteVector3(in Vector3 value)
		{
			var into = Allocate(12);
			var xslice = into.Slice(0, 4);
			xslice.WriteSingle(value.X);
			var yslice = into.Slice(4, 4);
			yslice.WriteSingle(value.Y);
			var zslice = into.Slice(8, 4);
			zslice.WriteSingle(value.Z);
		}

		public void WriteVector4(in Vector4 value)
		{
			var into = Allocate(16);
			var xslice = into.Slice(0, 4);
			xslice.WriteSingle(value.X);
			var yslice = into.Slice(4, 4);
			yslice.WriteSingle(value.Y);
			var zslice = into.Slice(8, 4);
			zslice.WriteSingle(value.Z);
			var wslice = into.Slice(12, 4);
			wslice.WriteSingle(value.W);
		}
	}
}
