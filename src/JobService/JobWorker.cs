using System;
using System.Threading;

namespace Lidgren.Core
{
	internal enum JobWorkerState
	{
		Run,
		TimingFlushTriggered,
		Shutdown
	}

	internal class JobWorker
	{
		private Thread m_thread;
		private JobWorkerState m_state;
		private string m_name;

		[ThreadStatic]
		private static JobWorker s_workerForThread;

		/// <summary>
		/// Get JobWorker associated with current thread; if any
		/// </summary>
		public static JobWorker WorkerForThread => s_workerForThread;

#if DEBUG
		public Job CurrentJob;
#endif
		public string Name => m_name;

		internal void SetState(JobWorkerState state)
		{
			if (m_state == JobWorkerState.Shutdown)
				return;
			m_state = state;
		}

		public JobWorker(int index)
		{
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.IsBackground = true;
			m_name = "JobWorker#" + index.ToString();
			m_thread.Name = m_name;
			m_thread.Start();
		}

		public override string ToString() => m_name;

		private void Run()
		{
			var tt = TimingThread.Instance; // init this thread

			// set thread local
			s_workerForThread = this;

			int numTotalWorkers = JobService.WorkersCount;

			for (; ; )
			{
				if (m_state == JobWorkerState.Shutdown)
				{
					TimingThread.Instance.InternalFlush();
					return; // done
				}

				if (m_state == JobWorkerState.TimingFlushTriggered)
					TimingThread.Instance.InternalFlush();

				bool jobDone = JobService.ExecuteOneJob(this);
				if (jobDone)
					continue;

				// wait for a job to be queued (may give false positives but...)
				int numIdle = Interlocked.Increment(ref JobService.m_idleWorkers);
				if (numIdle < numTotalWorkers) // at least one worker should always be awake
					JobService.JobWait.WaitOne();
				numIdle = Interlocked.Decrement(ref JobService.m_idleWorkers);
			}
		}
	}
}
