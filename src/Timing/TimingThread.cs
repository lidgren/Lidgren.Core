#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lidgren.Core
{
	[DebuggerDisplay("{Name} {Start} Dur {Duration}")]
	public struct TimingEntry
	{
		public string Name;
		public long Start;
		public uint Duration;
	}

	[DebuggerDisplay("{m_threadIndex} {m_threadName}")]
	public sealed class TimingThread
	{
		private const int k_maxCompletedEntries = 1024;
		private const int k_attemptFlushCount = 1024 - 64; // try to flush after this many completed entries

		private string? m_threadName;
		private int m_threadIndex;
		private readonly TimingEntry[] m_completed;
		private int m_completedCount;
		private bool m_triggerFlushOnce;
		private Thread m_thread;

		public string Name => m_threadName ?? "";
		public Thread Thread => m_thread;
		public int Index => m_threadIndex;

		/// <summary>
		/// One instance per thread
		/// </summary>
		[ThreadStatic]
		private static TimingThread? s_instance;

		/// <summary>
		/// Get thread specific timing instance
		/// </summary>
		public static TimingThread Instance
		{
			get
			{
				if (s_instance == null)
					s_instance = CreateNewTimingThread(); // thread static and rare
				return s_instance;
			}
		}

		// rare; don't inline!
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static TimingThread CreateNewTimingThread()
		{
			var instance = new TimingThread();
			int index = TimingService.RegisterThread(instance);
			instance.Init(index);
			return instance;
		}

		public TimingThread()
		{
			m_completed = new TimingEntry[k_maxCompletedEntries];
			m_thread = Thread.CurrentThread;
		}

		internal void TriggerFlush()
		{
			m_triggerFlushOnce = true;
		}

		/// <summary>
		/// Only call manually when adding external or "virtual" thread such as a "GPU thread"
		/// </summary>
		public void Init(int threadIndex, string? threadName = null)
		{
			m_threadIndex = threadIndex;
			if (string.IsNullOrWhiteSpace(threadName))
			{
				threadName = m_thread?.Name;
				if (string.IsNullOrWhiteSpace(threadName))
					threadName = "Thread#" + threadIndex;
			}
			m_threadName = threadName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEntry(string name, long started, int duration)
		{
			if (duration < 0)
			{
				CoreException.Assert(duration >= 0, "AddEntry negative duration");
				return;
			}

			int ccnt = m_completedCount;

			if (ccnt > 0 && m_completed[ccnt - 1].Start == started)
				started++; // push one tick to make better hierarchies

			// no need for interlocked; this is a thread local object
			ref var complete = ref m_completed[ccnt];
			m_completedCount = ccnt + 1;
			complete.Name = name;
			complete.Start = started;
			complete.Duration = (uint)duration;
			if (m_triggerFlushOnce || m_completedCount >= k_attemptFlushCount)
				InternalFlush();
		}

		// running on this thread; unless forced
		internal void InternalFlush()
		{
			lock (m_completed) // typically not needed; but cheap if not contended anyway
			{
				m_triggerFlushOnce = false;
				if (m_completedCount > 0)
				{
					TimingService.Flush(this, m_completed, m_completedCount);
					m_completedCount = 0;
				}
			}
		}
	}
}
