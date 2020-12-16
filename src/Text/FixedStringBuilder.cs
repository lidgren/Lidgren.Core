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
		private Span<char> m_buffer;
		private int m_length;

		public readonly int Length => m_length;
		public readonly Span<char> Span => m_buffer.Slice(0, m_length);
		public readonly ReadOnlySpan<char> ReadOnlySpan => m_buffer.Slice(0, m_length);

		public FixedStringBuilder(Span<char> buffer)
		{
			m_buffer = buffer;
			m_length = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			m_length = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(ReadOnlySpan<char> str)
		{
			var len = m_length;
			var span = m_buffer.Slice(len);

			str.CopyTo(span);
			span[str.Length] = '\n';

			m_length = len + str.Length + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ReadOnlySpan<char> str)
		{
			var len = m_length;
			var span = m_buffer.Slice(len);
			str.CopyTo(span);
			m_length = len + str.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(char c)
		{
			m_buffer[m_length++] = c;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(bool value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(int value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(uint value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(long value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ulong value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(float value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(double value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, int value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, float value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, double value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, float value, IFormatProvider provider)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format, provider: provider);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, double value, IFormatProvider provider)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format, provider: provider);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, uint value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, ushort value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, ulong value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormat(ReadOnlySpan<char> format, byte value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		/// <summary>
		/// Returns number of characters replaced
		/// </summary>
		public int Replace(char oldChar, char newChar)
		{
			int retval = 0;
			int len = m_length;
			for (int i = 0; i < len; i++)
			{
				if (m_buffer[i] == oldChar)
				{
					m_buffer[i] = newChar;
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
			}
			return retval;
		}

		public void Insert(int index, ReadOnlySpan<char> value)
		{
			if (index == m_length)
			{
				Append(value);
				return;
			}

			if (index == 0)
			{
				this.ReadOnlySpan.CopyTo(m_buffer.Slice(value.Length));
				value.CopyTo(m_buffer.Slice(0, value.Length));
				m_length += value.Length;
				return;
			}

			// in the middle
			var tail = ReadOnlySpan.Slice(index);
			tail.CopyTo(m_buffer.Slice(index + value.Length));
			value.CopyTo(m_buffer.Slice(index));
			m_length += value.Length;
		}

		public void Remove(int index, int length)
		{
			int tail = m_length - (index + length);
			CoreException.Assert(tail >= 0);
			m_length -= length;
			if (tail == 0)
				return;
			m_buffer.Slice(index + length, tail).CopyTo(m_buffer.Slice(index, tail));
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
