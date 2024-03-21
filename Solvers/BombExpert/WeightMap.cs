﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BombExpert; 
/// <summary>Represents a collection of weights used for weighted random selections.</summary>
/// <typeparam name="T">The type of the element being selected.</typeparam>
/// <typeparam name="TKey">The type of the keys used to associate weights to elements.</typeparam>
/// <remarks>
///		When performing a random selection, not all possible choices need have a weight assigned.
///		All elements have a weight of 1 by default.
///	</remarks>
public class WeightMap<T, TKey>(Func<T, TKey> keySelector) : IEnumerable<KeyValuePair<TKey, float>> where TKey : notnull {
	private readonly Dictionary<TKey, float> weights = [];
	private readonly Func<T, TKey> keySelector = keySelector;

	/// <summary>Returns the weight associated with the specified item (1 by default).</summary>
	public float GetWeight(T item) => this.weights.TryGetValue(this.keySelector(item), out var weight) ? weight : 1;
	/// <summary>Sets the weight for the specified item.</summary>
	public void SetWeight(T item, float weight) => this.weights[this.keySelector(item)] = weight;

	/// <summary>
	///		Selects a random element from the specified list with a weighted distribution,
	///		then multiplies the weight of the selected element by 0.05.
	///	</summary>
	public T Roll(IList<T> list, Random random)
		=> this.Roll(list, random, 0.05f);
	/// <summary>
	///		Selects a random element from the specified list with a weighted distribution,
	///		then multiplies the weight of the selected element by the specified number.
	///	</summary>
	public T Roll(IList<T> list, Random random, float weightReduction) {
		var roll = (float) (random.NextDouble() * list.Sum(this.GetWeight));
		foreach (var item in list) {
			var weight = this.GetWeight(item);
			roll -= weight;
			if (roll < 0) {
				this.SetWeight(item, weight * weightReduction);
				return item;
			}
		}
		return list[random.Next(list.Count)];
	}

	public IEnumerator<KeyValuePair<TKey, float>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, float>>) this.weights).GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, float>>) this.weights).GetEnumerator();
}
