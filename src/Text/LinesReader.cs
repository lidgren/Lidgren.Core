using System;
using System.Buffers;
using System.IO;

namespace Lidgren.Core
{
	/// <summary>
	/// Reads lines without generating strings; splits on \n but omits preceding \r if present
	/// </summary>
	public sealed class LinesReader : IDisposable
	{
		private char[] m_buffer;
		private bool m_bufferCreated;
		private StreamReader m_rdr;

		private int m_start;
		private int m_end; // exclusive

		private bool m_eof;

		public LinesReader(string filename, char[] buffer = null)
		{
			try
			{
				if (buffer is null)
				{
					m_buffer = ArrayPool<char>.Shared.Rent(1024 * 32);
					m_bufferCreated = true;
				}
				else
				{
					m_buffer = buffer;
				}

				var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
				m_rdr = new StreamReader(fs);
			}
			catch
			{
				if (m_rdr != null)
					DisposeUtils.Dispose(ref m_rdr);
				if (m_bufferCreated == true && m_buffer != null)
					ArrayPool<char>.Shared.Return(m_buffer);
				m_buffer = null;
				throw;
			}
		}

		public LinesReader(Stream stream, char[] buffer = null)
		{
			try
			{
				if (buffer is null)
				{
					m_buffer = ArrayPool<char>.Shared.Rent(1024 * 32);
					m_bufferCreated = true;
				}
				else
				{
					m_buffer = buffer;
				}

				m_rdr = new StreamReader(stream);
			}
			catch
			{
				if (m_rdr != null)
					DisposeUtils.Dispose(ref m_rdr);
				if (m_bufferCreated == true && m_buffer != null)
					ArrayPool<char>.Shared.Return(m_buffer);
				m_bufferCreated = false;
				m_buffer = null;
				throw;
			}
		}

		public void Dispose()
		{
			if (m_rdr != null)
				DisposeUtils.Dispose(ref m_rdr);

			if (m_bufferCreated == true)
			{
				var buf = m_buffer;
				m_buffer = null;
				if (buf != null)
					ArrayPool<char>.Shared.Return(buf);
			}
		}

		public bool ReadLine(out ReadOnlySpan<char> line)
		{
			if (m_eof)
			{
				line = default;
				return false;
			}

			// find end of line
			int idx = m_start;
			int end = m_end;
			for (; ; )
			{
				if (idx >= end)
				{
					// end of data, but no newline yet
					int curLen = end - idx;
					int wasRead = ReadBuffer();
					end = m_end;
					if (wasRead == 0)
					{
						// last line; without newline
						line = new ReadOnlySpan<char>(m_buffer, 0, end);
						m_eof = true;
						return end > 0;
					}

					// continue reading
					idx = curLen;
				}

				if (m_buffer[idx] == '\n')
					break;
				idx++;
			}

			int len = idx - m_start;

			// remove \r if \r\n
			if (len > 0 && m_buffer[m_start + len - 1] == '\r')
				line = new ReadOnlySpan<char>(m_buffer, m_start, len - 1);
			else
				line = new ReadOnlySpan<char>(m_buffer, m_start, len);
			m_start += (len + 1); // step past newline char also
			return true;
		}

		private int ReadBuffer()
		{
			int len = m_end - m_start;
			if (len > 0)
			{
				// move back data
				var src = new ReadOnlySpan<char>(m_buffer, m_start, len);
				src.CopyTo(m_buffer);
			}
			m_start = 0;
			m_end = len;

			int rem = m_buffer.Length - len;
			int wasRead = m_rdr.Read(new Span<char>(m_buffer, len, rem));
			m_end += wasRead;
			return wasRead;
		}
	}
}
