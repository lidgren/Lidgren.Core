using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace Lidgren.Core
{
	public static class FileCompare
	{
		public enum Result
		{
			/// <summary>
			/// The files have identical contents
			/// </summary>
			Identical,

			/// <summary>
			/// The same file was passed in both arguments
			/// </summary>
			SameFile,

			/// <summary>
			/// The files have different contents
			/// </summary>
			Different,

			/// <summary>
			/// File1 does not exist
			/// </summary>
			File1Missing,

			/// <summary>
			/// File2 does not exist
			/// </summary>
			File2Missing,

			/// <summary>
			/// Neither File1 or File2 exists
			/// </summary>
			BothFilesMissing,
		}

		private const int kDefaultBufferSize = 1024 * 16;

		/// <summary>
		/// Compare content of two files; buffers must be equal length and multiple of 8
		/// </summary>
		public static Result AreEqual(string filename1, string filename2, int bufSize = kDefaultBufferSize)
		{
			var retval = CompareByFileName(filename1, filename2, out bool gotResult, out FileInfo f1, out FileInfo f2);
			if (gotResult)
				return retval;
			using (var fs1 = f1.OpenRead())
			{
				using (var fs2 = f2.OpenRead())
				{
					return AreEqual(fs1, fs2, bufSize);
				}
			}
		}

		/// <summary>
		/// Compare content of two files; buffers must be equal length and multiple of 8
		/// </summary>
		public static Result AreEqual(string filename1, string filename2, Span<byte> buf1, Span<byte> buf2)
		{
			var retval = CompareByFileName(filename1, filename2, out bool gotResult, out FileInfo f1, out FileInfo f2);
			if (gotResult)
				return retval;
			using (var fs1 = f1.OpenRead())
			{
				using (var fs2 = f2.OpenRead())
				{
					return AreEqual(fs1, fs2, buf1, buf2);
				}
			}
		}

		/// <summary>
		/// Compare content of two streams; bufSize must be multiple of 8
		/// </summary>
		public static Result AreEqual(Stream fs1, Stream fs2, int bufSize = kDefaultBufferSize)
		{
			var pool = ArrayPool<byte>.Shared;
			byte[] buffer = null;
			try
			{
				buffer = pool.Rent(bufSize * 2);
				return AreEqual(fs1, fs2, buffer.AsSpan(0, bufSize), buffer.AsSpan(bufSize));
			}
			finally
			{
				if (buffer != null)
					pool.Return(buffer);
			}
		}

		/// <summary>
		/// Compare content of two streams; buffers must be equal length and multiple of 8
		/// </summary>
		public static Result AreEqual(Stream fs1, Stream fs2, Span<byte> buf1, Span<byte> buf2)
		{
			CoreException.Assert(buf1.Length == buf2.Length);
			CoreException.Assert((buf1.Length & 7) == 0);

			ReadOnlySpan<ulong> ulBuf1 = MemoryMarshal.Cast<byte, ulong>(buf1);
			ReadOnlySpan<ulong> ulBuf2 = MemoryMarshal.Cast<byte, ulong>(buf2);

			for (; ; )
			{
				int len = fs1.Read(buf1);
				if (len < 1)
					return Result.Identical;

				int len2 = fs2.Read(buf2.Slice(0, len));

				if (len != len2)
					CoreException.Throw("Failed to read part of file");

				if (len == buf1.Length)
				{
					// compare ulongs
					if (ulBuf1.SequenceEqual(ulBuf2) == false)
						return Result.Different;
				}
				else
				{
					// compare bytes
					if (buf1.Slice(0, len).SequenceEqual(buf2.Slice(0, len)) == false)
						return Result.Different;
				}
			}
		}

		private static Result CompareByFileName(string filename1, string filename2, out bool gotResult, out FileInfo f1, out FileInfo f2)
		{
			bool m1 = string.IsNullOrEmpty(filename1);
			bool m2 = string.IsNullOrEmpty(filename2);
			if (m1)
			{
				f1 = f2 = null;
				gotResult = true;
				return m2 ? Result.BothFilesMissing : Result.File1Missing;
			}
			if (m2)
			{
				f1 = f2 = null;
				gotResult = true;
				return Result.File2Missing;
			}

			// check existence and file sizes
			f1 = new FileInfo(filename1);
			f2 = new FileInfo(filename2);

			if (f1.Exists == false)
			{
				gotResult = true;
				return (f2.Exists == false) ? Result.BothFilesMissing : Result.File1Missing;
			}
			if (f2.Exists == false)
			{
				gotResult = true;
				return Result.File2Missing;
			}

			if (f1.FullName.Equals(f2.FullName, StringComparison.Ordinal))
			{
				gotResult = true;
				return Result.SameFile;
			}

			gotResult = f1.Length != f2.Length;
			return Result.Different;
		}
	}
}

