using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
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

		/// <summary>
		/// No need/possibility to implement IDisposable; but compiler magic will call this anyway!
		/// </summary>
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
