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
		private int m_numCompleted;

		public volatile int ContinuationAtCount;
		public Action<object> Continuation;
		public string ContinuationName;
		public object ContinuationArgument;

		public void WaitAndRelease(int completionCount)
		{
			CoreException.Assert(Continuation == null, "Can't use WaitAndRelease while also using a continuation");
			for (; ; )
			{
				var comp = Volatile.Read(ref m_numCompleted);
				if (comp >= completionCount)
					break;
				Thread.Sleep(0);
			}
			JobCompletion.Release(this);
		}

		public static JobCompletion Acquire()
		{
			lock (s_free)
			{
				if (s_free.TryPop(out var retval))
				{
					retval.ResetForReuse();
					return retval;
				}
#if DEBUG
				s_totalNumCreated++;
				CoreException.Assert(s_totalNumCreated < 64, "Unlikely high number of in-flight completions; leaking Completion objects?");
#endif
				return new JobCompletion();
			}
		}

		private void ResetForReuse()
		{
			m_numCompleted = 0;
			ContinuationAtCount = 0;
			Continuation = null;
			ContinuationName = null;
			ContinuationArgument = null;
		}

		public static void Release(JobCompletion completion)
		{
			lock (s_free)
			{
				s_free.Add(completion);
			}
		}

		internal void IncrementCompleted()
		{
			int cac = ContinuationAtCount;
			var numCompleted = Interlocked.Increment(ref m_numCompleted);
			if (numCompleted == cac && Continuation != null)
			{
				using (new Timing(ContinuationName == null ? "continuation" : ContinuationName))
				{
					var post = Continuation;
					var postArg = ContinuationArgument;
					JobCompletion.Release(this);
					post(postArg);
				}
			}
		}
	}
}
