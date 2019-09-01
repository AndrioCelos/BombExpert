#nullable enable

using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BombExpert;

namespace BombExpert.Solvers {
	public class KnobSolver : ISraixService {
		public static Rule[] GetRules(int ruleSeed) {
			var random = new MonoRandom(ruleSeed);

			var unusedRows = new List<int>(Enumerable.Range(0, 64));
			var potentials = new List<int>();
			do {
				var row1 = Utils.RemoveRandom(unusedRows, random);
				var row2 = Utils.RemoveRandom(unusedRows, random);
				var row3 = Utils.RemoveRandom(unusedRows, random);

				var pattern1 = row1 << 6 | row2;
				var pattern2 = row1 << 6 | row3;

				var row4 = Utils.RemoveRandom(unusedRows, random);
				var row5 = Utils.RemoveRandom(unusedRows, random);
				var row6 = Utils.RemoveRandom(unusedRows, random);

				var pattern3 = row5 << 6 | row4;
				var pattern4 = row6 << 6 | row4;

				potentials.Add(pattern1);
				potentials.Add(pattern2);
				potentials.Add(pattern3);
				potentials.Add(pattern4);
			}
			while (potentials.Count < 8);

			var rules = new Rule[8];
			for (var i = 0; i < 8; i++)
				rules[i] = new Rule((Position) (i / 2), Utils.RemoveRandom(potentials, random));

			return rules;
		}

		private static readonly List<(string name, int[] indices)> patterns = new List<(string name, int[] indices)>() {
			("Top", new[] { 0, 1, 2, 3, 4, 5 } ),
			("Bottom", new[] { 6, 7, 8, 9, 10, 11 } ),
			("Left", new[] { 0, 1, 2, 6, 7, 8 } ),
			("Right", new[] { 3, 4, 5, 9, 10, 11 } ),
			("Outer", new[] { 0, 5, 6, 11 } ),
			("Middle", new[] { 1, 4, 7, 10 } ),
			("Inner", new[] { 2, 3, 8, 9 } ),
			("All", new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } )
		};

		/// <param name="text">[rule seed] GetPattern -or- [rule seed] Lights ( [light number]:[state] )*</param>
		public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
			var fields = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
			var rules = GetRules(int.Parse(fields[0]));

			if (fields[1].Equals("GetPattern", StringComparison.InvariantCultureIgnoreCase)) {
				// Find the first pattern that uniquely determines the required position.
				foreach (var (name, indices) in patterns) {
					var valid = true;
					var mapping = new Dictionary<string, Position>();
					foreach (var rule in rules) {
						var key = string.Join("", indices.Select(i => rule.Lights[i] ? '1' : '0'));
						if (mapping.TryGetValue(key, out var position)) {
							if (position != rule.Position) {
								valid = false;
								break;
							}
						} else
							mapping[key] = rule.Position;
					}
					if (valid) return name + " " + string.Join(" ", indices);
				}
				throw new InvalidOperationException("No valid pattern found for this rule seed?!");
			}

			var known = new List<(int index, bool on)>();

			for (int i = 2; i < fields.Length; ++i) {
				var fields2 = fields[i].Split(new[] { ':' });
				known.Add((int.Parse(fields2[0]), fields2[1].Equals("on", StringComparison.CurrentCultureIgnoreCase)));
			}

			Position? position2 = null;

			foreach (var rule in rules) {
				if (known.All(e => rule.Lights[e.index] == e.on)) {
					if (position2 != null && position2 != rule.Position)
						return "MultiplePositions";
					position2 = rule.Position;
				}
			}

			return position2?.ToString()?.ToLower() ?? "unknown";
		}

		public enum Position {
			Up,
			Down,
			Left,
			Right
		}

		public class Rule {
			public bool[] Lights;
			public Position Position;

			public Rule(Position position, params bool[] lights) {
				this.Lights = lights;
				this.Position = position;
			}

			public Rule(Position position, int lights) {
				this.Position = position;
				this.Lights = new bool[12];
				for (int i = 0; i < 12; ++i) {
					this.Lights[i] = (lights & 1) != 0;
					lights >>= 1;
				}
			}
		}
	}
}
