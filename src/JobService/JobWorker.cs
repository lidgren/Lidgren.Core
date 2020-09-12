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
		private int m_sleepMode;
		private JobWorkerState m_state;

#if DEBUG
		public Job CurrentJob;
		public string Name { get; set; }
#endif
		internal void SetState(JobWorkerState state)
		{
			if (m_state == JobWorkerState.Shutdown)
				return;
			m_state = state;
		}

		public JobWorker(int index, int sleepMode)
		{
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.IsBackground = true;

			var name = "JobWorker#" + index.ToString();
#if DEBUG
			Name = name;
#endif
			m_thread.Name = name;
			m_sleepMode = sleepMode;

			m_thread.Start();
		}

		private void Run()
		{
			var tt = TimingThread.Instance; // init this thread

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

				switch (m_sleepMode)
				{
					case 0:
						// TODO: spin first, then sleep 0
						Thread.Sleep(0);
						break;
					case 1:
						Thread.Sleep(0);
						break;
					case 2:
					default:
						Thread.Sleep(1);
						break;
				}
			}
		}
	}
}
