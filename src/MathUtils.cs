using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public static class MathUtils
	{
		public const float E = 2.718282f;
		public const float Log10E = 0.4342945f;
		public const float Log2E = 1.442695f;
		public const float Pi = 3.141592653589793239f;
		public const float HalfPi = 1.570796326794896619f;
		public const float QuarterPi = 0.785398163397448310f;
		public const float TwoPi = 6.283185307179586f;
		public const float InvTwoPi = (float)(1.0 / 6.283185307179586);
		public const float PiTimesPi = (float)(3.141592653589793239 * 3.141592653589793239);

		public static readonly int[] Primes = {
		3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
		1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
		17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
		187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
		1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };

		/// <summary>
		/// Branchless sign; returns 0 if value < 0 and 1 if value >= 0
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int IsPositive(int v)
		{
			return (int)(1u ^ ((uint)v >> (int)31)); // if v < 0 then 0, else 1
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Max(int x, int y)
		{
			// return a > b ? a : b;
			int z = x - y;
			return x - ((z >> 31) & z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Max(float a, float b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Max(float a, float b, float c)
		{
			return a > b ? (a > c ? a : c) : (b > c ? b : c);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Max(double a, double b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Min(int a, int b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Min(uint a, uint b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Min(float a, float b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Min(double a, double b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPowerOfTwo(int x)
		{
			return ((x & (x - 1)) == 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int NextPowerOfTwo(int x)
		{
			x--;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			x++;
			return x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Clamp(double value, double min, double max)
		{
			return (value > max) ? max : (value < min) ? min : value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(float value, float min, float max)
		{
			return (value > max) ? max : (value < min) ? min : value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(int value, int min, int max)
		{
			return (value > max) ? max : (value < min) ? min : value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmoothStep(float value1, float value2, float amount)
		{
			float num = MathUtils.Clamp(amount, 0f, 1f);
			return Interpolate(value1, value2, (num * num) * (3f - (2f * num)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double SmoothStep(double value1, double value2, double amount)
		{
			var num = Clamp(amount, 0.0, 1.0);
			return Interpolate(value1, value2, (num * num) * (3f - (2f * num)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmoothStep(float amount)
		{
			float num = MathUtils.Clamp(amount, 0f, 1f);
			return (num * num) * (3f - (2f * num));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double SmoothStep(double amount)
		{
			var num = Clamp(amount, 0.0, 1.0);
			return (num * num) * (3.0 - (2.0 * num));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double SmootherStep(double amount)
		{
			var num = Clamp(amount, 0.0, 1.0);
			return num * num * num * (num * (num * 6.0 - 15.0) + 10.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Interpolate(float a, float b, float t)
		{
			return MathF.FusedMultiplyAdd(a, (1 - t), (b * t));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Interpolate(byte a, byte b, float t)
		{
			return (byte)(a * (1 - t) + b * t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Interpolate(double a, double b, double t)
		{
			return (a * (1 - t) + b * t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double DegreesToRadians(double degrees)
		{
			//return (Math.PI / 180.0) * degrees;
			return 0.017453292519943295 * degrees;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DegreesToRadians(float degrees)
		{
			//return (float)(Math.PI / 180.0) * degrees;
			return (float)(0.017453292519943295 * degrees);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RadiansToDegrees(float radians)
		{
			return (float)(57.295779513082320876798154814105 * radians);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double RadiansToDegrees(double radians)
		{
			return 57.295779513082320876798154814105 * radians;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToNearestMultiple(int n, int multiple)
		{
			return ((n + multiple - 1) / multiple) * multiple;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleToSlope(float degrees)
		{
			return MathF.Tan(MathUtils.DegreesToRadians(degrees));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double AngleToSlope(double degrees)
		{
			return Math.Tan(MathUtils.DegreesToRadians(degrees));
		}

		/// <summary>
		/// return x % y but y must be power of two
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RemainderByPowerOfTwo(int x, int y)
		{
			return x & (y - 1);
		}

		/// <summary>
		/// return x % y but y must be power of two
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long RemainderByPowerOfTwo(long x, long y)
		{
			return x & (y - 1);
		}

		/// <summary>
		/// Floored division modulo (not just remainder)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Modulo(double value, double mod)
		{
			var m = value % mod;
			return ((mod * m) >= 0.0) ? m : m + mod;
		}

		/// <summary>
		/// Floored division modulo (not just remainder)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Modulo(float value, float mod)
		{
			var m = value % mod;
			return ((mod * m) >= 0.0f) ? m : m + mod;
		}

		/// <summary>
		/// Floored division modulo (not just remainder)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Modulo(int value, int mod)
		{
			var m = value % mod;
			return ((mod * m) >= 0) ? m : m + mod;
		}

		/// <summary>
		/// population standard deviation
		/// </summary>
		public static float StandardDeviation(ReadOnlySpan<float> values)
		{
			double avg = 0.0;
			foreach (var val in values)
				avg += val;
			avg = avg / (double)values.Length;

			double sqrdAvg = 0.0;
			foreach (var val in values)
			{
				var delta = val - avg;
				sqrdAvg += (delta * delta);
			}
			return MathF.Sqrt((float)sqrdAvg / (float)values.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FasterSin(float radians)
		{
			CoreException.Assert(radians >= -MathUtils.Pi && radians <= MathUtils.Pi);
			if (radians < 0)
				return radians * (1.27323954f + 0.405284735f * radians);
			else
				return radians * (1.27323954f - 0.405284735f * radians);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FastSin(float radians)
		{
			//var ranged = x - MathUtils.TwoPi * MathF.Floor((x + MathUtils.Pi) / MathUtils.TwoPi);
			const float negTwoPi = -MathUtils.TwoPi;
			var pre = MathF.Floor((radians + MathUtils.Pi) * MathUtils.InvTwoPi);
			var ranged = MathF.FusedMultiplyAdd(negTwoPi, pre, radians);

			var xa = (MathUtils.Pi - ranged) * ranged;
			var over = 16 * xa;
			var under = MathF.FusedMultiplyAdd(5, MathUtils.PiTimesPi, (xa * -4));
			return over / under;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FastCos(float radians)
		{
			// cos(x) = sin(x + PI/2)
			return FastSin(radians + MathUtils.HalfPi);
		}

		public static bool IsPrime(int candidate)
		{
			if ((candidate & 1) != 0)
			{
				int num = (int)Math.Sqrt((double)candidate);
				for (int i = 3; i <= num; i += 2)
				{
					if (candidate % i == 0)
						return false;
				}
				return true;
			}
			return candidate == 2;
		}

		public static int GetPrime(int min)
		{
			for (int i = 0; i < Primes.Length; i++)
			{
				int num = Primes[i];
				if (num >= min)
					return num;
			}

			for (int j = min | 1; j < int.MaxValue; j += 2)
			{
				if (IsPrime(j) && (j - 1) % 101 != 0)
					return j;
			}
			return min;
		}

		public static int ExpandPrime(int oldSize)
		{
			int num = 2 * oldSize;
			if ((uint)num > 2146435069u && 2146435069 > oldSize)
			{
				return 2146435069;
			}
			return GetPrime(num);
		}

		/// <summary>
		/// Reduce a floating point value; so that its base-10 representation is as few decimals as possible
		/// </summary>
		public static float CropMantissa(float value, int bits)
		{
			SingleUIntUnion un;
			un.UIntValue = 0;
			un.SingleValue = value;

			// reduce by one bit at a time
			uint signAndExponent = un.UIntValue & 0b11111111_10000000_00000000_00000000u;
			uint mantissa = un.UIntValue & 0b00000000_01111111_11111111_11111111u;

			uint mask = ~0u << bits;
			mantissa = mantissa & mask;

			un.UIntValue = signAndExponent | mantissa;
			return un.SingleValue;
		}
	}
}
