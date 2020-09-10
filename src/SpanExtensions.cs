using System;

namespace Lidgren.Core
{
	public static class SpanExtensions
	{
		/// <summary>
		/// Swap places of two ranges of a span, in-place
		/// Example:
		///   (pivot 4) 1234BBB becomes BBB1234
		///   (pivot 2) AABBBBB becomes BBBBBAA
		/// </summary>
		public static void SwapBlocks<T>(this Span<T> span, int pivot) where T : struct
		{
			span.Slice(0, pivot).Reverse();
			span.Slice(pivot, span.Length - pivot).Reverse();
			span.Reverse();
		}

	}
}
