using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public static partial class JobService
	{
		private static readonly PriorityQueue<long, Job> s_delayed = new PriorityQueue<long, Job>(16);

		public static void Enqueue(string name, Action<object> work, object argument, double delaySeconds)
		{
			long earliest = Stopwatch.GetTimestamp() + TimeService.SecondsToTicks(delaySeconds);
			var djob = new Job() { Name = name, Work = work, Argument = argument };
			lock (s_delayed)
				s_delayed.Enqueue(earliest, djob);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PopDelayed(out Job job)
		{
			var now = Stopwatch.GetTimestamp();
			lock (s_delayed)
				return s_delayed.TryDequeue(out job, now);
		}
	}
}
