#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lidgren.Core
{
	public class ChromeTraceTimingConsumer : IDisposable
	{
		private StreamWriter? m_writer;
		private readonly HashSet<TimingThread> m_seen = new HashSet<TimingThread>(32);

		private double m_invFrequencyToMicros;

		public ChromeTraceTimingConsumer(string filename)
		{
			m_invFrequencyToMicros = 1000000.0 / (double)Stopwatch.Frequency;

			var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
			m_writer = new StreamWriter(fs);
			m_writer.WriteLine("[ { }");
			m_writer.Flush();
			TimingService.AddListener(Flush);
		}

		private const string k_part0 = ",{\"name\":\"";
		private const string k_part1 = "\",\"ph\":\"X\",\"ts\":";
		private const string k_part2 = ",\"dur\":";
		private const string k_part3 = ",\"pid\":0,\"tid\":";

		private void Flush(TimingThread thread, TimingEntry[] entries, int count)
		{
			var span = entries.AsSpan(0, count);

			lock (m_seen)
			{
				if (m_seen.Contains(thread) == false)
				{
					// output thread name to chrome trace
					// we can affort to do this inefficiently since it's only done once per thread
					m_writer?.WriteLine(", { \"name\": \"thread_name\", \"ph\": \"M\", \"pid\": 0, \"tid\": " + thread.Index.ToString() + ", \"args\": { \"name\": \"" + thread.Name + "\" } }");
					m_seen.Add(thread);
				}
			}

			// some acrobatics here to avoid string allocations

			// assume thread name < 64 chars
			Span<char> buffer = stackalloc char[64 + 5 + k_part0.Length + k_part1.Length + k_part2.Length];
			k_part0.AsSpan().CopyTo(buffer);
			var work = buffer.Slice(k_part0.Length);

			var invFrequencyToMicros = m_invFrequencyToMicros;
			var ti = thread.Index;

			for (int i = 0; i < span.Length; i++)
			{
				ref readonly var entry = ref span[i];

				// part0 is already added
				var rem = work;

				// add name
				var name = entry.Name.AsSpan();
				name.CopyTo(rem);
				rem = rem.Slice(name.Length);

				// add part1
				k_part1.AsSpan().CopyTo(rem);
				rem = rem.Slice(k_part1.Length);

				// add start time
				ulong startMicros = (ulong)((double)entry.Start * invFrequencyToMicros);
				bool ok = startMicros.TryFormat(rem, out int written);
				CoreException.Assert(ok);
				rem = rem.Slice(written);

				// add part2
				k_part2.AsSpan().CopyTo(rem);
				rem = rem.Slice(k_part2.Length);

				// add duration
				ulong durMicros = (ulong)((double)entry.Duration * invFrequencyToMicros);
				ok = durMicros.TryFormat(rem, out written);
				CoreException.Assert(ok);
				rem = rem.Slice(written);

				// add part3
				k_part3.AsSpan().CopyTo(rem);
				rem = rem.Slice(k_part3.Length);

				// add thread index
				ok = ti.TryFormat(rem, out written);
				CoreException.Assert(ok);
				rem = rem.Slice(written);

				// add end
				rem[0] = '}';
				rem = rem.Slice(1);

				// write line to stream
				int lineLen = buffer.Length - rem.Length;
				m_writer?.WriteLine(buffer.Slice(0, lineLen));
			}
		}

		public void Dispose()
		{
			var writer = m_writer;
			m_writer = null;
			if (writer != null)
			{
				writer.WriteLine("]");
				var fs = writer.BaseStream as FileStream;
				if (fs != null)
					fs.Flush(true);
				writer.Dispose();
			}
		}
	}
}
