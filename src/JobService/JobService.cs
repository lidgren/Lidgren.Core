using System;
using System.Diagnostics;
using System.Threading;

namespace Lidgren.Core
{
	public static partial class JobService
	{
		private static JobWorker[] s_workers;
		private static object s_setupShutdownLock = new object();

		public static int WorkersCount => s_workers == null ? 0 : s_workers.Length;

		public static bool IsInitialized => s_workers != null;

		internal static AutoResetEvent JobWait = new AutoResetEvent(true);
		internal static int m_idleWorkers;

		/// <summary>
		/// returns worker index, or -1 if not jobservice thread
		/// </summary>
		public static int GetWorkerForCurrentThread()
		{
			var worker = JobWorker.WorkerForThread;
			return worker == null ? -1 : worker.Index;
		}
		
		public static void Initialize()
		{
			using var _ = new Timing("jobsvcinit");

			lock (s_setupShutdownLock)
			{
				if (s_workers != null)
					return; // already initialized

				// set up a good amount of workers; but leave some room for main thread and misc other threads
				var hwThreads = Environment.ProcessorCount;

				// minimum 2 job workers; some may assume at least some concurrency
				int numWorkers = (hwThreads <= 2) ? 2 : hwThreads - (1 + (hwThreads / 8));
				s_workers = new JobWorker[numWorkers];
				for (int i = 0; i < numWorkers; i++)
					s_workers[i] = new JobWorker(i);
			}
		}

		// execute one job on this thread (owned by worker)
		// return true if a job was found and executed
		internal static bool ExecuteAnyJob(JobWorker worker, JobCompletion requiredCompletion = null)
		{
			Job job;

			if (requiredCompletion != null)
			{
				if (StealJob(requiredCompletion, out job) == false)
					return false;
			}
			else
			{
				if (s_instancesCount < 0 || PopAnyJob(out job) == false)
				{
					if (PopDelayed(Stopwatch.GetTimestamp(), out job) == false)
						return false;
				}
			}

			// do job
			using (new Timing(job.Name))
			{
#if DEBUG
				if (worker != null)
					worker.CurrentJob = job;
#endif
				// go go go
				job.Work(job.Argument);
			}

			OnJobCompleted(ref job);
			return true;
		}

		private static bool PopAnyJob(out Job job)
		{
			lock (s_instances)
			{
				int cnt = s_instancesCount;
				if (cnt <= 0)
				{
					job = default;
					return false; // exit quickly to let go of lock
				}

				// copy job description
				int nxt = s_nextInstance;
				job = s_instances[nxt];
				nxt = (nxt + 1) & k_instancesMask;
				s_nextInstance = nxt;

				var remainingInstances = cnt - 1;
				s_instancesCount = remainingInstances;
				if (remainingInstances > 0)
					JobWait.Set();
				return true;
			}
		}

		/// <summary>
		/// Slower than pop; will try to steal ONLY jobs with a certain completion
		/// </summary>
		private static bool StealJob(JobCompletion completion, out Job job)
		{
			lock (s_instances)
			{
				int cnt = s_instancesCount;
				if (cnt <= 0)
				{
					job = default;
					return false;
				}

				var idx = s_nextInstance;
				for (int i = 0; i < cnt; i++)
				{
					ref readonly var test = ref s_instances[idx];
					if (test.Completion == completion)
					{
						// found!
						job = test;

						// close gap
						for (; i < cnt - 1; i++)
						{
							var nxt = (idx + 1) & k_instancesMask;
							s_instances[idx] = s_instances[nxt];
							nxt = idx;
						}
						s_instancesCount = cnt - 1;
						return true;
					}
					idx = (idx + 1) & k_instancesMask;
				}

				job = default;
				return false;
			}
		}

		private static void OnJobCompleted(ref Job job)
		{
			var cmp = job.Completion;
			if (cmp != null)
			{
				int cac = cmp.ContinuationAtCount;
				var numCompleted = Interlocked.Increment(ref cmp.Completed);
				if (numCompleted == cac)
				{
					if (cmp.Continuation != null)
					{
						using (new Timing(cmp.ContinuationName == null ? "continuation" : cmp.ContinuationName))
						{
							var post = cmp.Continuation;
							var postArg = cmp.ContinuationArgument;
							JobCompletion.Release(cmp);
							post(postArg);
						}
					}
				}
			}
		}

		public static void Shutdown()
		{
			lock (s_setupShutdownLock)
			{
				if (s_workers != null)
				{
					foreach (var worker in s_workers)
						worker.SetState(JobWorkerState.Shutdown);
					s_workers = null;
					Thread.Sleep(50); // give workers time to exit orderly
				}
			}
		}
	}
}
