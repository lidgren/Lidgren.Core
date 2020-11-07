#nullable enable
using System;
using System.Diagnostics;

namespace Lidgren.Core
{
	/// <summary>
	/// Console output utility. Supports passing span of char and coloring.
	/// </summary>
	public static class StdOut
	{
		// Console is thread safe; but to ensure coloring is atomic we use a lock
		private static readonly object s_lock = new object();

		/// <summary>
		/// Writes to console
		/// </summary>
		public static void WriteLine(ReadOnlySpan<char> text)
		{
			lock (s_lock)
				Console.Out.WriteLine(text);
		}

		/// <summary>
		/// Writes to console
		/// </summary>
		public static void WriteLine()
		{
			lock (s_lock)
				Console.Out.WriteLine();
		}

		/// <summary>
		/// Writes to console
		/// </summary>
		public static void Write(ReadOnlySpan<char> text)
		{
			lock (s_lock)
				Console.Out.Write(text);
		}

		/// <summary>
		/// Writes to console, only in DEBUG configuration
		/// </summary>
		[Conditional("DEBUG")]
		public static void DebugLine(ReadOnlySpan<char> text)
		{
			WriteLine(text);
		}

		/// <summary>
		/// Writes to console, only in DEBUG configuration
		/// </summary>
		[Conditional("DEBUG")]
		public static void Debug(ReadOnlySpan<char> text)
		{
			Write(text);
		}

		/// <summary>
		/// Writes to console with a particular foreground color
		/// </summary>
		public static void WriteLine(ReadOnlySpan<char> text, ConsoleColor color)
		{
			lock (s_lock)
			{
				Console.ForegroundColor = color;
				Console.Out.WriteLine(text);
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Writes to console with a particular foreground color
		/// </summary>
		public static void Write(ReadOnlySpan<char> text, ConsoleColor color)
		{
			lock (s_lock)
			{
				Console.ForegroundColor = color;
				Console.Out.Write(text);
				Console.ResetColor();
			}
		}

		private const string k_escape = "\u001b[";
		private const string k_escapeReset = "\u001b[m";
		private const string k_escapeForegroundColor = "\u001b[38;2;";
		private const string k_escapeBackgroundColor = "\u001b[48;2;";
		private const string k_escapeClearScreen = "\u001b[2Jm\u001b[H";

		public static void ClearScreen(ReadOnlySpan<char> hexBackgroundColor = default)
		{
			lock (s_lock)
			{
				if (hexBackgroundColor != default)
				{
					Span<char> buffer = stackalloc char[64];
					Span<char> work = buffer;

					AnsiColorCode(k_escapeBackgroundColor, hexBackgroundColor, ref work);
					var code = buffer.Slice(0, buffer.Length - work.Length);

					Console.Out.Write(code);
				}

				Console.Out.Write(k_escapeClearScreen);
			}
		}

		/// <summary>
		/// Writes to console with a particular foreground color and optionally background color
		/// </summary>
		public static void WriteAnsiLine(ReadOnlySpan<char> text, ReadOnlySpan<char> hexForegroundColor, ReadOnlySpan<char> hexBackgroundColor = default)
		{
			Span<char> buffer = stackalloc char[64];
			Span<char> work = buffer;

			// foreground
			AnsiColorCode(k_escapeForegroundColor, hexForegroundColor, ref work);

			if (hexBackgroundColor != default)
				AnsiColorCode(k_escapeBackgroundColor, hexBackgroundColor, ref work);

			var code = buffer.Slice(0, buffer.Length - work.Length);
			//var debug = code.ToString();

			lock (s_lock)
			{
				Console.Out.Write(code);
				Console.Out.Write(text);
				Console.Out.WriteLine(k_escapeReset);
			}
		}

		private static void AnsiColorCode(ReadOnlySpan<char> codePrefix, ReadOnlySpan<char> hexColor, ref Span<char> work)
		{
			if (hexColor.StartsWith("#"))
				hexColor = hexColor.Slice(1);
			var color = (uint)StringUtils.FromHex(hexColor);
			codePrefix.CopyTo(work);
			work = work.Slice(codePrefix.Length);

			// red
			((color >> 16) & 255).TryFormat(work, out int written);
			work[written] = ';';
			work = work.Slice(written + 1);

			// green
			((color >> 8) & 255).TryFormat(work, out written);
			work[written] = ';';
			work = work.Slice(written + 1);

			// blue
			(color & 255).TryFormat(work, out written);
			work[written] = 'm';
			work = work.Slice(written + 1);

			return;
		}
	}
}
