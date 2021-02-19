#nullable enable
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Helper class to read incrementally from a Stream
	/// </summary>
	public ref struct DataReader
	{
		private Stream? m_stream;

		private Span<byte> m_buffer;
		private ReadOnlySpan<byte> m_remaining;

		public DataReader(Stream stream, Span<byte> buffer)
		{
			m_stream = stream;
			m_buffer = buffer;
			m_remaining = default;
		}

		public DataReader(ReadOnlySpan<byte> data)
		{
			m_stream = null;
			m_buffer = null;
			m_remaining = data;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private ReadOnlySpan<byte> Fill()
		{
			if (m_stream == null)
				return m_remaining; // nothing to fill from; created from finite data size

			int remLen = m_remaining.Length;
			m_remaining.CopyTo(m_buffer);

			int bytesRead = m_stream.Read(m_buffer.Slice(remLen));
			if (bytesRead == 0)
				throw new Exception("No more bytes to read");

			m_remaining = m_buffer.Slice(0, remLen + bytesRead);
			return m_remaining;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadBool()
		{
			if (m_remaining.Length < 1)
				Fill();
			return m_remaining.ReadBool();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort ReadUInt16()
		{
			if (m_remaining.Length < 2)
				Fill();
			return m_remaining.ReadUInt16();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint ReadUInt32()
		{
			if (m_remaining.Length < 4)
				Fill();
			return m_remaining.ReadUInt32();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadUInt64()
		{
			if (m_remaining.Length < 8)
				Fill();
			return m_remaining.ReadUInt64();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short ReadInt16()
		{
			if (m_remaining.Length < 2)
				Fill();
			return m_remaining.ReadInt16();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadInt32()
		{
			if (m_remaining.Length < 4)
				Fill();
			return m_remaining.ReadInt32();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadInt64()
		{
			if (m_remaining.Length < 8)
				Fill();
			return m_remaining.ReadInt64();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadSingle()
		{
			if (m_remaining.Length < 4)
				Fill();
			return m_remaining.ReadSingle();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double ReadDouble()
		{
			if (m_remaining.Length < 8)
				Fill();
			return m_remaining.ReadDouble();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString()
		{
			try
			{
				return m_remaining.ReadString();
			}
			catch
			{
				Fill();
				return m_remaining.ReadString();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadString(Span<char> into)
		{
			try
			{
				return m_remaining.ReadString(into);
			}
			catch
			{
				Fill();
				return m_remaining.ReadString(into);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadBytes(Span<byte> into)
		{
			if (m_remaining.Length < into.Length)
				Fill();
			var src = m_remaining.ReadBytes(into.Length);
			src.CopyTo(into);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> ReadBytes(int count)
		{
			if (m_remaining.Length < count)
				Fill();
			return m_remaining.ReadBytes(count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint ReadVariableUInt32()
		{
			if (m_remaining.Length < 5)
				Fill();
			return (uint)m_remaining.ReadVariableUInt64();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadVariableUInt64()
		{
			if (m_remaining.Length < 9)
				Fill();
			return m_remaining.ReadVariableUInt64();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadVariableInt64()
		{
			if (m_remaining.Length < 9)
				Fill();
			return m_remaining.ReadVariableInt64();
		}

		/// <summary>
		/// Reads an unmanaged struct (ie containing no reference types)
		/// </summary>
		public T Read<T>() where T : unmanaged
		{
			int size = Unsafe.SizeOf<T>();
			var rem = m_remaining;
			if (rem.Length < size)
				rem = Fill();
			var retval = MemoryMarshal.Read<T>(rem);
			m_remaining = rem.Slice(size);
			return retval;
		}

		public TArrItem[] ReadLengthPrefixedArray<TArrItem>() where TArrItem : unmanaged
		{
			var itemsCount = (int)ReadVariableUInt32();
			var itemSize = Unsafe.SizeOf<TArrItem>();
			var numBytes = itemsCount * itemSize;

			var rem = m_remaining;
			if (rem.Length < numBytes)
				rem = Fill();

			var src = MemoryMarshal.Cast<byte, TArrItem>(rem.Slice(0, numBytes));
			var retval = new TArrItem[itemsCount];
			src.CopyTo(retval);

			rem = rem.Slice(numBytes);
			m_remaining = rem;

			return retval;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2 ReadVector2()
		{
			if (m_remaining.Length < 8)
				Fill();
			return new Vector2(
				m_remaining.ReadSingle(),
				m_remaining.ReadSingle()
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 ReadVector3()
		{
			if (m_remaining.Length < 12)
				Fill();
			return new Vector3(
				m_remaining.ReadSingle(),
				m_remaining.ReadSingle(),
				m_remaining.ReadSingle()
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector4 ReadVector4()
		{
			if (m_remaining.Length < 16)
				Fill();
			return new Vector4(
				m_remaining.ReadSingle(),
				m_remaining.ReadSingle(),
				m_remaining.ReadSingle(),
				m_remaining.ReadSingle()
			);
		}
	}
}
