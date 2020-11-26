using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Dictionary of K,V equivalent with GetOrInit() TryGetRef() and better performance
	/// </summary>
	[DebuggerDisplay("Count = {Count}")]
	public sealed class FastDictionary<TKey, TValue> :
		IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
		IEnumerable<KeyValuePair<TKey, TValue>>
		where TKey : IEquatable<TKey>
	{
		[DebuggerDisplay("Key: {Key} Next: {Next}")]
		private struct Entry
		{
			// If x >= 0:   Holds value; next entry in chain is: x - 1  (result -1 means end of chain)
			//
			// If x < 0:    Freed value or unused; next freed value in chain is: -(x + 2)  (result -1 means unused; end of free chain)
			//
			//  Examples:
			//  -4    Freed value; next freed value in chain is slot 2 ( -(x + 2) )
			//  -3    Freed value; next freed value in chain is slot 1 ( -(x + 2) )
			//  -2    Freed value; next freed value in chain is slot 0 ( -(x + 2) )
			//  -1    Unused entry
			//   0    Holds value; end of chain
			//   1    Holds value; next in chain is slot 0 (x - 1)
			//   2    Holds value; next in chain is slot 1 (x - 1)
			public int Next;

			public TKey Key;
		}

		private int[] m_lookup;
		private Entry[] m_entries;
		private TValue[] m_values;
		private int m_count;
		private int m_freeListIndex;
		private int m_capacityMinusOne;

		public int Count { get { return m_count; } }

		public FastDictionary(int minCapacity = 64)
		{
			int capacity = MathUtils.NextPowerOfTwo(minCapacity);

			m_lookup = new int[capacity];
			m_capacityMinusOne = capacity - 1;

			m_entries = new Entry[capacity];
			m_values = new TValue[capacity];

			Clear();
			Validate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(TKey key)
		{
			int index = m_lookup[KeyToLookupIndex(key)];
			var entries = m_entries;
			while (index >= 0)
			{
				ref var entry = ref entries[index];
				if (key.Equals(entry.Key))
					return true;
				index = entry.Next - 1;
			}
			return false;
		}

		public bool ContainsValue(in TValue value)
		{
			int count = m_count;
			var entries = m_entries;
			int examined = 0;
			for (int i = 0; i < entries.Length && examined < count; i++)
			{
				ref var item = ref entries[i];
				if (item.Next >= 0)
				{
					examined++;
					if (EqualityComparer<TValue>.Default.Equals(m_values[i], value))
						return true;
				}
			}
			return false;
		}

		private static readonly Entry s_emptyEntry = new Entry() { Next = -1, Key = default(TKey) };

		public void Clear()
		{
			m_lookup.AsSpan().Fill(-1);
			m_entries.AsSpan().Fill(s_emptyEntry);
			m_count = 0;
			m_freeListIndex = -1;

			Validate();
		}

		private static TValue s_dummyValue = default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int KeyToLookupIndex(TKey key)
		{
			return key.GetHashCode() & m_capacityMinusOne;
		}

		public ref TValue TryGetRef(TKey key, out bool wasFound)
		{
			int index = m_lookup[KeyToLookupIndex(key)];
			var entries = m_entries;
			while (index >= 0)
			{
				ref var entry = ref entries[index];
				if (key.Equals(entry.Key))
				{
					wasFound = true;
					return ref m_values[index];
				}
				index = entry.Next - 1;
			}
			wasFound = false;
			return ref s_dummyValue;
		}

		public TValue this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				ref var value = ref TryGetRef(key, out bool found);
				if (found)
					return value;
				CoreException.Throw("Index not found");
				return default(TValue);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				ref var val = ref GetOrInit(key, out _);
				val = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey key, in TValue value)
		{
			ref var val = ref GetOrInit(key, out _);
			val = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			ref var valueByRef = ref TryGetRef(key, out bool found);
			if (found)
			{
				value = valueByRef;
				return true;
			}
			value = default(TValue);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue GetValueOrDefault(TKey key)
		{
			ref var valueByRef = ref TryGetRef(key, out bool found);
			if (found)
				return valueByRef;
			return default(TValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TKey key, in TValue value)
		{
			ref var val = ref GetOrInit(key, out _);
			val = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public ref TValue GetOrInit(TKey key, out bool wasCreated)
		{
			var entries = m_entries;

			int bucketIndex = KeyToLookupIndex(key);
			int chainLength = 0;

			int entryIndex = m_lookup[bucketIndex];
			if (entryIndex >= 0)
			{
				for (; ; )
				{
					ref Entry entry = ref entries[entryIndex];
					if (key.Equals(entry.Key))
					{
						// found!
						wasCreated = false;
						return ref m_values[entryIndex];
					}
					var next = entry.Next;
					if (next < 1)
						break;
					entryIndex = entry.Next - 1;
					chainLength++;
				}
			}

			// not found; add
			int addedAsIndex;

			if (m_freeListIndex != -1)
			{
				// get slot from free list
				addedAsIndex = m_freeListIndex;
				m_freeListIndex = -(entries[addedAsIndex].Next + 2);
			}
			else
			{
				if (m_count >= entries.Length || chainLength > 100)
				{
					Grow();
					return ref GetOrInit(key, out wasCreated);
				}
				addedAsIndex = m_count;
			}
			m_count++;

			if (entryIndex == -1)
			{
				// first in bucket
				m_lookup[bucketIndex] = addedAsIndex;
			}
			else
			{
				// added to chain
				entries[entryIndex].Next = addedAsIndex + 1;
			}

			ref Entry newEntry = ref entries[addedAsIndex];
			newEntry.Key = key;
			newEntry.Next = 0; // last in chain; no next item

			Validate();

			wasCreated = true;
			return ref m_values[addedAsIndex];
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Grow()
		{
			int newSize = m_entries.Length << 1;

			CoreException.Assert(m_count == m_entries.Length);
			CoreException.Assert(MathUtils.IsPowerOfTwo(newSize));
			m_capacityMinusOne = newSize - 1;

			m_lookup = new int[newSize];
			m_lookup.AsSpan().Fill(-1);

			Array.Resize(ref m_entries, newSize);
			Array.Resize(ref m_values, newSize);

			// loop over all entries and reinsert them into bucket lists
			var entries = m_entries;
			int count = m_count;
			int mask = m_capacityMinusOne;
			for (int i = 0; i < count; i++)
			{
				ref var entry = ref entries[i];
				var hash = entry.Key.GetHashCode();
				var bucketIndex = hash & mask;

				// place first in chain
				ref var bucket = ref m_lookup[bucketIndex];
				entry.Next = bucket + 1;
				bucket = i;
			}

			m_freeListIndex = m_count;

			for (int i = m_count; i < entries.Length; i++)
			{
				ref var entry = ref entries[i];
				entry.Next = -1;
			}

			Validate();
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public bool Remove(TKey key)
		{
			int bucketIndex = KeyToLookupIndex(key);
			int index = m_lookup[bucketIndex];
			int previousIndex = -1;
			var entries = m_entries;
			var lookup = m_lookup;
			while (index >= 0)
			{
				ref Entry indexEntry = ref entries[index];
				if (key.Equals(indexEntry.Key))
				{
					// found it, bypass
					if (previousIndex >= 0)
						entries[previousIndex].Next = indexEntry.Next;
					else
						lookup[bucketIndex] = indexEntry.Next - 1;

					// add first in free list
					indexEntry.Next = -(m_freeListIndex + 2);
					m_freeListIndex = index;
					m_count--;

					Validate();
					return true;
				}
				previousIndex = index;
				index = indexEntry.Next - 1;
			}

			Validate();
			return false;
		}

		[Conditional("DEBUG")]
		public void Validate()
		{
			var count = 0;
			var lookup = m_lookup;
			var entries = m_entries;
			for (int b = 0; b < lookup.Length; b++)
			{
				int idx = lookup[b];
				while (idx >= 0)
				{
					count++;
					idx = entries[idx].Next - 1;
				}
			}
			CoreException.Assert(count == m_count);
		}

		public void CopyKeys(Span<TKey> target)
		{
			CoreException.Assert(target.Length >= m_count);

			int count = m_count;
			var entries = m_entries;
			int added = 0;
			for (int i = 0; i < entries.Length && added < count; i++)
			{
				ref var item = ref entries[i];
				if (item.Next >= 0)
					target[added++] = item.Key;
			}
		}

		public int CopyValues(Span<TValue> target)
		{
			int count = Math.Min(target.Length, m_count);
			var entries = m_entries;
			var values = m_values;
			int added = 0;
			for (int i = 0; i < entries.Length && added < count; i++)
			{
				ref var item = ref entries[i];
				if (item.Next >= 0)
					target[added++] = values[i];
			}
			return count;
		}

		public Enumerator GetEnumerator() => new Enumerator(this);
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			private readonly FastDictionary<TKey, TValue> m_dictionary;
			private int m_index;
			private int m_uncounted;
			private KeyValuePair<TKey, TValue> m_current;

			public readonly KeyValuePair<TKey, TValue> Current => m_current;
			object IEnumerator.Current => m_current;

			internal Enumerator(FastDictionary<TKey, TValue> dictionary)
			{
				m_dictionary = dictionary;
				m_index = 0;
				m_uncounted = dictionary.m_count;
				m_current = default;
			}

			/// <summary>
			/// Move to next
			/// </summary>
			public bool MoveNext()
			{
				if (m_uncounted == 0)
				{
					m_current = default;
					return false;
				}

				m_uncounted--;

				var entries = m_dictionary.m_entries;
				var index = m_index;
				for (; ; )
				{
					ref var entry = ref entries[index];
					if (entry.Next >= 0)
					{
						m_current = new KeyValuePair<TKey, TValue>(
							entry.Key,
							m_dictionary.m_values[index]
						);
						m_index = index + 1;
						return true;
					}
					index++;
				}
			}

			void IEnumerator.Reset()
			{
				m_index = 0;
				m_uncounted = m_dictionary.m_count;
				m_current = default;
			}

			public void Dispose() { }
		}

		//
		// Values
		//

		public ValuesEnumerator Values => new ValuesEnumerator(this);

		public struct ValuesEnumerator : IEnumerator<TValue>, IEnumerable<TValue>
		{
			private readonly FastDictionary<TKey, TValue> m_dictionary;
			private int m_index;
			private int m_uncounted;
			private TValue m_current;

			public readonly TValue Current => m_current;
			object IEnumerator.Current => m_current;

			internal ValuesEnumerator(FastDictionary<TKey, TValue> dictionary)
			{
				m_dictionary = dictionary;
				m_index = 0;
				m_uncounted = dictionary.m_count;
				m_current = default;
			}

			/// <summary>
			/// Move to next
			/// </summary>
			public bool MoveNext()
			{
				if (m_uncounted == 0)
				{
					m_current = default;
					return false;
				}

				m_uncounted--;

				var entries = m_dictionary.m_entries;
				var index = m_index;
				for (; ; )
				{
					ref var entry = ref entries[index];
					if (entry.Next >= 0)
					{
						m_current = m_dictionary.m_values[index];
						m_index = index + 1;
						return true;
					}
					index++;
				}
			}

			void IEnumerator.Reset()
			{
				m_index = 0;
				m_uncounted = m_dictionary.m_count;
				m_current = default;
			}

			public void Dispose() { }

			public IEnumerator<TValue> GetEnumerator()
			{
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this;
			}
		}

		//
		// Keys
		//
		public KeysEnumerator Keys => new KeysEnumerator(this);

		public struct KeysEnumerator : IEnumerator<TKey>, IEnumerable<TKey>
		{
			private readonly FastDictionary<TKey, TValue> m_dictionary;
			private int m_index;
			private TKey m_current;

			public TKey Current => m_current;
			object IEnumerator.Current => m_current;

			internal KeysEnumerator(FastDictionary<TKey, TValue> dictionary)
			{
				m_dictionary = dictionary;
				m_index = 0;
				m_current = default;
			}

			[MethodImpl(MethodImplOptions.AggressiveOptimization)]
			public bool MoveNext()
			{
				var entries = m_dictionary.m_entries;
				int index = m_index;
				while (index < entries.Length)
				{
					ref Entry entry = ref entries[index++];
					if (entry.Next >= 0)
					{
						m_current = entry.Key;
						m_index = index;
						return true;
					}
				}
				m_current = default;
				return false;
			}

			void IEnumerator.Reset()
			{
				m_index = 0;
				m_current = default;
			}

			public void Dispose() { }

			public IEnumerator<TKey> GetEnumerator()
			{
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this;
			}
		}
	}
}
