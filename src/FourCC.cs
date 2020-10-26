#nullable enable
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Immutable 32 bit struct convertable to/from fourcc notation
	/// </summary>
	[DebuggerDisplay("{Readable}")]
	[StructLayout(LayoutKind.Sequential, Size = 4)]
	public readonly struct FourCC : IEquatable<FourCC>, IEquatable<uint>
	{
		public static readonly FourCC Empty = new FourCC(0);

		/// <summary>
		/// Big endian value; with regards to text
		/// </summary>
		public readonly uint Value;

		public string Readable => ToString();

		public FourCC(string str)
		{
			CoreException.Assert(str.Length >= 4);
			Value = (uint)((uint)str[3] << 24 | (uint)str[2] << 16 | (uint)str[1] << 8 | str[0]);
		}

		/// <summary>
		/// Pass bytes in reading order; ie { 'm', 'p', '4', 'a' }
		/// </summary>
		public FourCC(char a, char b, char c, char d)
		{
			Value = (uint)((uint)d << 24 | (uint)c << 16 | (uint)b << 8 | (uint)a);
		}

		public FourCC(ReadOnlySpan<byte> data)
		{
			CoreException.Assert(data.Length >= 4);
			Value = (uint)((uint)data[3] << 24 | (uint)data[2] << 16 | (uint)data[1] << 8 | (uint)data[0]);
		}

		/// <summary>
		/// Note; value should be big endian
		/// </summary>
		public FourCC(uint value)
		{
			Value = value;
		}

		public void ToString(Span<char> into)
		{
			into[0] = (char)(Value & 255u);
			into[1] = (char)(Value >> 8 & 255u);
			into[2] = (char)(Value >> 16 & 255u);
			into[3] = (char)(Value >> 24 & 255u);
		}

		public override string ToString()
		{
			Span<char> data = stackalloc char[4];
			ToString(data);
			return data.ToString();
		}

		public bool Equals(FourCC other)
		{
			return this.Value == other.Value;
		}

		public bool Equals(ReadOnlySpan<byte> bytes)
		{
			var value = BinaryPrimitives.ReadUInt32BigEndian(bytes);
			return value == Value;
		}

		public bool Equals(ReadOnlySpan<char> text)
		{
			CoreException.Assert(text.Length == 4);
			if (text.Length < 4)
				return false;

			Span<char> arr = stackalloc char[4];
			ToString(arr);

			return arr.SequenceEqual(text);
		}

		public override bool Equals(object? obj)
		{
			return obj != null && obj is FourCC && this.Equals((FourCC)obj);
		}

		public override int GetHashCode()
		{
			return (int)Value;
		}

		public bool Equals(uint other)
		{
			return other == Value;
		}

		public static bool operator ==(FourCC left, FourCC right)
		{
			return left.Value == right.Value;
		}

		public static bool operator !=(FourCC left, FourCC right)
		{
			return left.Value != right.Value;
		}

		public static bool operator ==(FourCC left, ReadOnlySpan<char> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FourCC left, ReadOnlySpan<char> right)
		{
			return !left.Equals(right);
		}

		public static implicit operator uint(FourCC cc)
		{
			return cc.Value;
		}

		public static implicit operator FourCC(uint value)
		{
			return new FourCC(value);
		}
	}
}
