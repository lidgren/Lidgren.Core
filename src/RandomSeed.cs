using System;
using System.Diagnostics;
using System.Threading;
using System.Buffers.Binary;

namespace Lidgren.Core
{
	public static class RandomSeed
	{
		private static volatile int s_seedIncrement = 997;
		private static readonly Random s_systemRandom = new Random();

		/// <summary>
		/// Generates a 32 bit random seed by combining various factors that are very hard to predict
		/// </summary>
		public static uint GetUInt32()
		{
			unchecked
			{
				var value = GetUInt64();
				return (uint)value ^ (uint)(value >> 32);
			}
		}

		/// <summary>
		/// Generates a 64 bit random seed by combining various factors that are very hard to predict
		/// </summary>
		public static ulong GetUInt64()
		{
			unchecked
			{
				// start with bits from new guid
				Span<byte> span = stackalloc byte[16];
				Guid.NewGuid().TryWriteBytes(span);
				ulong guidLow = BinaryPrimitives.ReadUInt64LittleEndian(span);
				ulong guidHigh = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(8));
				ulong value = guidLow ^ guidHigh;

				// xor bits from system random
				s_systemRandom.NextBytes(span);
				ulong rndLow = BinaryPrimitives.ReadUInt64LittleEndian(span);
				ulong rndHigh = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(8));
				value ^= (rndLow ^ rndHigh);

				// add time as source of randomness as well
				value ^= (((ulong)Environment.TickCount << 32) ^ (ulong)Stopwatch.GetTimestamp());

				// ensure rapid invocations differ
				value ^= ((ulong)Interlocked.Add(ref s_seedIncrement, s_seedIncrement) * 7199369ul);

				return value;
			}
		}

		/// <summary>
		/// Expand 32 bit seed to 64 bit deterministically, ensuring there are bits set in the upper part
		/// </summary>
		public static ulong ExpandSeed(uint seed)
		{
			unchecked
			{
				ulong upper = (ulong)HashUtil.XorFold64To32(seed * 7199369) << 32;
				return ((ulong)seed | upper) ^ 0b11100000_01111101_01000011_11110110_01001111_10100000_01100000_10011011UL;
			}
		}
	}
}
