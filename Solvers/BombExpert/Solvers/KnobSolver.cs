#nullable enable

using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BombExpert;
using System.IO;

namespace BombExpert.Solvers {
	public class KnobSolver : IModuleSolver {
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
				var (name, indices) = GetPattern(rules);
				return name + " " + string.Join(" ", indices);
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

		private static (string name, int[] indices) GetPattern(Rule[] rules) {
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
				if (valid) return (name, indices);
			}
			throw new InvalidOperationException("No valid pattern found for this rule seed?!");
		}

		public void GenerateAiml(string path, int ruleSeed) {
			var rules = GetRules(ruleSeed);

			using var writer = new StreamWriter(Path.Combine(path, "aiml", $"knob{ruleSeed}.aiml"));
			writer.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
			writer.WriteLine("<aiml version='2.0'>");

			var (name, indices) = GetPattern(rules);
			writer.WriteLine("<category>");
			writer.WriteLine($"<pattern>SolverFallback Knob {ruleSeed} GetPattern</pattern>");
			writer.WriteLine($"<template>{name} {string.Join(" ", indices)}</template>");
			writer.WriteLine("</category>");

			var used = new HashSet<string>();
			for (int i = 0; i < 8; ++i) {
				var rule = rules[i];
				var lights = string.Join(" ", indices.Select(j => j + ":" + (rule.Lights[j] ? "on" : "off")));

				if (used.Add(lights)) {
					writer.WriteLine("<category>");
					writer.WriteLine($"<pattern>SolverFallback Knob {ruleSeed} Lights {lights}</pattern>");
					writer.WriteLine($"<template>{rule.Position}</template>");
					writer.WriteLine("</category>");
				}
			}

			writer.WriteLine("<category>");
			writer.WriteLine($"<pattern>SolverFallback Knob {ruleSeed} Lights *</pattern>");
			writer.WriteLine($"<template>unknown</template>");
			writer.WriteLine("</category>");
			writer.WriteLine("</aiml>");
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
