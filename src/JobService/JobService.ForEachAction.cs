using System;

namespace Lidgren.Core
{
	public static partial class JobService
	{
		/// <summary>
		/// Enqueue a list of actions to be performed concurrently; calls continuation(continuationArgument) when all completed
		/// </summary>
		public static void ForEachAction(string name, ReadOnlySpan<Action<object>> works, object argument, Action<object> continuation, object continuationArgument)
		{
			CoreException.Assert(s_workers != null, "JobService not initialized");

			int numJobs = works.Length;

			if (numJobs == 0)
				return;
			if (numJobs == 1)
			{
				EnqueueInternal(name, works[0], argument);
				return;
			}

			var completion = JobCompletion.Acquire();
			completion.ContinuationAtCount = numJobs;
			completion.Continuation = continuation;
			completion.ContinuationArgument = continuationArgument;
#if DEBUG
			completion.ContinuationName = name + "Contd";
#endif

			foreach (var work in works)
				EnqueueInternal(name, work, argument, completion);
		}

		/// <summary>
		/// Enqueue a list of actions to be performed concurrently; calls continuation(continuationArgument) when all completed
		/// </summary>
		public static void ForEachActionBlock(string name, ReadOnlySpan<Action<object>> works, object argument)
		{
			CoreException.Assert(s_workers != null, "JobService not initialized");

			if (works.Length == 0)
				return;
			if (works.Length == 1)
			{
				works[0](argument);
				return;
			}

			int numJobs = works.Length - 1;
			var completion = JobCompletion.Acquire();
			foreach (var work in works.Slice(1))
				EnqueueInternal(name, work, argument, completion);

			// run one time on this thread
			using (new Timing(name))
				works[0](argument);

			// then, try to steal relevant jobs if possible
			while (JobService.ExecuteAnyJob(null, completion))
				;

			completion.WaitAndRelease(numJobs);
		}
	}
}
