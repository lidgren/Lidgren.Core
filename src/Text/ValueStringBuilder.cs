#nullable enable
using System;

namespace Lidgren.Core
{
	/// <summary>
	/// Stack allocated string builder backed by (potentially) stack based memory
	/// 
	/// Usage:
	/// Span<char> backing = stackalloc char[64];
	/// var bdr = new ValueStringBuilder(backing);
	/// bdr.Append(...
	/// ... then use .Result or .ToString() to get results
	/// </summary>
	public ref struct ValueStringBuilder
	{
		private Span<char> m_buffer;
		private int m_length;

		public ReadOnlySpan<char> Result { get { return m_buffer.Slice(0, m_length); } }

		public ValueStringBuilder(Span<char> buffer)
		{
			m_buffer = buffer;
			m_length = 0;
		}

		public void Append(ReadOnlySpan<char> str)
		{
			str.CopyTo(m_buffer.Slice(m_length));
			m_length += str.Length;
		}

		public void Append(char c)
		{
			m_buffer[m_length++] = c;
		}

		public void Append(bool value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void Append(int value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void Append(uint value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void Append(long value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void Append(ulong value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void Append(float value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void Append(double value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, int value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, float value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, double value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, float value, IFormatProvider provider)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format, provider: provider);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, double value, IFormatProvider provider)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format, provider: provider);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, uint value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, ushort value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, ulong value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public void AppendFormat(ReadOnlySpan<char> format, byte value)
		{
			bool ok = value.TryFormat(m_buffer.Slice(m_length), out int written, format: format);
			CoreException.Assert(ok);
			m_length += written;
		}

		public override int GetHashCode()
		{
			return (int)HashUtil.Hash32(Result);
		}

		public override string ToString()
		{
			return Result.ToString();
		}
	}
}
