using System;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Utility struct for writing Singles
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public ref struct SingleUIntUnion
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
	public ref struct DoubleULongUnion
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
		public ulong ULongValue;
	}
}
