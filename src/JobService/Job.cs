using System;

namespace Lidgren.Core
{
	internal struct Job
	{
		public string Name;
		public Action<object> Work;
		public object Argument;
		public JobCompletion Completion;
	}
}
