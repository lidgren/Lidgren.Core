using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	public static class FileCompare
	{
		/// <summary>
		/// Returns true if the contents of the files are binary equal
		/// </summary>
		public static bool AreEqual(string filename1, string filename2)
		{
			return AreEqual(filename1, filename2, 1024 * 32);
		}

		/// <summary>
		/// Returns true if the contents of the files are binary equal
		/// </summary>
		public static bool AreEqual(string filename1, string filename2, int bufSize)
		{
			CoreException.Assert((bufSize & 7) == 0);
			var pool = ArrayPool<byte>.Shared;

			byte[] buf1 = null;
			byte[] buf2 = null;
			try
			{
				buf1 = pool.Rent(bufSize);
				buf2 = pool.Rent(bufSize);
				return AreEqual(filename1, filename2, buf1, buf2);
			}
			finally
			{
				if (buf1 != null)
					pool.Return(buf1);
				if (buf2 != null)
					pool.Return(buf2);
			}
		}

		/// <summary>
		/// Returns true if the contents of the files are binary equal
		/// </summary>
		public static bool AreEqual(string filename1, string filename2, byte[] buf1, byte[] buf2)
		{
			CoreException.Assert(buf1.Length == buf2.Length);
			CoreException.Assert((buf1.Length & 7) == 0);

			if (filename1.Equals(filename2, StringComparison.OrdinalIgnoreCase))
				return true; // same file

			// check file sizes
			var f1 = new FileInfo(filename1);
			var f2 = new FileInfo(filename2);

			if (f1.Exists == false || f2.Exists == false)
				return false; // like NaN, if any of them don't exists - there's no equality

			if (f1.Length != f2.Length)
				return false; // not same size

			using (var fs1 = new FileStream(filename1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var fs2 = new FileStream(filename2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					return AreEqual(fs1, fs2, buf1, buf2);
				}
			}
		}

		/// <summary>
		/// Returns true if the contents of the files are binary equal
		/// </summary>
		public static bool AreEqual(Stream fs1, Stream fs2, byte[] buf1, byte[] buf2)
		{
			CoreException.Assert(buf1.Length == buf2.Length);
			CoreException.Assert((buf1.Length & 7) == 0);

			using (var bfs1 = new BufferedStream(fs1))
			{
				using (var bfs2 = new BufferedStream(fs2))
				{
					ReadOnlySpan<ulong> ulBuf1 = MemoryMarshal.Cast<byte, ulong>(buf1);
					ReadOnlySpan<ulong> ulBuf2 = MemoryMarshal.Cast<byte, ulong>(buf2);

					while (true)
					{
						int len = fs1.Read(buf1, 0, buf1.Length);
						if (len < 1)
							return true;

						int len2 = fs2.Read(buf2, 0, len);

						if (len != len2)
							CoreException.Throw("Failed to read part of file");

						if (len == buf1.Length)
						{
							// compare ulongs
							if (ulBuf1.SequenceEqual(ulBuf2) == false)
								return false;
						}
						else
						{
							// compare bytes
							if (buf1.AsSpan(0, len).SequenceEqual(buf2.AsSpan(0, len)) == false)
								return false;
						}
					}
				}
			}
		}
	}
}
