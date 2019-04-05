using System;
using System.Collections.Generic;
using System.Text;

namespace BombExpert {
	public class Cache<TKey, TValue> {
		private List<TKey> keys;
		private Dictionary<TKey, TValue> dictionary;

		public Func<TKey, TValue> Func { get; }

		public Cache(int capacity, Func<TKey, TValue> func) {
			this.keys = new List<TKey>(capacity);
			this.dictionary = new Dictionary<TKey, TValue>(capacity);
			this.Func = func;
		}
		public Cache(int capacity, IEqualityComparer<TKey> comparer, Func<TKey, TValue> func) {
			this.keys = new List<TKey>(capacity);
			this.dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
			this.Func = func;
		}

		public TValue Get(TKey key) {
			if (this.dictionary.TryGetValue(key, out var value)) return value;
			return this.Add(key);
		}
		public TValue Put(TKey key) {
			if (this.dictionary.ContainsKey(key)) {
				this.keys.Remove(key);
				this.keys.Add(key);
				return this.dictionary[key] = this.Func(key);
			} else {
				return this.Add(key);
			}
		}

		private TValue Add(TKey key) {
			if (this.keys.Count >= this.keys.Capacity) {
				var key2 = this.keys[0];
				this.keys.RemoveAt(0);
				this.dictionary.Remove(key2);
			}
			this.keys.Add(key);
			return this.dictionary[key] = this.Func(key);
		}
	}
}
