#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public static class StringUtils
	{
		private static char[] s_valueToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		/// <summary>
		/// Single hex character to integer; ie. 'B' to 11; supports any casing
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int HexCharToInteger(char c)
		{
			var d = (int)c;
			return (d & 0xf) + (d >> 6) + ((d >> 6) << 3);
		}

		/// <summary>
		/// Convert string of hex characters to integer; ie. "0x1F" or just "1F" to 31; supports any casing
		/// </summary>
		public static ulong FromHex(ReadOnlySpan<char> hexString)
		{
			if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				hexString = hexString.Slice(2);
			ulong result = 0;
			while (hexString.Length > 0)
			{
				var val = (uint)HexCharToInteger(hexString[0]);
				result = (result << 4) | val;
				hexString = hexString.Slice(1);
			}
			return result;
		}

		/// <summary>
		/// Convert string of bytes representing hex characters in UTF8 format to integer; ie. "0x1F" or just "1F" to 31; supports any casing
		/// </summary>
		public static ulong FromUTF8Hex(ReadOnlySpan<byte> hexUTF8Bytes)
		{
			if (hexUTF8Bytes.Length >= 2)
			{
				if (hexUTF8Bytes[0] == (byte)'0' && hexUTF8Bytes[1] == (byte)'x')
					hexUTF8Bytes = hexUTF8Bytes.Slice(2);
			}
			ulong result = 0;
			while (hexUTF8Bytes.Length > 0)
			{
				uint d = (uint)hexUTF8Bytes[0];
				var val = (d & 0xf) + (d >> 6) + ((d >> 6) << 3);
				result = (result << 4) | val;
				hexUTF8Bytes = hexUTF8Bytes.Slice(1);
			}
			return result;
		}

		/// <summary>
		/// Convert integer to string of hex characters; ie. 31 to "1F"
		/// </summary>
		public static int ToHex(ulong value, Span<char> destination, int zeroPadToCharLength = 0)
		{
			bool ok = value.TryFormat(destination, out int written, format: "X");
			if (!ok)
				return 0; // failed

			int pad = zeroPadToCharLength - written;
			if (pad > 0)
			{
				destination.Slice(0, written).CopyTo(destination.Slice(pad));
				destination.Slice(0, pad).Fill('0');
				return zeroPadToCharLength;
			}
			return written;
		}

		/// <summary>
		/// Convert integer to string of hex characters; ie. 31 to "1F"
		/// </summary>
		public static string ToHex(ulong value, int zeroPadToCharLength = 0)
		{
			Span<char> tmp = stackalloc char[Math.Max(16, zeroPadToCharLength)];
			int written = ToHex(value, tmp, zeroPadToCharLength);
			return tmp.Slice(0, written).ToString();
		}

		/// <summary>
		/// Convert array of bytes to string of hex characters; ie. { 31, 00 } to "1F00"
		/// </summary>
		public static void ToHex(ReadOnlySpan<byte> data, Span<char> into)
		{
			CoreException.Assert(into.Length >= data.Length * 2);
			for (int i = 0; i < data.Length; i++)
			{
				into[i * 2] = s_valueToHex[data[i] >> 4];
				into[i * 2 + 1] = s_valueToHex[data[i] & 0b1111];
			}
		}

		/// <summary>
		/// Convert array of bytes to string of hex characters; ie. { 31, 00 } to "1F00"
		/// </summary>
		public static string ToHex(ReadOnlySpan<byte> data)
		{
			if (data.Length < 1024)
			{
				Span<char> stacktmp = stackalloc char[data.Length * 2];
				ToHex(data, stacktmp);
				return stacktmp.ToString();
			}

			var tmp = new char[data.Length * 2];
			ToHex(data, tmp);
			return new string(tmp);
		}

		private const string c_doubleFixedPoint = "0.###################################################################################################################################################################################################################################################################################################################################################";

		/// <summary>
		/// Without scientific notation (0.00000000000123 -> "0.00000000000123", not "1.23E-12")
		/// </summary>
		public static string DoubleToString(double value, IFormatProvider? culture = null)
		{
			if (culture != null)
				return value.ToString(c_doubleFixedPoint, culture);
			return value.ToString(c_doubleFixedPoint, System.Globalization.CultureInfo.InvariantCulture);
		}
	}
}
