#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lidgren.Core
{
	public static class TimingService
	{
		private static readonly FastList<TimingThread> s_threads = new FastList<TimingThread>(16);
		private static readonly FastList<Action<TimingThread, TimingEntry[], int>> s_listeners = new FastList<Action<TimingThread, TimingEntry[], int>>(2);

		public static bool IsEnabled { get; set; }

		/// <summary>
		/// Add a listener that will be called on the thread that is flushing
		/// </summary>
		public static void AddListener(Action<TimingThread, TimingEntry[], int> listener)
		{
			lock (s_listeners)
				s_listeners.Add(listener);
		}

		/// <summary>
		/// Only call this manually when adding external or "virtual" threads such as "GPU thread" for profiling
		/// </summary>
		public static int RegisterThread(TimingThread thread)
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
				Thread.Sleep(34); // give other threads some time to flush voluntarily

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
		internal static void Flush(TimingThread thread, TimingEntry[] items, int count)
		{
			foreach (var listener in s_listeners.ReadOnlySpan)
			{
				lock (listener)
					listener(thread, items, count);
			}
		}
	}
}
