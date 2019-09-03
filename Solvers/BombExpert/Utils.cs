using System;
using System.Collections.Generic;

namespace BombExpert {
	public class Utils {
		internal const int CacheSize = 64;

		public static string[] IndicatorLabels = new[] { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK" };

		public static string GetPortDescriptionAn(PortType portType)
			=> portType switch {
				PortType.DviD => "a DVI-D port",
				PortType.Parallel => "a parallel port",
				PortType.PS2 => "a PS/2 port",
				PortType.RJ45 => "an RJ-45 port",
				PortType.Serial => "a serial port",
				PortType.StereoRCA => "a stereo RCA port",
				_ => throw new ArgumentException("Unknown port type", nameof(portType)),
			};

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
	}
}
