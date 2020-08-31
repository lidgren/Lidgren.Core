using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public sealed class CoreException : Exception
	{
#if DEBUG
		private static readonly HashSet<object> s_runOnceHasRan = new HashSet<object>();
#endif

		public CoreException()
			: base()
		{
		}

		public CoreException(string message)
			: base(message)
		{
		}

		public CoreException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Throws an exception, in DEBUG only, if first parameter is false
		/// </summary>
		[Conditional("DEBUG")]
		public static void Assert(bool isOk, string message = "")
		{
			if (isOk == false)
			{
#if DEBUG
				if (Debugger.IsAttached)
					Debugger.Break();
				else
					CoreException.Throw(message);
#else
				CoreException.Throw(message);
#endif
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Throw(string message = null, Exception inner = null)
		{
			throw new CoreException(message, inner);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ThrowNotImplemented(string message = null)
		{
			throw new CoreException(message == null ? "Not implemented" : message);
		}

		/// <summary>
		/// Throw exception, in DEBUG only, if this identifier runs more than once
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertRunOnce(object identifier)
		{
#if DEBUG
			lock (identifier)
			{
				if (s_runOnceHasRan.Contains(identifier))
				{
					Throw("Identifier " + identifier + " ran more than once!");
					return;
				}
				s_runOnceHasRan.Add(identifier);
			}
#endif
		}

		[Conditional("DEBUG")]
		public static void CheckNaN(Vector3 vector)
		{
			CoreException.Assert(
				float.IsNaN(vector.X) == false &&
				float.IsNaN(vector.Y) == false &&
				float.IsNaN(vector.Z) == false &&

				float.IsInfinity(vector.X) == false &&
				float.IsInfinity(vector.Y) == false &&
				float.IsInfinity(vector.Z) == false
			);
		}

		[Conditional("DEBUG")]
		public static void CheckNaN(float vector)
		{
			CoreException.Assert(float.IsNaN(vector) == false);
		}

		[Conditional("DEBUG")]
		public static void CheckNaN(Quaternion vector)
		{
			CoreException.Assert(
				float.IsNaN(vector.X) == false &&
				float.IsNaN(vector.Z) == false &&
				float.IsNaN(vector.Y) == false &&
				float.IsNaN(vector.W) == false &&
				float.IsInfinity(vector.X) == false &&
				float.IsInfinity(vector.Y) == false &&
				float.IsInfinity(vector.Z) == false
			);
		}

		[Conditional("DEBUG")]
		public static void CheckNaN(Vector2 vector)
		{
			CoreException.Assert(
				float.IsNaN(vector.X) == false &&
				float.IsNaN(vector.Y) == false &&

				float.IsInfinity(vector.X) == false &&
				float.IsInfinity(vector.Y) == false
			);
		}
	}
}
