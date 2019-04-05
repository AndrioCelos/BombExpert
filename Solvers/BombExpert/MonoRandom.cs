using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BombExpert {
	public class MonoRandom : Random {
		private const int MSEED = 161803398;
		private const int MBIG = int.MaxValue;
		private const int MZ = 0;

		private int inext;
		private int inextp;
		private readonly int[] seedArray = new int[56];
		private int count;

		public MonoRandom() : this(Environment.TickCount) { }

		public MonoRandom(int Seed) {
			Seed = Seed == int.MinValue ? int.MaxValue : Math.Abs(Seed);
			int num = MSEED - Seed;
			this.seedArray[55] = num;
			int num2 = 1;
			for (int i = 1; i < 55; i++) {
				int num3 = 21 * i % 55;
				this.seedArray[num3] = num2;
				num2 = num - num2;
				if (num2 < 0)
					num2 += 2147483647;
				num = this.seedArray[num3];
			}

			for (int j = 1; j < 5; j++) for (int k = 1; k < 56; k++) {
					this.seedArray[k] -= this.seedArray[1 + (k + 30) % 55];
					if (this.seedArray[k] < 0)
						this.seedArray[k] += 2147483647;
				}
			this.inext = 0;
			this.inextp = 31;
		}

		protected override double Sample() {
			++this.count;
			if (++this.inext >= 56) this.inext = 1;
			if (++this.inextp >= 56) this.inextp = 1;
			var num = this.seedArray[this.inext] - this.seedArray[this.inextp];
			if (num < 0) num += int.MaxValue;
			this.seedArray[this.inext] = num;
			return num * 4.6566128752457969e-10;
		}

		public override double NextDouble() => this.Sample();

		public override int Next() => this.Next(int.MaxValue);
		public override int Next(int maxValue) => maxValue == 1 ? 0 : (int) Math.Floor(this.NextDouble() * maxValue);
		public override int Next(int minValue, int maxValue) {
			if (maxValue - minValue <= 1) return minValue;
			return minValue + this.Next(maxValue - minValue);
		}

		public T[] Shuffle<T>(IEnumerable<T> enumerable) => enumerable.OrderBy(x => this.NextDouble()).ToArray();
		public void ShuffleFisherYates<T>(IList<T> list) {
			var i = list.Count;
			while (i > 1) {
				var j = this.Next(i);
				--i;
				(list[i], list[j]) = (list[j], list[i]);
			}
		}

		public T Pick<T>(IList<T> list) => list[this.Next(list.Count)];
	}
}
