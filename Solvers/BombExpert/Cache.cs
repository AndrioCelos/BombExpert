using System;
using System.Collections.Generic;

namespace BombExpert {
	/// <summary>Represents a collection that allows results of an expensive function to be cached.</summary>
	/// <typeparam name="TParam">The type of the input to the function.</typeparam>
	/// <typeparam name="TResult">The type of the output from the function.</typeparam>
	/// <remarks>
	///		The underlying function being cached by this object must be a pure function:
	///		that is, its output must depend only on the provided input and must have no side effects.
	/// </remarks>
	public class Cache<TParam, TResult> {
		/// <summary>The list of inputs in the order they were added.</summary>
		private readonly List<TParam> keys;
		private readonly Dictionary<TParam, TResult> dictionary;

		/// <summary>The maximum number of input-result pairs that can be cached before the oldest results are removed.</summary>
		public int Capacity {
			get => this.keys.Capacity;
			set => this.keys.Capacity = value;
		}
		public Func<TParam, TResult> Func { get; }

		public Cache(int capacity, Func<TParam, TResult> func) {
			this.keys = new List<TParam>(capacity);
			this.dictionary = new Dictionary<TParam, TResult>(capacity);
			this.Func = func;
		}
		public Cache(int capacity, IEqualityComparer<TParam> comparer, Func<TParam, TResult> func) {
			this.keys = new List<TParam>(capacity);
			this.dictionary = new Dictionary<TParam, TResult>(capacity, comparer);
			this.Func = func;
		}

		/// <summary>Returns the cached result if one exists; otherwise, runs the underlying function, adds the result to the cache and returns it.</summary>
		public TResult Get(TParam key) {
			if (this.dictionary.TryGetValue(key, out var value)) return value;
			return this.Add(key);
		}
		/// <summary>Refreshes the cached result for the specified input by calling the underlying function and returns the new result.</summary>
		public TResult Put(TParam key) {
			if (this.dictionary.ContainsKey(key)) {
				this.keys.Remove(key);
				this.keys.Add(key);
				return this.dictionary[key] = this.Func(key);
			} else {
				return this.Add(key);
			}
		}

		/// <summary>Calls the underlying function and caches the result.</summary>
		private TResult Add(TParam key) {
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
