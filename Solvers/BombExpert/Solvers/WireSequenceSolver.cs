using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Aiml;
using static BombExpert.Solvers.WireSequenceSolver.Instruction;

namespace BombExpert.Solvers;
public class WireSequenceSolver : IModuleSolver {
	private const int NUM_PAGES = 4;
	private const int NUM_PER_PAGE = 3;
	private const int BLANK_PAGE_COUNT = 1;

	private const int NUM_WIRES_PER_COLOUR = NUM_PER_PAGE * (NUM_PAGES - BLANK_PAGE_COUNT);

	public static RuleSet GetRules(int ruleSeed) {
		if (ruleSeed == 1) return new RuleSet(
			[CutC, CutB, CutA, CutA | CutC, CutB, CutA | CutC, CutA | CutB | CutC, CutA | CutB, CutB],
			[CutB, CutA | CutC, CutB, CutA, CutB, CutB | CutC, CutC, CutA | CutC, CutA],
			[CutA | CutB | CutC, CutA | CutC, CutB, CutA | CutC, CutB, CutB | CutC, CutA | CutB, CutC, CutC]
		);

		var random = new MonoRandom(ruleSeed);
		return new RuleSet(
			GetInstructions(random),
			GetInstructions(random),
			GetInstructions(random)
		);
	}

	private static Instruction[] GetInstructions(Random random) {
		var instructions = new Instruction[NUM_WIRES_PER_COLOUR];
		for (int i = 0; i < NUM_WIRES_PER_COLOUR; ++i) {
			var flags = Never;
			for (int j = 0; j < 3; ++j) {
				if (random.NextDouble() > 0.55) flags |= (Instruction) (1 << j);
			}
			instructions[i] = flags;
		}
		return instructions;
	}

	public void GenerateAiml(string path, int ruleSeed) {
		var rules = GetRules(ruleSeed);

		using var writer = new StreamWriter(Path.Combine(path, "maps", $"WireSequence{ruleSeed}.txt"));
		void write(string colour, Instruction[] instructions) {
			for (int i = 0; i < instructions.Length; ++i) {
				var instruction = instructions[i];
				writer.Write($"{colour} {i + 1}:");
				if (instruction == 0) writer.WriteLine("nil");
				else {
					if (instruction.HasFlag(CutA)) writer.Write("A ");
					if (instruction.HasFlag(CutB)) writer.Write("B ");
					if (instruction.HasFlag(CutC)) writer.Write("C ");
					writer.WriteLine();
				}
			}
		}
		write("red", rules.RedRules);
		write("blue", rules.BlueRules);
		write("black", rules.BlackRules);
	}

	/// <param name="text">[rule seed] [red total] [blue total] [black total] [colour] [letter]</param>
	public string Process(string text, XElement element, RequestProcess process) {
		var words = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
		var rules = GetRules(int.Parse(words[0]));

		if (words[1].Equals("GetRule", StringComparison.InvariantCultureIgnoreCase)) {
			var colour = (Colour) Enum.Parse(typeof(Colour), words[2], true);
			var total = int.Parse(words[3]);

			var ruleSet = colour == Colour.Red ? rules.RedRules : colour == Colour.Blue ? rules.BlueRules : rules.BlackRules;
			var cut = ruleSet[total - 1];

			var result = string.Join(' ', Enumerable.Range(0, 3).Where(i => cut.HasFlag((Instruction) (1 << i))).Select(i => (char) ('A' + i)));
			return result != "" ? result : "nil";
		} else {
			var redWireCount = int.Parse(words[1]);
			var blueWireCount = int.Parse(words[2]);
			var blackWireCount = int.Parse(words[3]);

			var colour = (Colour) Enum.Parse(typeof(Colour), words[4], true);
			var letter = char.ToUpperInvariant(words[5][0]);

			var ruleSet = colour == Colour.Red ? rules.RedRules : colour == Colour.Blue ? rules.BlueRules : rules.BlackRules;
			var total = colour == Colour.Red ? redWireCount : colour == Colour.Blue ? blueWireCount : blackWireCount;
			var cut = ruleSet[total - 1].HasFlag(letter == 'A' ? CutA : letter == 'B' ? CutB : CutC);
			return cut ? "true" : "false";
		}
	}

	[Flags]
	public enum Instruction {
		Never,
		CutA = 1,
		CutB = 2,
		CutC = 4
	}

	public record RuleSet(Instruction[] RedRules, Instruction[] BlueRules, Instruction[] BlackRules) {
		public Instruction[] this[Colour colour] => colour switch {
			Colour.Red => this.RedRules,
			Colour.Blue => this.BlueRules,
			Colour.Black => this.BlackRules,
			_ => throw new KeyNotFoundException(),
		};
	}
}
