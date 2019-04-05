using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombExpert {
	public class Utils {
		internal const int CacheSize = 64;

		public static string[] IndicatorLabels = new[] { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK" };

		public static string GetPortDescriptionAn(PortType portType) {
			switch (portType) {
				case PortType.DviD: return "a DVI-D port";
				case PortType.Parallel: return "a parallel port";
				case PortType.PS2: return "a PS/2 port";
				case PortType.RJ45: return "an RJ-45 port";
				case PortType.Serial: return "a serial port";
				case PortType.StereoRCA: return "a stereo RCA port";
				default: throw new ArgumentException("Unknown port type", nameof(portType));
			}
		}

		public static void StableSort<T, TKey>(IList<T> list, Func<T, TKey> key) where TKey : IComparable<TKey> {
			for (int i = 1; i < list.Count; ++i) {
				for (int j = i - 1; j >= 0; --j) {
					if (key(list[j]).CompareTo(key(list[j + 1])) <= 0) break;
					var hold = list[j];
					list[j] = list[j + 1];
					list[j + 1] = hold;
				}
			}
		}

		public static T RemoveRandom<T>(IList<T> list, Random random) {
			var i = random.Next(list.Count);
			var item = list[i];
			list.RemoveAt(i);
			return item;
		}

		[Obsolete("Use WeightMap instead.")]
		public static T WeightedRoll<T, TKey>(IList<T> list, IDictionary<TKey, double> weights, Random random, Func<T, TKey> keySelector, double multiplier = 0.05) {
			double getWeight(T item) {
				if (weights.TryGetValue(keySelector(item), out var weight)) return weight;
				return 1;
			}

			var num = list.Sum(getWeight);
			var num2 = random.NextDouble() * num;
			foreach (var item in list) {
				var weight = getWeight(item);
				num2 -= weight;
				if (num2 < 0) {
					weights[keySelector(item)] = weight * multiplier;
					return item;
				}
			}
			return list[random.Next(list.Count)];
		}
	}
}
