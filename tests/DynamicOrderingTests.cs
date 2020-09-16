using System;
using System.Diagnostics;
using System.Threading;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	public enum ConcType
	{
		Car,
		Color,
		Game,
		Fruit
	}

	[TestClass]
	public class DynamicOrderingTests
	{
		[TestMethod]
		public void TestDynamicOrdering()
		{
			JobService.Initialize();

			var list = new IDynamicOrdered[]
			{
				new Feet(), new Torso(), new Legs(), new Head(), new Neck()
			};

			bool ok = DynamicOrdering.Perform(list.AsSpan(), out var err);

			Assert.IsTrue(ok);
			Assert.AreEqual(typeof(Head), list[0].GetType());
			Assert.AreEqual(typeof(Neck), list[1].GetType());
			Assert.AreEqual(typeof(Torso), list[2].GetType());
			Assert.AreEqual(typeof(Legs), list[3].GetType());
			Assert.AreEqual(typeof(Feet), list[4].GetType());


			var items = new TestItem[]
			{
				new TestItem("Volvo", 4, ConcType.Car, 0),
				new TestItem("Audi", 3, ConcType.Car, 1),
				new TestItem("Mazda", 6, ConcType.Car, 1),
				new TestItem("BMW", 4, ConcType.Car, 2),
				new TestItem("Opel", 1, ConcType.Car, 1),
				new TestItem("Tesla", 1, ConcType.Car, 1),

				new TestItem("Blue", 4, ConcType.Color, 1),
				new TestItem("AliceBlue", 4, ConcType.Color, 1),
				new TestItem("Chartreuse", 2, ConcType.Color, 1),
				new TestItem("Pink", 4, ConcType.Color, 1),
				new TestItem("Orange", 2, ConcType.Color, 1),

				new TestItem("Battlefield", 4, ConcType.Game, 1),
				new TestItem("AssCreed", 4, ConcType.Game, 1),
				new TestItem("GTA", 24, ConcType.Game, 1),
				new TestItem("Minecraft", 10, ConcType.Game, 1),

				new TestItem("Banana", 1, ConcType.Fruit, 1),
				new TestItem("Apple", 1, ConcType.Fruit, 1),
				new TestItem("Pear", 2, ConcType.Fruit, 1),
			};

			var scheduler = new DynamicScheduler<TestItem>(items);
			scheduler.Execute("stuffs", "hello");
		}
	}

	[DebuggerDisplay("{m_name}")]
	public class TestItem : IDynamicOrdered, IDynamicScheduled
	{
		private string m_name;
		private int m_sleepTime;

		public string Name => m_name;

		private ConcType m_concType;
		private int m_prio;

		public TestItem(string name, int sleepTime, ConcType concurrentType, int prio)
		{
			m_name = name;
			m_sleepTime = sleepTime;
			m_concType = concurrentType;
			m_prio = prio;
		}

		public override string ToString()
		{
			return m_name;
		}

		public void Execute(object arg)
		{
			Thread.Sleep(m_sleepTime);
		}

		public DynamicOrder OrderAgainst(object otherOb)
		{
			var other = otherOb as TestItem;
			if (m_prio > other.m_prio)
				return DynamicOrder.PreferABeforeB;
			if (other.m_prio > m_prio)
				return DynamicOrder.PreferBBeforeA;
			return DynamicOrder.AnyOrder;
		}

		public bool CanRunConcurrently(object other)
		{
			var ot = other as TestItem;
			return m_concType != ot.m_concType;
		}
	}

	public class Head : IDynamicOrdered
	{
		public DynamicOrder OrderAgainst(object other)
		{
			if (other.GetType().Name == "Neck")
				return DynamicOrder.RequireABeforeB;
			return DynamicOrder.PreferABeforeB;
		}
	}

	public class Neck : IDynamicOrdered
	{
		public DynamicOrder OrderAgainst(object other)
		{
			if (other.GetType().Name == "Head")
				return DynamicOrder.PreferBBeforeA;
			return DynamicOrder.PreferABeforeB;
		}
	}


	public class Torso : IDynamicOrdered
	{
		public DynamicOrder OrderAgainst(object other)
		{
			if (other.GetType().Name == "Feet")
				return DynamicOrder.RequireABeforeB;
			return DynamicOrder.AnyOrder;
		}
	}

	public class Legs : IDynamicOrdered
	{
		public DynamicOrder OrderAgainst(object other)
		{
			if (other.GetType().Name == "Torso")
				return DynamicOrder.RequireBBeforeA;
			return DynamicOrder.AnyOrder;
		}
	}

	public class Feet : IDynamicOrdered
	{
		public DynamicOrder OrderAgainst(object other)
		{
			if (other.GetType().Name == "Head")
				return DynamicOrder.RequireBBeforeA;
			return DynamicOrder.PreferBBeforeA;
		}
	}
}
