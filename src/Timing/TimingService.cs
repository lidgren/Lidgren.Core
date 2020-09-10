using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lidgren.Core
{
	public static class TimingService
	{
		private static FastList<TimingThread> s_threads = new FastList<TimingThread>(16);
		private static FastList<Action<TimingThread, Memory<TimingEntry>>> s_listeners = new FastList<Action<TimingThread, Memory<TimingEntry>>>(2);

		public static bool IsEnabled { get; set; }

		/// <summary>
		/// Add a listener that will be called on the thread that is flushing
		/// </summary>
		public static void AddListener(Action<TimingThread, Memory<TimingEntry>> listener)
		{
			lock (s_listeners)
				s_listeners.Add(listener);
		}

		internal static int RegisterThread(TimingThread thread)
		{
			lock (s_threads)
			{
				var retval = s_threads.Count;
				s_threads.Add(thread);
				return retval;
			}
		}

		public static void TriggerFlush(bool shuttingDown)
		{
			if (shuttingDown)
				IsEnabled = false;

			lock (s_threads)
			{
				foreach (var thread in s_threads.ReadOnlySpan)
					thread.TriggerFlush();

				// trigger flush on _this_ thread immediately
				TimingThread.Instance.InternalFlush();
			}

			if (shuttingDown)
			{
				Thread.Sleep(17); // give other threads some time to flush voluntarily

				// forced flush; let's squeeze out the last remaining scopes in the threads
				foreach (var thread in s_threads.ReadOnlySpan)
				{
					try
					{
						thread.InternalFlush(); // ick!
					}
					catch { } // best effort
				}
			}
		}

		// called by TimingThread
		internal static void Flush(TimingThread thread, Memory<TimingEntry> span)
		{
			foreach (var listener in s_listeners.ReadOnlySpan)
			{
				lock (listener)
					listener(thread, span);
			}
		}
	}

	/// <summary>
	/// Example usage: using var _ = new Timing("myScopeName");
	/// </summary>
	public ref struct Timing
	{
		public string Name;
		public long Started;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Timing(string name)
		{
			CoreException.Assert(string.IsNullOrWhiteSpace(name) == false);
			Name = name;
			Started = Stopwatch.GetTimestamp();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			long now = Stopwatch.GetTimestamp();
			CoreException.Assert(now >= Started);
			long duration = now - Started;
			if (duration > 0 && duration < int.MaxValue)
				TimingThread.Instance.AddEntry(Name, Started, (int)duration);
		}
	}
}
