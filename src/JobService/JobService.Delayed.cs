using System;
using System.Diagnostics;

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

		private static bool PopDelayed(long now, out Job job)
		{
			lock(s_delayed)
			{
				if (s_delayed.PeekPriority(out var nextPrio) && now > nextPrio)
					return s_delayed.TryDequeue(out job);
				job = default;
				return false;
			}
		}
	}
}
