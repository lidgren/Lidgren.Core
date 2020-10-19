using System;

namespace Lidgren.Core
{
	public static partial class JobService
	{
		/// <summary>
		/// Enqueue work to be called once per argument in list; calls continuation(continuationArgument) when all completed
		/// </summary>
		public static void ForEachArgument(string name, Action<object> work, ReadOnlySpan<object> arguments, Action<object> continuation, object continuationArgument)
		{
			CoreException.Assert(s_workers != null, "JobService not initialized");

			int numJobs = arguments.Length;

			if (numJobs == 0)
				return;

			var completion = JobCompletion.Acquire();
			completion.ContinuationAtCount = numJobs;
			completion.Continuation = continuation;
			completion.ContinuationArgument = continuationArgument;
#if DEBUG
			completion.ContinuationName = name + "Contd";
#endif
			lock (s_instances)
			{
				foreach (var argument in arguments)
					EnqueueInternal(name, work, argument, completion);
			}
		}

		/// <summary>
		/// Enqueue work to be called once per argument in list; blocks until all completed
		/// </summary>
		public static void ForEachArgumentBlock<TArg>(string name, Action<object> work, ReadOnlySpan<TArg> arguments)
		{
			CoreException.Assert(s_workers != null, "JobService not initialized");

			if (arguments.Length == 0)
				return;

			if (arguments.Length == 1)
			{
				work(arguments[0]);
				return;
			}

			int numJobs = arguments.Length - 1;
			var completion = JobCompletion.Acquire();
			lock (s_instances)
			{
				foreach (var argument in arguments.Slice(1))
					EnqueueInternal(name, work, argument, completion);
			}

			// run one time on this thread
			using (new Timing(name))
				work(arguments[0]);

			// then, try to steal relevant jobs if possible

			// am I running within a job? try fetching the JobWorker for this thread
			var localWorker = JobWorker.WorkerForThread;
			while (JobService.ExecuteAnyJob(localWorker, completion))
			;

			completion.WaitAndRelease(numJobs);
		}
	}
}
