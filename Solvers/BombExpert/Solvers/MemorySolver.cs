using Aiml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace BombExpert.Solvers; 
public class MemorySolver : IModuleSolver {
	private const int NumStages = 5;

	public static Instruction[,] GetRules(int ruleSeed) {
		var random = new MonoRandom(ruleSeed);
		var weights = new WeightMap<Instruction, string>(i => i.Key);
		var rules = new Instruction[NumStages, 4];
		for (int i = 0; i < NumStages; ++i) {
			for (int j = 0; j < 4; ++j) {
				var digit = random.Next(1, 5);
				var stage = random.Next(1, i + 1);

				var instructions = new List<Instruction>() {
					Instruction.Position1, Instruction.Position2, Instruction.Position3, Instruction.Position4, Instruction.PressDigit(digit)
				};
				if (i > 0) {
					var instruction = i % 2 == 0 ? Instruction.PressLabelFromStage(stage) :
						Instruction.PressPositionFromStage(stage);
					if (i > 2)
						weights.SetWeight(instruction, 1);
					instructions.Add(instruction);
				}

				var instruction2 = weights.Roll(instructions, random);
				rules[i, j] = instruction2;
			}
		}
		return rules;
	}

	public void GenerateAiml(string path, int ruleSeed) {
		var rules = GetRules(ruleSeed);

		using var writer = new StreamWriter(Path.Combine(path, "maps", $"Memory{ruleSeed}.txt"));
		for (int stage = 0; stage < 5; ++stage) {
			for (int display = 0; display < 4; ++display) {
				var instruction = rules[stage, display];
				writer.Write($"{stage + 1} {display + 1}:");
				writer.WriteLine(instruction.Key switch {
					"0" => "Position 1",
					"1" => "Position 2",
					"2" => "Position 3",
					"3" => "Position 4",
					"4" => "Label " + instruction.Number,
					"5" => "SamePosition " + instruction.Number,
					"6" => "SameLabel " + instruction.Number,
					_ => "?"
				});
			}
		}
	}

	public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
		var words = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
		if (words[1].Equals("GetRule", StringComparison.CurrentCultureIgnoreCase)) {
			// [rule seed] GetRule [stage] [display]
			var rules = GetRules(int.Parse(words[0]));
			var instruction = rules[int.Parse(words[2]) - 1, int.Parse(words[3]) - 1];
			return instruction.Key switch {
				"0" => "Position 1",
				"1" => "Position 2",
				"2" => "Position 3",
				"3" => "Position 4",
				"4" => "Label " + instruction.Number,
				"5" => "SamePosition " + instruction.Number,
				"6" => "SameLabel " + instruction.Number,
				_ => throw new InvalidOperationException("Unknown rule"),
			};
		}
		throw new ArgumentException("Unknown command");
	}

	public class Instruction {
		public delegate int SolutionDelegate(RequestProcess process, int[] numbers);
		public string Key { get; }
		public int Number { get; }
		public string Text { get; }
		public SolutionDelegate Delegate { get; }

		public Instruction(string key, int number, string text, SolutionDelegate solutionDelegate) {
			this.Key = key;
			this.Number = number;
			this.Text = text;
			this.Delegate = solutionDelegate;
		}

		public override string ToString() => this.Text;

		public static Instruction Position1 { get; } = new Instruction("0", 1, "press the button in the first position", (p, n) => n[1]);
		public static Instruction Position2 { get; } = new Instruction("1", 2, "press the button in the second position", (p, n) => n[2]);
		public static Instruction Position3 { get; } = new Instruction("2", 3, "press the button in the third position", (p, n) => n[3]);
		public static Instruction Position4 { get; } = new Instruction("3", 4, "press the button in the fourth position", (p, n) => n[4]);
		public static Instruction PressDigit(int digit) => new("4", digit, $"press the button labelled “{digit}”", (p, n) => digit);
		public static Instruction PressPositionFromStage(int stage) => new("5", stage, $"press the button in the same position as you pressed in stage {stage}",
			(p, n) => n[int.Parse(p.User.GetPredicate("BombMemoryPosition" + stage))]);
		public static Instruction PressLabelFromStage(int stage) => new("6", stage, $"press the button with the same label as you pressed in stage {stage}",
			(p, n) => int.Parse(p.User.GetPredicate("BombMemoryLabel" + stage)));
	}
}
