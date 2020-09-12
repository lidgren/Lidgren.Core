using System;
using System.Threading;

namespace Lidgren.Core
{
	internal sealed class JobCompletion
	{
		private static FastList<JobCompletion> s_free = new FastList<JobCompletion>(4);
#if DEBUG
		private static int s_totalNumCreated = 0;
#endif
		public int Completed;

		public volatile int ContinuationAtCount;
		public Action<object> Continuation;
		public string ContinuationName;
		public object ContinuationArgument;

		public void WaitAndRelease(int completionCount)
		{
			CoreException.Assert(Continuation == null, "Can't use WaitAndRelease while also using a continuation");

			while (Volatile.Read(ref Completed) < completionCount)
				Thread.Sleep(0);
			JobCompletion.Release(this);
		}

		public static JobCompletion Acquire()
		{
			lock (s_free)
			{
				if (s_free.TryPop(out var retval))
				{
					CoreException.Assert(retval.Completed == 0);
					return retval;
				}
#if DEBUG
				s_totalNumCreated++;
				CoreException.Assert(s_totalNumCreated < 64, "Unlikely high number of in-flight completions; leaking Completion objects?");
#endif
				return new JobCompletion();
			}
		}

		public static void Release(JobCompletion completion)
		{
			completion.Completed = 0;
			completion.ContinuationAtCount = 0;
			completion.Continuation = null;
			completion.ContinuationName = null;
			completion.ContinuationArgument = null;

			lock (s_free)
				s_free.Add(completion);
		}
	}
}
