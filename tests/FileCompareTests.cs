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

			Assert.AreEqual(FileCompare.Result.SameFile, FileCompare.AreEqual(assemblyFile, assemblyFile));
			Assert.AreEqual(FileCompare.Result.SameFile, FileCompare.AreEqual("FileOne.txt", "FileOne.txt"));
			Assert.AreEqual(FileCompare.Result.Identical, FileCompare.AreEqual("FileOne.txt", "FileTwo.txt"));
			Assert.AreEqual(FileCompare.Result.Different, FileCompare.AreEqual("FileOne.txt", assemblyFile));
			Assert.AreEqual(FileCompare.Result.File2Missing, FileCompare.AreEqual("FileOne.txt", "xxx"));
			Assert.AreEqual(FileCompare.Result.File1Missing, FileCompare.AreEqual("xxx", "FileOne.txt"));
			Assert.AreEqual(FileCompare.Result.BothFilesMissing, FileCompare.AreEqual("xxx", "yyy"));
			Assert.AreEqual(FileCompare.Result.BothFilesMissing, FileCompare.AreEqual("xxx", "xxx"));
			Assert.AreEqual(FileCompare.Result.File1Missing, FileCompare.AreEqual(null, "FileOne.txt"));

			int bufSize = 4096;
			Assert.AreEqual(FileCompare.Result.SameFile, FileCompare.AreEqual(assemblyFile, assemblyFile, bufSize));
			Assert.AreEqual(FileCompare.Result.SameFile, FileCompare.AreEqual("FileOne.txt", "FileOne.txt", bufSize));
			Assert.AreEqual(FileCompare.Result.Identical, FileCompare.AreEqual("FileOne.txt", "FileTwo.txt", bufSize));
			Assert.AreEqual(FileCompare.Result.Different, FileCompare.AreEqual("FileOne.txt", assemblyFile, bufSize));
			Assert.AreEqual(FileCompare.Result.File2Missing, FileCompare.AreEqual("FileOne.txt", "xxx", bufSize));
			Assert.AreEqual(FileCompare.Result.File1Missing, FileCompare.AreEqual("xxx", "FileOne.txt", bufSize));
			Assert.AreEqual(FileCompare.Result.BothFilesMissing, FileCompare.AreEqual("xxx", "yyy", bufSize));
			Assert.AreEqual(FileCompare.Result.BothFilesMissing, FileCompare.AreEqual("xxx", "xxx", bufSize));
			Assert.AreEqual(FileCompare.Result.File1Missing, FileCompare.AreEqual(null, "FileOne.txt", bufSize));

			var buffer = new byte[8192];
			var b1 = buffer.AsSpan(0, 4096);
			var b2 = buffer.AsSpan(4096);
			Assert.AreEqual(FileCompare.Result.SameFile, FileCompare.AreEqual(assemblyFile, assemblyFile, b1, b2));
			Assert.AreEqual(FileCompare.Result.SameFile, FileCompare.AreEqual("FileOne.txt", "FileOne.txt", b1, b2));
			Assert.AreEqual(FileCompare.Result.Identical, FileCompare.AreEqual("FileOne.txt", "FileTwo.txt", b1, b2));
			Assert.AreEqual(FileCompare.Result.Different, FileCompare.AreEqual("FileOne.txt", assemblyFile, b1, b2));
			Assert.AreEqual(FileCompare.Result.File2Missing, FileCompare.AreEqual("FileOne.txt", "xxx", b1, b2));
			Assert.AreEqual(FileCompare.Result.File1Missing, FileCompare.AreEqual("xxx", "FileOne.txt", b1, b2));
			Assert.AreEqual(FileCompare.Result.BothFilesMissing, FileCompare.AreEqual("xxx", "yyy", b1, b2));
			Assert.AreEqual(FileCompare.Result.BothFilesMissing, FileCompare.AreEqual("xxx", "xxx", b1, b2));
			Assert.AreEqual(FileCompare.Result.File1Missing, FileCompare.AreEqual(null, "FileOne.txt", b1, b2));
		}
	}
}
