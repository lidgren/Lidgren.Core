using System;
using System.Diagnostics;
using System.Threading;
using System.Buffers.Binary;

namespace Lidgren.Core
{
	public static class RandomSeed
	{
		private static volatile int s_seedIncrement = 997;

		// default seed for Random() is Environment.TickCount... but we can do better.
		private static readonly Random s_systemRandom = new Random((int)HashUtil.XorFold64To32((ulong)Stopwatch.GetTimestamp()));

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
				ulong v1 = guidLow ^ guidHigh;

				// xor bits from system random
				s_systemRandom.NextBytes(span);
				ulong rndLow = BinaryPrimitives.ReadUInt64LittleEndian(span);
				ulong rndHigh = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(8));
				ulong v2 = (rndLow ^ rndHigh);

				// add time as source of randomness as well
				v1 ^= (((ulong)Environment.TickCount << 30) ^ (ulong)Stopwatch.GetTimestamp());

				// ensure rapid invocations differ
				v2 ^= ((ulong)Interlocked.Add(ref s_seedIncrement, s_seedIncrement) * 7199369ul);

#if NET5_0_OR_GREATER
				// add processid in case multiple process is running concurrently
				v1 ^= (ulong)Environment.ProcessId << 16;
#endif

				// merge results
				ulong result = v1 ^ v2;

				// make sure at least ONE bit is set to ensure the seed is never zero
				// which is a very problematic numbers for some PRNGs
				if (result == 0)
					return GetUInt64();

				return result;
			}
		}

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
