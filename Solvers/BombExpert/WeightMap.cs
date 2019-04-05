using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombExpert {
    public class WeightMap<T, TKey> : IEnumerable<KeyValuePair<TKey, float>> {
		private readonly Dictionary<TKey, float> weights = new Dictionary<TKey, float>();
		private readonly Func<T, TKey> keySelector;

		public WeightMap(Func<T, TKey> keySelector) {
			this.keySelector = keySelector;
		}

		public float GetWeight(T item) {
			if (this.weights.TryGetValue(this.keySelector(item), out var weight)) return weight;
			return 1;
		}
		public void SetWeight(T item, float weight)
			=> this.weights[this.keySelector(item)] = weight;

		public T Roll(IList<T> list, Random random)
			=> this.Roll(list, random, 0.05f);
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
}
