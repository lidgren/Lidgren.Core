using System;
using Lidgren.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class SymmetricMatrixTests
	{
		[TestMethod]
		public void TestSymmetricMatrix()
		{
			TestMatrix(1);
			TestMatrix(2);
			TestMatrix(3);
			TestMatrix(4);
			TestMatrix(5);
			TestMatrix(129);

			TestBoolMatrix(1);
			TestBoolMatrix(2);
			TestBoolMatrix(17);
			TestBoolMatrix(64);
			TestBoolMatrix(65);
			TestBoolMatrix(1023);
			TestBoolMatrix(1024);
			TestBoolMatrix(1025);
		}

		private void TestMatrix(int size)
		{
			var mat = new SymmetricMatrix<int>(size);
			PRNG.Fill<int>(mat.Data);
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					Assert.AreEqual(mat[x, y], mat[y, x]);
		}

		private void TestBoolMatrix(int size)
		{
			var mat = new SymmetricMatrixBool(size);

			// fill
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					mat[x, y] = PRNG.NextBool();

			// check
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					Assert.AreEqual(mat[x, y], mat[y, x]);
		}
	}
}
