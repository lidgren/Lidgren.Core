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
