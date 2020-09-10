using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lidgren.Core;

namespace UnitTests
{
	[TestClass]
	public class UlidTests
	{
		[TestMethod]
		public void Test()
		{
			// just create
			var anything = Ulid.Create();
			Assert.IsTrue(anything.ToString() != null);

			// from timestamp
			var dto = DateTimeOffset.FromUnixTimeMilliseconds(1484581420);
			var ulid = Ulid.Create(dto);
			Assert.IsTrue(ulid.ToString().StartsWith("0001C7STHC"));

			// string roundtrip
			var str = anything.ToString();
			var rev = Ulid.Parse(str);
			Assert.IsTrue(str == rev.ToString());

			// bytes roundtrip
			var bytes = new byte[16];
			anything.AsBytes(bytes);
			var fromBytes = new Ulid(bytes);
			Assert.IsTrue(anything.Equals(fromBytes));
			Assert.IsTrue(anything.ToString() == fromBytes.ToString());

			// test operators
			var one = new Ulid(100UL, 200UL);
			var two = new Ulid(42UL, 43UL);
			var three = new Ulid(42UL, 43UL);
			Assert.IsTrue(one != two);
			Assert.IsTrue(two == three);
		}
	}
}
