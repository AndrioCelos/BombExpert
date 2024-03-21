using System;
using System.Collections.Generic;
using System.Linq;

namespace BombExpert; 
/// <summary>Emulates the pseudo-random number generator used by Unity.</summary>
public class MonoRandom : Random {
	private const int MSEED = 161803398;

	private int inext;
	private int inextp;
	private readonly int[] seedArray = new int[56];

	/// <summary>Returns the number of times <see cref="Sample"/> has been called. For debugging purposes.</summary>
	public int Count { get; private set; }

	/// <summary>Initialises a new <see cref="MonoRandom"/> using a time-dependent seed.</summary>
	public MonoRandom() : this(Environment.TickCount) { }

	/// <summary>Initialises a new <see cref="MonoRandom"/> using the specified seed.</summary>
	public MonoRandom(int seed) {
		seed = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
		int num = MSEED - seed;
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
				this.seedArray[k] += int.MaxValue;
		}
		this.inext = 0;
		this.inextp = 31;
	}

	/// <summary>Returns a non-negative random number that is less than 1.</summary>
	protected override double Sample() {
		++this.Count;
		if (++this.inext >= 56) this.inext = 1;
		if (++this.inextp >= 56) this.inextp = 1;
		var num = this.seedArray[this.inext] - this.seedArray[this.inextp];
		if (num < 0) num += int.MaxValue;
		this.seedArray[this.inext] = num;
		return num * (1.0 / int.MaxValue);
	}

	/// <summary>Returns a non-negative random number that is less than 1.</summary>
	public override double NextDouble() => this.Sample();

	/// <summary>Returns a non-negative random integer that is less than <see cref="int.MaxValue"/>.</summary>
	public override int Next() => this.Next(int.MaxValue);
	/// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
	public override int Next(int maxValue) => maxValue == 1 ? 0 : (int) Math.Floor(this.NextDouble() * maxValue);
	/// <summary>Returns a random integer that is between the specified minimum (inclusive) and the specified maximum (exclusive).</summary>
	public override int Next(int minValue, int maxValue) => maxValue - minValue <= 1 ? minValue : minValue + this.Next(maxValue - minValue);

	/// <summary>Returns an array that contains the elements from the specified enumerable in a random order.</summary>
	public T[] Shuffle<T>(IEnumerable<T> enumerable) => [.. enumerable.OrderBy(x => this.NextDouble())];
	/// <summary>Shuffles the elements of the specified list in place using a Fisher-Yates shuffle.</summary>
	public void ShuffleFisherYates<T>(IList<T> list) {
		var i = list.Count;
		while (i > 1) {
			var j = this.Next(i);
			--i;
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	/// <summary>Returns a random element from the specified list.</summary>
	/// <seealso cref="WeightMap{T, TKey}"/>
	public T Pick<T>(IList<T> list) => list[this.Next(list.Count)];
}
