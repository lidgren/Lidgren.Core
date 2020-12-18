using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Fixed size stack allocated string builder backed by (potentially) stack based memory
	/// 
	/// Usage:
	/// Span<char> backing = stackalloc char[64];
	/// var bdr = new ValueStringBuilder(backing);
	/// bdr.Append(...
	/// ... then use .Result or .ToString() to get results
	///
	/// Will throw exception on overflow.
	/// </summary>
	public ref struct FixedStringBuilder
	{
		private static readonly bool s_crlf = Environment.NewLine.Equals("\n", StringComparison.Ordinal) ? false : true;

		private Span<char> m_buffer;
		private Span<char> m_remaining;
		
		public readonly int Length => m_buffer.Length - m_remaining.Length;
		public readonly Span<char> Span => m_buffer.Slice(0, Length);
		public readonly ReadOnlySpan<char> ReadOnlySpan => m_buffer.Slice(0, Length);

		public FixedStringBuilder(Span<char> buffer)
		{
			m_buffer = buffer;
			m_remaining = buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			m_remaining = m_buffer;
		}

		// advances span
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void NewLine(ref Span<char> span)
		{
			if (s_crlf)
			{
				span[0] = '\r';
				span[1] = '\n';
				span = span.Slice(2);
				return;
			}
			span[0] = '\n';
			span = span.Slice(1);
		}

		public void AppendLine()
		{
			NewLine(ref m_remaining);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(ReadOnlySpan<char> str)
		{
			var rem = m_remaining;
			str.CopyTo(rem);
			rem = rem.Slice(str.Length);
			NewLine(ref rem);
			m_remaining = rem;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ReadOnlySpan<char> str)
		{
			str.CopyTo(m_remaining);
			m_remaining = m_remaining.Slice(str.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(char c)
		{
			m_remaining[0] = c;
			m_remaining = m_remaining.Slice(1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(bool value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(int value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(uint value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(long value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ulong value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(float value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(double value)
		{
			bool ok = value.TryFormat(m_remaining, out int written);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, int value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, float value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, double value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, float value, IFormatProvider provider)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format, provider: provider);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, double value, IFormatProvider provider)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format, provider: provider);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, uint value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, ushort value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, ulong value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, byte value)
		{
			bool ok = value.TryFormat(m_remaining, out int written, format: format);
			CoreException.Assert(ok);
			m_remaining = m_remaining.Slice(written);
		}

		/// <summary>
		/// Returns number of characters replaced
		/// </summary>
		public int Replace(char oldChar, char newChar)
		{
			var content = this.Span;
			int retval = 0;
			for (int i = 0; i < content.Length; i++)
			{
				if (content[i] == oldChar)
				{
					content[i] = newChar;
					retval++;
				}
			}
			return retval;
		}

		/// <summary>
		/// Returns number of substitutions
		/// </summary>
		public int Replace(string oldValue, string newValue)
		{
			int retval = 0;

			if (oldValue.Length == newValue.Length)
			{
				// in-place replacement
				var span = this.Span;
				for (; ; )
				{
					var idx = span.IndexOf(oldValue);
					if (idx == -1)
						break;
					newValue.AsSpan().CopyTo(span.Slice(idx, newValue.Length));
					retval++;
					span = span.Slice(idx + 1); // avoid infinite substitutions
				}
				return retval;
			}

			for (; ; )
			{
				var span = this.Span;
				var idx = span.IndexOf(oldValue);
				if (idx == -1)
					break;
				Remove(idx, oldValue.Length);
				Insert(idx, newValue);
				retval++;
			}
			return retval;
		}

		public void Insert(int index, ReadOnlySpan<char> value)
		{
			var curLen = this.Length;
			if (index == curLen)
			{
				Append(value);
				return;
			}

			if (index == 0)
			{
				this.ReadOnlySpan.CopyTo(m_buffer.Slice(value.Length));
				value.CopyTo(m_buffer.Slice(0, value.Length));
				m_remaining = m_buffer.Slice(curLen + value.Length);
				return;
			}

			// in the middle
			var tail = ReadOnlySpan.Slice(index);
			tail.CopyTo(m_buffer.Slice(index + value.Length));
			value.CopyTo(m_buffer.Slice(index));
			m_remaining = m_buffer.Slice(curLen + value.Length);
		}

		public void Remove(int index, int count)
		{
			int curLen = this.Length;
			int tail = curLen - (index + count);
			CoreException.Assert(tail >= 0);
			m_remaining = m_buffer.Slice(curLen - count);
			if (tail == 0)
				return;
			m_buffer.Slice(index + count, tail).CopyTo(m_buffer.Slice(index, tail));
		}

		public override int GetHashCode()
		{
			return (int)HashUtil.Hash32(ReadOnlySpan);
		}

		public override string ToString()
		{
			return ReadOnlySpan.ToString();
		}
	}
}
