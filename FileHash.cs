using System;
using System.Buffers;
using System.IO;

namespace Lidgren.Core
{
	/// <summary>
	/// Hash files using Hasher
	/// </summary>
	public static class FileHash
	{
		public static ulong Hash(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				return Hash(fs);
		}

		public static ulong Hash(ReadOnlySpan<byte> bytes)
		{
			var hasher = Hasher.Create();
			hasher.Add(bytes);
			return hasher.Finalize64();
		}

		public static ulong Hash(Stream stream)
		{
			// read 4k at a time
			const int bufSize = 1024 * 4;
			var backing = ArrayPool<byte>.Shared.Rent(bufSize);
			var buffer = backing.AsSpan(0, bufSize);
			var hasher = Hasher.Create();
			for (; ; )
			{
				int numBytes = stream.Read(buffer);
				if (numBytes > 0)
					hasher.Add(buffer.Slice(0, numBytes));
				else
					break;
			}
			return hasher.Finalize64();
		}

		public static ulong Hash(Stream stream, out int bytesHashed)
		{
			// read 4k at a time
			const int bufSize = 1024 * 4;
			var backing = ArrayPool<byte>.Shared.Rent(bufSize);
			var buffer = backing.AsSpan(0, bufSize);
			var hasher = Hasher.Create();
			bytesHashed = 0;
			for (; ; )
			{
				int numBytes = stream.Read(buffer);
				if (numBytes > 0)
				{
					bytesHashed += numBytes;
					hasher.Add(buffer.Slice(0, numBytes));
				}
				else
					break;
			}
			return hasher.Finalize64();
		}
	}
}
