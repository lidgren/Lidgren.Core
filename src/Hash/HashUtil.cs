#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	public static class HashUtil
	{
		public static uint Hash32(ReadOnlySpan<byte> bytes)
		{
			var hasher = Hasher.Create();
			hasher.Add(bytes);
			return hasher.Finalize32();
		}

		public static ulong Hash64(ReadOnlySpan<byte> bytes)
		{
			var hasher = Hasher.Create();
			hasher.Add(bytes);
			return hasher.Finalize64();
		}

		public static uint Hash32(ReadOnlySpan<char> str)
		{
			var hasher = Hasher.Create();
			hasher.Add(str);
			return hasher.Finalize32();
		}

		public static ulong Hash64(ReadOnlySpan<char> str)
		{
			var hasher = Hasher.Create();
			hasher.Add(str);
			return hasher.Finalize64();
		}

		/// <summary>
		/// Hash string converted to utf-8; note: stackallocs 2x length of string
		/// </summary>
		public static ulong Utf8Hash64(ReadOnlySpan<char> str)
		{
			Span<byte> bytes = stackalloc byte[str.Length * 2];
			int cnt = System.Text.Encoding.UTF8.GetBytes(str, bytes);
			return Hash64(bytes.Slice(0, cnt));
		}

		/// <summary>
		/// Hash string converted to utf-8; note: stackallocs 2x length of string
		/// </summary>
		public static uint Utf8Hash32(ReadOnlySpan<char> str)
		{
			Span<byte> bytes = stackalloc byte[str.Length * 2];
			int cnt = System.Text.Encoding.UTF8.GetBytes(str, bytes);
			return Hash32(bytes.Slice(0, cnt));
		}

		public static uint HashLower32(ReadOnlySpan<char> str)
		{
			var hasher = Hasher.Create();
			hasher.AddLowerInvariant(str);
			return hasher.Finalize32();
		}

		public static ulong HashLower64(ReadOnlySpan<char> str)
		{
			var hasher = Hasher.Create();
			hasher.AddLowerInvariant(str);
			return hasher.Finalize64();
		}

		public static ulong Hash64<T>(ref T instance, int byteCount) where T : struct
		{
			var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan<T>(ref instance, 1));
			var hasher = Hasher.Create();
			hasher.Add(bytes);
			return hasher.Finalize64();
		}

		public static ulong Hash64(ulong value)
		{
			// MurmurHash3 finalizer
			value ^= value >> 33;
			value *= 0xff51afd7ed558ccd;
			value ^= value >> 33;
			value *= 0xc4ceb9fe1a85ec53;
			value ^= value >> 33;
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong Hash64(float x, float y)
		{
			SingleUIntUnion union;
			union.UIntValue = 0;

			union.SingleValue = x;
			ulong input1 = union.UIntValue;
			union.SingleValue = y;
			input1 |= (union.UIntValue << 32);

			return Hash64(input1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong Hash64(float x, float y, float z)
		{
			var hasher = Hasher.Create();
			SingleUIntUnion union;
			union.UIntValue = 0;
			
			union.SingleValue = x;
			ulong input1 = union.UIntValue;
			union.SingleValue = y;
			input1 |= (union.UIntValue << 32);
			hasher.AddAligned64(input1);

			union.SingleValue = z;
			hasher.Add(union.UIntValue);
			
			return hasher.Finalize64();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong Hash64(float x, float y, float z, float w)
		{
			SingleUIntUnion union;
			union.UIntValue = 0;

			union.SingleValue = x;
			ulong input1 = union.UIntValue;
			union.SingleValue = y;
			input1 |= (union.UIntValue << 32);

			union.SingleValue = z;
			ulong input2 = union.UIntValue;
			union.SingleValue = w;
			input2 |= (union.UIntValue << 32);

			return Hash64(input1, input2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Hash32(float x, float y)
		{
			return HashUtil.XorFold64To32(Hash64(x, y));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Hash32(float x, float y, float z)
		{
			var hasher = Hasher.Create();
			SingleUIntUnion union;
			union.UIntValue = 0;
			
			union.SingleValue = x;
			ulong input1 = union.UIntValue;
			union.SingleValue = y;
			input1 |= (union.UIntValue << 32);
			hasher.AddAligned64(input1);

			union.SingleValue = z;
			hasher.Add(union.UIntValue);
			
			return hasher.Finalize32();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Hash32(float x, float y, float z, float w)
		{
			SingleUIntUnion union;
			union.UIntValue = 0;

			union.SingleValue = x;
			ulong input1 = union.UIntValue;
			union.SingleValue = y;
			input1 |= (union.UIntValue << 32);

			union.SingleValue = z;
			ulong input2 = union.UIntValue;
			union.SingleValue = w;
			input2 |= (union.UIntValue << 32);

			return Hash32(input1, input2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Hash32(ulong one, ulong two)
		{
			return HashUtil.XorFold64To32(Hash64(one, two));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong Hash64(ulong one, ulong two)
		{
			// inlined partial murmur hash
			const ulong M = 0xc6a4a7935bd1e995ul;
			ulong hash = M;

			one *= M;
			one ^= one >> 47;
			one *= M;
			hash ^= one;
			hash *= M;

			two *= M;
			two ^= two >> 47;
			two *= M;
			hash ^= two;
			hash *= M;

			// finalize
			hash ^= hash >> 47;
			hash *= M;
			hash ^= hash >> 47;

			return hash;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint XorFold64To32(ulong value)
		{
			unchecked
			{
				return (uint)value ^ (uint)(value >> 32);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort XorFold64To16(ulong value)
		{
			return XorFold32To16(XorFold64To32(value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort XorFold32To16(uint value)
		{
			unchecked
			{
				return (ushort)(value ^ (value >> 16));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte XorFold32To8(uint value)
		{
			unchecked
			{
				return (byte)(value ^ (value >> 8) ^ (value >> 16) ^ (value >> 24));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort XorFold16To8(ushort value)
		{
			unchecked
			{
				return (byte)(value ^ (byte)(value >> 8));
			}
		}

		// weyl constants
		private const uint c_WEYLW0 = 0x3504f333u;   // 3*2309*128413 
		private const uint c_WEYLW1 = 0xf1bbcdcbu;   // 7*349*1660097 
		private const uint c_WEYLM = 741103597u;    // 13*83*686843

		/// <summary>
		/// Weyl hash
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Hash32(uint x, uint y)
		{
			x *= c_WEYLW0;
			y *= c_WEYLW1;
			x ^= y;
			x *= c_WEYLM;
			return x;
		}

		public static uint DJBa2(ReadOnlySpan<byte> bytes)
		{
			uint hash = 5381u;
			for (int i = 0; i < bytes.Length; i++)
				hash = ((hash << 5) + hash) ^ bytes[i];
			return hash;
		}

		public static uint FNV1aLower(ReadOnlySpan<char> str)
		{
			unchecked
			{
				uint retval = 2166136261u;
				for (int i = 0; i < str.Length; i++)
					retval = (retval ^ (uint)char.ToLower(str[i])) * 16777619u;
				return retval;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FNV1a(ReadOnlySpan<char> str)
		{
			unchecked
			{
				uint retval = 2166136261u;
				for (int i = 0; i < str.Length; i++)
					retval = (retval ^ (uint)str[i]) * 16777619u;
				return retval;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong FNV1a64(ReadOnlySpan<byte> data)
		{
			unchecked
			{
				ulong retval = 14695981039346656037ul;
				for (int i = 0; i < data.Length; i++)
					retval = (retval ^ data[i]) * 1099511628211ul;
				return retval;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong FNV1a64(ReadOnlySpan<char> str)
		{
			unchecked
			{
				ulong retval = 14695981039346656037ul;
				for (int i = 0; i < str.Length; i++)
					retval = (retval ^ (uint)str[i]) * 1099511628211ul;
				return retval;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong FNV1a64Lower(ReadOnlySpan<char> str)
		{
			unchecked
			{
				ulong retval = 14695981039346656037ul;
				for (int i = 0; i < str.Length; i++)
					retval = (retval ^ (uint)char.ToLower(str[i])) * 1099511628211ul;
				return retval;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FNV1a(ReadOnlySpan<byte> data)
		{
			unchecked
			{
				uint retval = 2166136261u;
				for (int i = 0; i < data.Length; i++)
					retval = (retval ^ (uint)data[i]) * 16777619u;
				return retval;
			}
		}
	}
}
