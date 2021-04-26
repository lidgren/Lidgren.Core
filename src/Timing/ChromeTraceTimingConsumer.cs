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
			//m_invFrequencyToMicros = 1000000.0 / (double)Stopwatch.Frequency;
			m_invFrequencyToMicros = 1000000000.0 / (double)Stopwatch.Frequency;

			var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
			m_writer = new StreamWriter(fs);
			m_writer.WriteLine("{ \"traceEvents\": [");
			m_writer.Flush();
			TimingService.AddListener(Flush);
		}

		private const string k_namePart = ",{\"name\":\"";

		private void Flush(TimingThread thread, TimingEntry[] entries, int count)
		{
			var span = entries.AsSpan(0, count);

			var writer = m_writer;
			if (writer == null)
				return;

			lock (m_seen)
			{
				if (m_seen.Contains(thread) == false)
				{
					// output thread name to chrome trace
					// we can affort to do this inefficiently since it's only done once per thread
					writer.WriteLine(", { \"name\": \"thread_name\", \"ph\": \"M\", \"pid\": 0, \"tid\": " + thread.Index.ToString() + ", \"args\": { \"name\": \"" + thread.Name + "\" } }");
					m_seen.Add(thread);
				}
			}

			var invFrequencyToMicros = m_invFrequencyToMicros;
			var ti = thread.Index;

			Span<char> buf = stackalloc char[256];
			var bdr = new FixedStringBuilder(buf);
			bdr.Append(k_namePart);
			for (int i = 0; i < span.Length; i++)
			{
				bdr.Length = k_namePart.Length; // keep name part only

				ref readonly var entry = ref span[i];

				bdr.Append(entry.Name);

				// add start time
				bdr.Append("\",\"ph\":\"X\",\"ts\":");
				ulong startMicros = (ulong)((double)entry.Start * invFrequencyToMicros);
				bdr.Append(startMicros);

				// add duration
				bdr.Append(",\"dur\":");
				ulong durMicros = (ulong)((double)entry.Duration * invFrequencyToMicros);
				if (durMicros <= 0)
					durMicros = 1;
				bdr.Append(durMicros);

				// add thread index
				bdr.Append(",\"pid\":0,\"tid\":");
				bdr.Append(ti);

				// add end
				bdr.Append('}');

				// write line to stream
				writer.WriteLine(bdr.ReadOnlySpan);
			}
		}

		public void Dispose()
		{
			var writer = m_writer;
			m_writer = null;
			if (writer != null)
			{
				writer.WriteLine("] }");
				var fs = writer.BaseStream as FileStream;
				if (fs != null)
					fs.Flush(true);
				writer.Dispose();
			}
		}
	}
}
