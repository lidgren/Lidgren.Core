using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lidgren.Core;
using System.Reflection;

namespace UnitTests
{
	[TestClass]
	public class FileCompareTests
	{
		[TestMethod]
		public void TextFileCompare()
		{
			// get this assembly file
			var assemblyFile = Assembly.GetExecutingAssembly().Location;

			Assert.IsTrue(FileCompare.AreEqual(assemblyFile, assemblyFile));
			Assert.IsTrue(FileCompare.AreEqual("FileOne.txt", "FileOne.txt"));
			Assert.IsTrue(FileCompare.AreEqual("FileOne.txt", "FileTwo.txt"));
			Assert.IsFalse(FileCompare.AreEqual("FileOne.txt", assemblyFile));
			Assert.IsFalse(FileCompare.AreEqual("FileOne.txt", "xxx"));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "FileOne.txt"));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "yyy"));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "xxx"));
			Assert.IsFalse(FileCompare.AreEqual(null, "xxx"));

			int bufSize = 4096;
			Assert.IsTrue(FileCompare.AreEqual(assemblyFile, assemblyFile, bufSize));
			Assert.IsTrue(FileCompare.AreEqual("FileOne.txt", "FileOne.txt", bufSize));
			Assert.IsTrue(FileCompare.AreEqual("FileOne.txt", "FileTwo.txt", bufSize));
			Assert.IsFalse(FileCompare.AreEqual("FileOne.txt", assemblyFile, bufSize));
			Assert.IsFalse(FileCompare.AreEqual("FileOne.txt", "xxx", bufSize));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "FileOne.txt", bufSize));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "yyy", bufSize));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "xxx", bufSize));
			Assert.IsFalse(FileCompare.AreEqual(null, "xxx", bufSize));

			var buffer = new byte[8192];
			var b1 = buffer.AsSpan(0, 4096);
			var b2 = buffer.AsSpan(4096);
			Assert.IsTrue(FileCompare.AreEqual(assemblyFile, assemblyFile, b1, b2));
			Assert.IsTrue(FileCompare.AreEqual("FileOne.txt", "FileOne.txt", b1, b2));
			Assert.IsTrue(FileCompare.AreEqual("FileOne.txt", "FileTwo.txt", b1, b2));
			Assert.IsFalse(FileCompare.AreEqual("FileOne.txt", assemblyFile, b1, b2));
			Assert.IsFalse(FileCompare.AreEqual("FileOne.txt", "xxx", b1, b2));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "FileOne.txt", b1, b2));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "yyy", b1, b2));
			Assert.IsFalse(FileCompare.AreEqual("xxx", "xxx", b1, b2));
			Assert.IsFalse(FileCompare.AreEqual(null, "xxx", b1, b2));
		}
	}
}
