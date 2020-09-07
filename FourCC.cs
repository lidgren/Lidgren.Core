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

		public readonly uint Value;

		public string Readable => ToString();

		public FourCC(string str)
		{
			CoreException.Assert(str.Length == 4);
			Value = (uint)((uint)str[3] << 24 | (uint)str[2] << 16 | (uint)str[1] << 8 | str[0]);
		}

		public FourCC(char byte1, char byte2, char byte3, char byte4)
		{
			Value = (uint)((uint)byte4 << 24 | (uint)byte3 << 16 | (uint)byte2 << 8 | byte1);
		}

		public FourCC(ReadOnlySpan<byte> data)
		{
			Value = BinaryPrimitives.ReadUInt32LittleEndian(data);
		}

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
			var value = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
			return value == Value;
		}

		public bool Equals(ReadOnlySpan<char> text)
		{
			CoreException.Assert(text.Length == 4);
			if (text.Length < 4)
				return false;
			var textValue = (uint)((uint)text[3] << 24 | (uint)text[2] << 16 | (uint)text[1] << 8 | text[0]);
			return textValue == Value;
		}

		public override bool Equals(object obj)
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
