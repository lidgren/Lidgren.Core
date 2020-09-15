using System;
using System.Threading;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class JobServiceTests
	{
		[TestMethod]
		public void TestJobService()
		{
			JobService.Initialize();

			Console.WriteLine($"JobService running with {JobService.WorkersCount.ToString()} workers");

			var action = new Action<object>(Work);
			var argument = "hello";

			var anotherAction = new Action<object>(MoreWork);
			var anotherArgument = "goodbye";

			var actionsList = new Action<object>[] { Work, MoreWork, Work, MoreWork };
			var argumentsList = new object[] { "hi", "howdy", "yello", "wassup" };

			// schedule a single job to run; returns immediately
			JobService.Enqueue((x) => { Console.WriteLine("hello from job"); });

			// runs action(argument) on a worker thread; returns immediately
			JobService.Enqueue("singleFF", action, argument);

			// runs action(argument) on a worker thread; calls anotherAction(anotherArgument) when it is complete; returns immediately
			JobService.Enqueue("singleWc", action, argument, anotherAction, anotherArgument);

			// schedule a wide job; blocks until all has completed
			JobService.EnqueueWideBlock("wideBlk", action, argument);

			// schedule a wide job; call 'anotherAction' when all has finished; returns immediately
			JobService.EnqueueWide("wideWc", action, argument, anotherAction, anotherArgument);

			// schedule a wide job; returns immediately
			JobService.EnqueueWide("wideFF", action, argument, null, null);

			// run each action in list concurrently with the argument; blocks until all has completed
			JobService.ForEachActionBlock("feActionBlk", actionsList, argument);

			// run each action in list concurrently with the argument; when all completed call anotherAction(anotherArgument); returns immediately
			JobService.ForEachAction("feActionWc", actionsList, argument, anotherAction, anotherArgument);

			// schedule work to be done on each argument in list, return immediately
			JobService.ForEachAction("feActionFF", actionsList, argument, null, null);

			// schedule work to be done on each argument in list; blocks until all has completed
			JobService.ForEachArgumentBlock<object>("feArgBlk", action, argumentsList);

			// schedule work to be done on each argument in list; when all completed call anotherAction(anotherArgument); returns immediately
			JobService.ForEachArgument("feArgWc", action, argumentsList, anotherAction, anotherArgument);

			// schedule work to be done on each argument in list; returns immediately
			JobService.ForEachArgument("feArgFF", action, argumentsList, null, null);


			JobService.Shutdown();
		}

		public static void Work(object ob)
		{
			Console.WriteLine("Work(" + ob.ToString() + ")");
			Thread.Sleep(PRNG.Next(2, 5));
		}

		public static void MoreWork(object ob)
		{
			Console.WriteLine("MoreWork(" + ob.ToString() + ")");
			Thread.Sleep(PRNG.Next(2, 5));
		}
	}
}
