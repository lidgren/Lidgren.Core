using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	[StructLayout(LayoutKind.Explicit)]
	public struct BoolUIntUnion
	{
		/// <summary>
		/// Value as a boolean
		/// </summary>
		[FieldOffset(0)]
		public bool BoolValue;

		/// <summary>
		/// Value as an unsigned 32 bit integer
		/// </summary>
		[FieldOffset(0)]
		public uint UIntValue;

		public BoolUIntUnion(bool value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			UIntValue = 0;
#endif
			BoolValue = value;
		}

		public BoolUIntUnion(uint value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			BoolValue = false;
#endif
			UIntValue = value & 1u; // don't trust the top bits
		}

		public static uint ReinterpretCast(bool value)
		{
			return (new BoolUIntUnion(value)).UIntValue;
		}

		public static bool ReinterpretCast(uint value)
		{
			return (new BoolUIntUnion(value)).BoolValue;
		}
	}

	/// <summary>
	/// Utility struct for writing Singles
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct SingleUIntUnion
	{
		/// <summary>
		/// Value as a 32 bit float
		/// </summary>
		[FieldOffset(0)]
		public float SingleValue;

		/// <summary>
		/// Value as an unsigned 32 bit integer
		/// </summary>
		[FieldOffset(0)]
		public uint UIntValue;

		private SingleUIntUnion(float value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			UIntValue = 0;
#endif
			SingleValue = value;
		}

		private SingleUIntUnion(uint value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			SingleValue = 0;
#endif
			UIntValue = value;
		}

		public static uint ReinterpretCast(float value)
		{
			return (new SingleUIntUnion(value)).UIntValue;
		}

		public static float ReinterpretCast(uint value)
		{
			return (new SingleUIntUnion(value)).SingleValue;
		}
	}

	/// <summary>
	/// Utility struct for writing Singles
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public ref struct SinglesULongUnion
	{
		[FieldOffset(0)]
		public float SingleValue0;
		[FieldOffset(4)]
		public float SingleValue1;

		/// <summary>
		/// Value as an unsigned 64 bit integer
		/// </summary>
		[FieldOffset(0)]
		public ulong ULongValue;
	}

	/// <summary>
	/// Utility struct for writing Singles
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public ref struct SingleBytesUnion
	{
		/// <summary>
		/// Value as a 32 bit float
		/// </summary>
		[FieldOffset(0)]
		public float SingleValue;

		[FieldOffset(0)]
		public byte B0;
		[FieldOffset(1)]
		public byte B1;
		[FieldOffset(2)]
		public byte B2;
		[FieldOffset(3)]
		public byte B3;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct DoubleULongUnion
	{
		/// <summary>
		/// Value as a 64 bit double
		/// </summary>
		[FieldOffset(0)]
		public double DoubleValue;

		/// <summary>
		/// Value as an unsigned 64 bit integer
		/// </summary>
		[FieldOffset(0)]
		public ulong UlongValue;

		public DoubleULongUnion(double value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			UlongValue = 0;
#endif
			DoubleValue = value;
		}

		public DoubleULongUnion(ulong value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			DoubleValue = 0;
#endif
			UlongValue = value;
		}

		public static ulong ReinterpretCast(double value)
		{
			return (new DoubleULongUnion(value)).UlongValue;
		}

		public static double ReinterpretCast(ulong value)
		{
			return (new DoubleULongUnion(value)).DoubleValue;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct TwoSinglesULongUnion
	{
		/// <summary>
		/// Value as a 64 bit double
		/// </summary>
		[FieldOffset(0)]
		public ulong UlongValue;

		[FieldOffset(0)]
		public float F0;

		[FieldOffset(4)]
		public float F1;

		public TwoSinglesULongUnion(float a, float b)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			UlongValue = 0;
#endif
			F0 = a;
			F1 = b;
		}

		public TwoSinglesULongUnion(ulong value)
		{
#if NET5_0_OR_GREATER
			Unsafe.SkipInit(out this);
#else
			F0 = 0;
			F1 = 0;
#endif
			UlongValue = value;
		}

		public static ulong ReinterpretCast(float a, float b)
		{
			return (new TwoSinglesULongUnion(a, b)).UlongValue;
		}

		public static (float, float) ReinterpretCast(ulong value)
		{
			var val = new TwoSinglesULongUnion(value);
			return (val.F0, val.F1);
		}
	}
}
