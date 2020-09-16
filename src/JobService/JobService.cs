using System;
using System.Threading;

namespace Lidgren.Core
{
	public static partial class JobService
	{
		private static JobWorker[] s_workers;
		private static object s_setupShutdownLock = new object();

		public static int WorkersCount => s_workers == null ? 0 : s_workers.Length;

		public static bool IsInitialized => s_workers != null;

		public static void Initialize()
		{
			using var _ = new Timing("jobsvcinit");

			lock (s_setupShutdownLock)
			{
				if (s_workers != null)
					return; // already initialized

				// set up a good amount of workers; but leave some room for main thread and misc other threads
				JobWorker[] workers = null;
				var hw = Environment.ProcessorCount;
				switch (hw)
				{
					case 0:
						workers = new JobWorker[] { }; // for nullability 
						CoreException.Throw("Failed to determine number of HW threads");
						break;
					case 1:
						workers = new JobWorker[] { new JobWorker(0, 1) };
						break;
					case 2:
					case 3: // ?!
						workers = new JobWorker[] { new JobWorker(0, 1), new JobWorker(1, 1) };
						break;
					case 4:
					case 5: // ?!
						workers = new JobWorker[] { new JobWorker(0, 0), new JobWorker(1, 1), new JobWorker(2, 1) };
						break;
					case 6:
					case 7:
						workers = new JobWorker[] { new JobWorker(0, 0), new JobWorker(1, 1), new JobWorker(2, 1), new JobWorker(3, 1), new JobWorker(4, 2) };
						break;
					case 8:
						workers = new JobWorker[]
						{
						new JobWorker(0, 0),
						new JobWorker(1, 1),
						new JobWorker(2, 1),
						new JobWorker(3, 1),
						new JobWorker(4, 1),
						new JobWorker(5, 2),
						new JobWorker(6, 2)
						};
						break;
					default:
						var cnt = hw - (1 + (hw / 8));
						workers = new JobWorker[cnt];
						workers[0] = new JobWorker(0, 0);
						workers[1] = new JobWorker(1, 0);
						workers[cnt - 1] = new JobWorker(cnt - 1, 2);
						workers[cnt - 2] = new JobWorker(cnt - 2, 2);
						for (int i = 2; i < cnt - 2; i++)
							workers[i] = new JobWorker(i, 1);
						break;
				}

				s_workers = workers;
			}
		}

		// execute one job on this thread (owned by worker)
		// return true if a job was found and executed
		internal static bool ExecuteOneJob(JobWorker worker, JobCompletion requiredCompletion = null)
		{
			if (s_instancesCount <= 0)
				return false;

			Job job;

			if (requiredCompletion != null)
			{
				if (StealJob(requiredCompletion, out job) == false)
					return false;
			}
			else
			{
				if (PopAnyJob(out job) == false)
					return false;
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
				s_instancesCount = cnt - 1;
			}
			return true;
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
				var numCompleted = Interlocked.Increment(ref cmp.Completed);
				if (cmp.Continuation != null)
				{
					if (numCompleted == cmp.ContinuationAtCount)
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
