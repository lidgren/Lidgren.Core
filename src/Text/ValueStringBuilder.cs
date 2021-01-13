#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Replacement for StringBuilder but it's a value type and takes spans
	/// </summary>
	public struct ValueStringBuilder
	{
		private static readonly bool s_crlf = Environment.NewLine.Equals("\n", StringComparison.Ordinal) ? false : true;
		private char[] m_buffer;
		private int m_length;

		public readonly int Length => m_length;
		public readonly Span<char> Span => m_buffer.AsSpan(0, m_length);
		public readonly ReadOnlySpan<char> ReadOnlySpan => m_buffer.AsSpan(0, m_length);

		public ValueStringBuilder(int initialCapacity)
		{
			m_buffer = new char[initialCapacity];
			m_length = 0;
		}

		public void Clear()
		{
			m_length = 0;
		}

		public int Capacity
		{
			readonly get { return m_buffer.Length; }
			set
			{
				var newBuffer = new char[value];
				m_buffer.AsSpan(0, m_length).CopyTo(newBuffer);
				m_buffer = newBuffer;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int add)
		{
			if (add > m_buffer.Length - m_length)
				Grow(add);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Grow(int add)
		{
			int newSize = Math.Max(m_length + add, m_buffer.Length * 2);
			Capacity = newSize;
		}

		// assumes capacity exists; returns number of chars added
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int NewLine(Span<char> span)
		{
			if (s_crlf)
			{
				span[0] = '\r';
				span[1] = '\n';
				return 2;
			}
			span[0] = '\n';
			return 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine()
		{
			if (m_buffer.Length - m_length < 2)
				Grow(8);
			int len = m_length;
			len += NewLine(m_buffer.AsSpan(len));
			m_length = len;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(string str)
		{
			AppendLine(str.AsSpan());
		}

		public void AppendLine(ReadOnlySpan<char> str)
		{
			if (str.Length == 0)
			{
				AppendLine();
				return;
			}

			var curLen = m_length;

			int ensureSize = str.Length + 2; // +2 for max newline size
			if (ensureSize > m_buffer.Length - m_length)
				Grow(ensureSize);

			var span = m_buffer.AsSpan(curLen);
			str.CopyTo(span);
			span = span.Slice(str.Length);
			var nllen = NewLine(span);
			m_length = curLen + str.Length + nllen;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(string str)
		{
			Append(str.AsSpan());
		}

		public void Append(ReadOnlySpan<char> str)
		{
			if (str.Length == 0)
				return;
			EnsureCapacity(str.Length);
			var span = m_buffer.AsSpan(m_length);
			str.CopyTo(span);
			m_length += str.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(char c)
		{
			EnsureCapacity(1);
			m_buffer[m_length++] = c;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(char c)
		{
			Append(c);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(bool value)
		{
			EnsureCapacity(5);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(bool value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(int value)
		{
			EnsureCapacity(12);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(int value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(uint value)
		{
			EnsureCapacity(12);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(uint value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(byte value)
		{
			EnsureCapacity(4);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(byte value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(short value)
		{
			EnsureCapacity(7);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(short value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ushort value)
		{
			EnsureCapacity(6);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(ushort value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(long value)
		{
			EnsureCapacity(22);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(long value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ulong value)
		{
			EnsureCapacity(22);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(ulong value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(float value)
		{
			EnsureCapacity(24);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(float value)
		{
			Append(value);
			AppendLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(double value)
		{
			EnsureCapacity(24);
			bool ok = value.TryFormat(m_buffer.AsSpan(m_length), out int written);
			CoreException.Assert(ok);
			m_length += written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLine(double value)
		{
			Append(value);
			AppendLine();
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

			for(; ;)
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
			EnsureCapacity(value.Length);
			if (index == m_length)
			{
				Append(value);
				return;
			}

			if (index == 0)
			{
				this.ReadOnlySpan.CopyTo(m_buffer.AsSpan(value.Length));
				value.CopyTo(m_buffer.AsSpan(0, value.Length));
				m_length += value.Length;
				return;
			}

			// in the middle
			var tail = ReadOnlySpan.Slice(index);
			tail.CopyTo(m_buffer.AsSpan(index + value.Length));
			value.CopyTo(m_buffer.AsSpan(index));
			m_length += value.Length;
		}

		public void Remove(int index, int length)
		{
			int tail = m_length - (index + length);
			CoreException.Assert(tail >= 0);
			m_length -= length;
			if (tail == 0)
				return;
			m_buffer.AsSpan(index + length, tail).CopyTo(m_buffer.AsSpan(index, tail));
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
