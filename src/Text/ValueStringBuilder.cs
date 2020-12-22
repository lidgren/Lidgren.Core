#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Replacement for StringBuilder with differences:
	/// 1. It's a value type
	/// 2. It takes spans
	/// 3. Has indentation support
	/// </summary>
	public struct ValueStringBuilder
	{
		private static readonly bool s_crlf = Environment.NewLine.Equals("\n", StringComparison.Ordinal) ? false : true;
		private static readonly string s_tabs = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";
		private char[] m_buffer;
		private int m_length;

		private bool m_lineHasIndention;
		private int m_indentionLevel;

		public readonly int Length => m_length;
		public readonly Span<char> Span => m_buffer.AsSpan(0, m_length);
		public readonly ReadOnlySpan<char> ReadOnlySpan => m_buffer.AsSpan(0, m_length);

		public ValueStringBuilder(int initialCapacity)
		{
			m_buffer = new char[initialCapacity];
			m_length = 0;
			m_lineHasIndention = false;
			m_indentionLevel = 0;
		}

		public void Clear()
		{
			m_length = 0;
			m_lineHasIndention = false;
			m_indentionLevel = 0;
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
		private void EnsureCapacity(int len)
		{
			if (len > m_buffer.Length - m_length)
				Grow(len);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Grow(int len)
		{
			int newSize = Math.Max(m_length + len, m_buffer.Length * 2);
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
			EnsureCapacity(2);
			int len = m_length;
			len += NewLine(m_buffer.AsSpan(len));
			m_length = len;
			m_lineHasIndention = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Indent(int add)
		{
			m_indentionLevel += add;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Indent(int add, string postAppendLine)
		{
			m_indentionLevel += add;
			AppendLine(postAppendLine);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Indent(string preAppendLine, int add)
		{
			AppendLine(preAppendLine);
			m_indentionLevel += add;
		}

		// remaining MUST have room for m_indentionLevel characters, and span will be modified
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void MaybeIndent(ref Span<char> span)
		{
			if (m_lineHasIndention)
				return;
			var lvl = m_indentionLevel;
			if (lvl == 0)
				return;
			s_tabs.AsSpan(0, lvl).CopyTo(m_buffer.AsSpan(m_length));
			span = span.Slice(lvl);
			m_lineHasIndention = true;
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
			var strLen = str.Length + m_indentionLevel;

			//EnsureCapacity(strLen + 2); // +2 for max newline size
			int ensureSize = strLen + 2; // +2 for max newline size
			if (ensureSize > m_buffer.Length - m_length)
				Grow(ensureSize);

			var span = m_buffer.AsSpan(curLen, strLen + 2); // +2 for max newline size

			MaybeIndent(ref span); // add indention
			str.CopyTo(span); // add str
			span = span.Slice(str.Length);
			var nllen = NewLine(span);

			m_length = curLen + strLen + nllen;
			m_lineHasIndention = false;
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
			var len = str.Length + m_indentionLevel;
			//EnsureCapacity(len);
			if (len > m_buffer.Length - m_length)
				Grow(len);
			var span = m_buffer.AsSpan(m_length, len);
			MaybeIndent(ref span); // add indention
			str.CopyTo(span); // add str
			m_length += len;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(char c)
		{
			var len = 1 + m_indentionLevel;
			EnsureCapacity(len);
			var span = m_buffer.AsSpan(m_length, len);
			MaybeIndent(ref span); // add indention
			span[0] = c;
			m_length += len;
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
			var maxLen = 5 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
			var maxLen = 12 + m_indentionLevel;
			EnsureCapacity(maxLen);

			var span = m_buffer.AsSpan(m_length, maxLen);
			MaybeIndent(ref span); // add indention

			bool ok = value.TryFormat(span, out int written);
			CoreException.Assert(ok);

			var actualLen = m_indentionLevel + written;
			m_length += actualLen;
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
