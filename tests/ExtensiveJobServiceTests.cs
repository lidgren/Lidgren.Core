using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;
using System.Threading;

namespace UnitTests
{
	[TestClass]
	public class ExtensiveJobServiceTests
	{
		[TestMethod]
		public void TestJobServiceMore()
		{
			const float runFor = 4.0f; // run for this many seconds
			const int simultaneousTests = 10;

			JobService.Initialize();

			//TimingService.IsEnabled = true;
			//var chrome = new ChromeTraceTimingConsumer("ejobtests.json");

			var inProgress = new List<JobTest>();

			var end = TimeService.Wall + runFor;
			for(; ;)
			{
				double now = TimeService.Wall;
				if (now > end)
					break;

				// check for completeness and timeouts
				for (int i = 0; i < inProgress.Count; i++)
				{
					var test = inProgress[i];
					if (test.IsComplete())
					{
						test.Verify();
						inProgress.RemoveAt(i);
						i--;
					}
					else if (now > test.Timeout)
					{
						Assert.Fail(test.GetType().Name + " timed out");
						inProgress.RemoveAt(i);
						i--;
					}
				}

				// start tests if necessary
				while(inProgress.Count < simultaneousTests)
				{
					var test = StartRandomTest();
					inProgress.Add(test);
				}
			}

			for (int i = 0; i < m_runs.Length; i++)
			{
				if (m_runs[i] > 0)
					Console.WriteLine($"Test type {i} started {m_runs[i]} times");
			}

			//TimingService.TriggerFlush(true); // this will set TimingService.IsEnabled to false
			//chrome.Dispose();
		}

		private int[] m_runs = new int[10];

		private JobTest StartRandomTest()
		{
			JobTest test;
			var nr = PRNG.Next(0, 9);
			m_runs[nr]++;
			switch (nr)
			{
				case 0:
				case 1:
				case 2:
				case 3:
					test = new Simple();
					break;
				case 4:
				case 5:
					test = new Wide();
					break;
				case 6:
				case 7:
					test = new WideArg();
					break;
				case 8:
					test = new Delayed();
					break;
				default:
					throw new NotImplementedException();
			}
			test.Timeout = TimeService.Wall + test.Start();
			return test;
		}
	}

	public abstract class JobTest
	{
		protected string m_name;

		public JobTest()
		{
			m_name = GetType().Name + StringUtils.ToHex(PRNG.NextUInt32());
		}

		public double Timeout;
		public abstract float Start();
		public abstract bool IsComplete();
		public abstract void Verify();
	}

	public class Simple : JobTest
	{
		private bool m_didExecute = false;
		public override float Start()
		{
			JobService.Enqueue(m_name, Execute, m_name);
			return 1.0f;
		}

		public override bool IsComplete()
		{
			return m_didExecute;
		}

		private void Execute(object ob)
		{
			Assert.AreEqual(ob, m_name);
			Thread.Sleep(PRNG.Next(0, 4));
			m_didExecute = true;
		}

		public override void Verify()
		{
			Assert.IsTrue(m_didExecute);
		}
	}

	public class Wide : JobTest
	{
		private int m_ranTimes = 0;
		private int m_requireRuns;

		public override float Start()
		{
			m_ranTimes = 0;
			m_requireRuns = JobService.WorkersCount;
			JobService.EnqueueWide(m_name, Execute, m_name);
			return 1.0f;
		}

		private void Execute(object ob)
		{
			Assert.AreEqual(ob, m_name);
			Thread.Sleep(PRNG.Next(0, 4));
			Interlocked.Increment(ref m_ranTimes);
		}

		public override bool IsComplete()
		{
			return m_ranTimes >= m_requireRuns;
		}

		public override void Verify()
		{
			Assert.IsTrue(m_ranTimes == m_requireRuns);
		}
	}

	public class WideArg : JobTest
	{
		private bool m_allOk;
		private object[] m_args;
		private bool[] m_done;

		public override float Start()
		{
			var num = PRNG.Next(1, 5);
			m_args = new object[num];
			m_done = new bool[num];

			for (int i = 0; i < m_args.Length; i++)
				m_args[i] = StringUtils.ToHex(PRNG.NextUInt32());

			JobService.ForEachArgument(m_name, Execute, m_args, OnDone, "onDone");
			return 1.0f;
		}

		private void OnDone(object obj)
		{
			Assert.AreEqual("onDone", obj);

			Thread.Sleep(10);
			for (int i = 0; i < m_args.Length; i++)
				Assert.IsTrue(m_done[i]);
			m_allOk = true;
		}

		private void Execute(object ob)
		{
			// which
			for (int i = 0; i < m_args.Length; i++)
			{
				if (m_args[i] == ob)
				{
					Assert.IsFalse(m_done[i]);
					m_done[i] = true;
				}
			}
			Thread.Sleep(PRNG.Next(0, 4));
		}

		public override bool IsComplete()
		{
			return m_allOk;
		}

		public override void Verify()
		{
		}
	}

	public class Delayed : JobTest
	{
		private bool m_didExecute = false;
		private float m_delay;
		private double m_started;

		public override float Start()
		{
			m_started = TimeService.Wall;
			m_delay = PRNG.NextFloat(0.01f, 0.05f);
			JobService.Enqueue(m_name, Execute, m_name, m_delay);
			return m_delay + 1.0f;
		}

		public override bool IsComplete()
		{
			return m_didExecute;
		}

		private void Execute(object ob)
		{
			double ago = TimeService.Wall - m_started;
			Assert.IsTrue(ago > m_delay);
			Assert.IsTrue(ago < m_delay + 0.5);
			Assert.AreEqual(ob, m_name);
			Thread.Sleep(PRNG.Next(0, 4));
			m_didExecute = true;
		}

		public override void Verify()
		{
			Assert.IsTrue(m_didExecute);
		}
	}
}
