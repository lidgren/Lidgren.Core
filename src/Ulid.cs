#nullable enable
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Alternative to Guid; see https://github.com/ulid/spec
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Ulid : IEquatable<Ulid>, IComparable<Ulid>
	{
		public const int SizeInBytes = 16;

		public static readonly Ulid Empty = new Ulid(0, 0);

		private static readonly long s_timeOffset = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (long)TimeService.TicksToMilliSeconds(Stopwatch.GetTimestamp());

		public readonly ulong Low;
		public readonly ulong High;

		private const string k_encode = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
		private static readonly byte[] s_decode = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 255, 255, 255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 15, 16, 17, 255, 18, 19, 255, 20, 21, 255, 22, 23, 24, 25, 26, 255, 27, 28, 29, 30, 31, 255, 255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 15, 16, 17, 255, 18, 19, 255, 20, 21, 255, 22, 23, 24, 25, 26, 255, 27, 28, 29, 30, 31 };

		public Ulid(ulong low, ulong high)
		{
			Low = low;
			High = high;
		}

		public Ulid(ReadOnlySpan<byte> fromBytes)
		{
			CoreException.Assert(fromBytes.Length >= 16);
			High = BinaryPrimitives.ReadUInt64LittleEndian(fromBytes.Slice(0, 8));
			Low = BinaryPrimitives.ReadUInt64LittleEndian(fromBytes.Slice(8, 8));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ulid Create()
		{
			var time = s_timeOffset + TimeService.TicksToMilliSeconds(Stopwatch.GetTimestamp());
			return new Ulid(PRNG.NextUInt64(), ((ulong)time << 16) | (PRNG.NextUInt64() & 0xFFFF));
		}

		public static Ulid Create(ref ulong rndState)
		{
			var time = s_timeOffset + TimeService.TicksToMilliSeconds(Stopwatch.GetTimestamp());
			return new Ulid(PRNG.NextUInt64(ref rndState), ((ulong)time << 16) | (PRNG.NextUInt64(ref rndState) & 0xFFFF));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ulid Create(in DateTimeOffset dto)
		{
			var time = dto.ToUnixTimeMilliseconds();
			return new Ulid(PRNG.NextUInt64(), ((ulong)time << 16) | (PRNG.NextUInt64() & 0xFFFF));
		}

		public static Ulid Create(in DateTimeOffset dto, ref ulong rndState)
		{
			var time = dto.ToUnixTimeMilliseconds();
			return new Ulid(PRNG.NextUInt64(ref rndState), ((ulong)time << 16) | (PRNG.NextUInt64(ref rndState) & 0xFFFF));
		}

		public static Ulid Parse(ReadOnlySpan<char> fromString)
		{
			CoreException.Assert(fromString.Length >= 26);
#if DEBUG
			for (int i = 0; i < 26; i++)
			{
				var c = fromString[i];
				if (s_decode[c] == 255)
					throw new Exception("Bad char in Ulid string: " + c);
			}
#endif
			ulong high =
				(ulong)s_decode[fromString[0]] << 61 |
				((ulong)s_decode[fromString[1]] << 56) |
				((ulong)s_decode[fromString[2]] << 51) |
				((ulong)s_decode[fromString[3]] << 46) |
				((ulong)s_decode[fromString[4]] << 41) |
				((ulong)s_decode[fromString[5]] << 36) |
				((ulong)s_decode[fromString[6]] << 31) |
				((ulong)s_decode[fromString[7]] << 26) |
				((ulong)s_decode[fromString[8]] << 21) |
				((ulong)s_decode[fromString[9]] << 16) |
				((ulong)s_decode[fromString[10]] << 11) |
				((ulong)s_decode[fromString[11]] << 6) |
				((ulong)s_decode[fromString[12]] << 1);

			ulong straddle = s_decode[fromString[13]];
			high |= (straddle >> 4); // 1 bit from straddle to high

			ulong low = (ulong)straddle << 60 | // keep 4 bits from straddle
				((ulong)s_decode[fromString[14]] << 55) |
				((ulong)s_decode[fromString[15]] << 50) |
				((ulong)s_decode[fromString[16]] << 45) |
				((ulong)s_decode[fromString[17]] << 40) |
				((ulong)s_decode[fromString[18]] << 35) |
				((ulong)s_decode[fromString[19]] << 30) |
				((ulong)s_decode[fromString[20]] << 25) |
				((ulong)s_decode[fromString[21]] << 20) |
				((ulong)s_decode[fromString[22]] << 15) |
				((ulong)s_decode[fromString[23]] << 10) |
				((ulong)s_decode[fromString[24]] << 5) |
				((ulong)s_decode[fromString[25]]);

			return new Ulid(low, high);
		}

		public void AsString(Span<char> into)
		{
			into[0] = k_encode[(int)(High >> 61)];
			into[1] = k_encode[(int)((High >> 56) & 0b11111)];
			into[2] = k_encode[(int)((High >> 51) & 0b11111)];
			into[3] = k_encode[(int)((High >> 46) & 0b11111)];
			into[4] = k_encode[(int)((High >> 41) & 0b11111)];
			into[5] = k_encode[(int)((High >> 36) & 0b11111)];
			into[6] = k_encode[(int)((High >> 31) & 0b11111)];
			into[7] = k_encode[(int)((High >> 26) & 0b11111)];
			into[8] = k_encode[(int)((High >> 21) & 0b11111)];
			into[9] = k_encode[(int)((High >> 16) & 0b11111)];
			into[10] = k_encode[(int)((High >> 11) & 0b11111)];
			into[11] = k_encode[(int)((High >> 6) & 0b11111)];
			into[12] = k_encode[(int)((High >> 1) & 0b11111)];

			var straddle = ((High & 0b1) << 4) | ((Low >> 60) & 0b1111);
			into[13] = k_encode[(int)straddle];

			into[14] = k_encode[(int)((Low >> 55) & 0b11111)];
			into[15] = k_encode[(int)((Low >> 50) & 0b11111)];
			into[16] = k_encode[(int)((Low >> 45) & 0b11111)];
			into[17] = k_encode[(int)((Low >> 40) & 0b11111)];
			into[18] = k_encode[(int)((Low >> 35) & 0b11111)];
			into[19] = k_encode[(int)((Low >> 30) & 0b11111)];
			into[20] = k_encode[(int)((Low >> 25) & 0b11111)];
			into[21] = k_encode[(int)((Low >> 20) & 0b11111)];
			into[22] = k_encode[(int)((Low >> 15) & 0b11111)];
			into[23] = k_encode[(int)((Low >> 10) & 0b11111)];
			into[24] = k_encode[(int)((Low >> 5) & 0b11111)];
			into[25] = k_encode[(int)(Low & 0b11111)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AsBytes(Span<byte> into)
		{
			into[0] = (byte)High;
			into[1] = (byte)(High >> 8);
			into[2] = (byte)(High >> 16);
			into[3] = (byte)(High >> 24);
			into[4] = (byte)(High >> 32);
			into[5] = (byte)(High >> 40);
			into[6] = (byte)(High >> 48);
			into[7] = (byte)(High >> 56);
			into[8] = (byte)Low;
			into[9] = (byte)(Low >> 8);
			into[10] = (byte)(Low >> 16);
			into[11] = (byte)(Low >> 24);
			into[12] = (byte)(Low >> 32);
			into[13] = (byte)(Low >> 40);
			into[14] = (byte)(Low >> 48);
			into[15] = (byte)(Low >> 56);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetTime(out DateTimeOffset time)
		{
			time = DateTimeOffset.FromUnixTimeMilliseconds((long)(High >> 16));
		}

		public override string ToString()
		{
			return String.Create<Ulid>(26, this, (span, ulid) =>
			{
				ulid.AsString(span);
			});
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? other)
		{
			if (other is null || !(other is Ulid))
				return false;
			var oulid = (Ulid)other;
			return oulid.Low == Low && oulid.High == High;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Ulid other)
		{
			return other.Low == Low && other.High == High;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(in Ulid other)
		{
			return other.Low == Low && other.High == High;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in Ulid x, in Ulid y)
		{
			return (x.Low == y.Low && x.High == y.High);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in Ulid x, in Ulid y)
		{
			return (x.Low != y.Low || x.High != y.High);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			// Low is all random bits; can be used verbatim
			// But the time part of High needs to be mixed a bit
			unchecked
			{
				// simple mixing since we're xoring with random bits anyway
				ulong mixed = High;
				mixed ^= mixed >> 47;
				mixed *= 0xc6a4a7935bd1e995ul;
				mixed ^= mixed >> 47;

				ulong hash = mixed ^ Low;

				// xor fold to 32 bits
				return (int)((uint)hash ^ (uint)(hash >> 32));
			}
		}

		public int CompareTo(Ulid other)
		{
			if (High < other.High)
				return -1;
			if (High > other.High)
				return 1;
			if (Low < other.Low)
				return -1;
			if (Low > other.Low)
				return 1;
			return 0;
		}
	}
}
