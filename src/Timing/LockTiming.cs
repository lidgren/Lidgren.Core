using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lidgren.Core
{
	/// <summary>
	/// Example usage: using (new LockTiming("timerName", myLockObject)) { ... } - will add waitLock scope if lock cannot be taken immediately
	/// </summary>
	public ref struct LockTiming
	{
		public string Name;
		public long Started;
		public long WaitEnd;
		public object LockObject;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LockTiming(string name, object lockObject)
		{
			CoreException.Assert(string.IsNullOrWhiteSpace(name) == false);
			Name = name;
			LockObject = lockObject;
			Started = Stopwatch.GetTimestamp();

			var ok = Monitor.TryEnter(lockObject);
			if (!ok)
			{
				Monitor.Enter(lockObject);
				WaitEnd = Stopwatch.GetTimestamp();
			}
			else
			{
				WaitEnd = Started;
			}
		}

		/// <summary>
		/// No need/possibility to implement IDisposable; but compiler magic will call this anyway!
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			Monitor.Exit(LockObject);

			long now = Stopwatch.GetTimestamp();
			CoreException.Assert(now >= Started);
			long duration = now - Started;
			var waited = WaitEnd - Started;
			if (duration > 0 && duration < int.MaxValue)
			{
				TimingThread.Instance.AddEntry(Name, Started, (int)duration);
				if (waited > 0)
					TimingThread.Instance.AddEntry("lockWait", Started, (int)waited);
			}
		}
	}
}
