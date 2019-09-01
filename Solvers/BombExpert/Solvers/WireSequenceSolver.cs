#nullable enable

using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BombExpert;

using static BombExpert.Solvers.WireSequenceSolver.Instruction;

namespace BombExpert.Solvers {
	public class WireSequenceSolver : ISraixService {
		private const int NUM_PAGES = 4;
		private const int NUM_PER_PAGE = 3;
		private const int NUM_COLOURS = 3;
		private const int BLANK_PAGE_COUNT = 1;

		private const int NumWiresPerColour = NUM_PER_PAGE * (NUM_PAGES - BLANK_PAGE_COUNT);

		public static RuleSet GetRules(int ruleSeed) {
			if (ruleSeed == 1) return new RuleSet(
				new[] { CutC, CutB, CutA, CutA | CutC, CutB, CutA | CutC, CutA | CutB | CutC, CutA | CutB, CutB },
				new[] { CutB, CutA | CutC, CutB, CutA, CutB, CutB | CutC, CutC, CutA | CutC, CutA },
				new[] { CutA | CutB | CutC, CutA | CutC, CutB, CutA | CutC, CutB, CutB | CutC, CutA | CutB, CutC, CutC }
			);

			var random = new MonoRandom(ruleSeed);
			return new RuleSet(
				GetInstructions(random),
				GetInstructions(random),
				GetInstructions(random)
			);
		}

		private static Instruction[] GetInstructions(Random random) {
			var instructions = new Instruction[NumWiresPerColour];
			for (int i = 0; i < NumWiresPerColour; ++i) {
				var flags = Never;
				for (int j = 0; j < 3; ++j) {
					if (random.NextDouble() > 0.55) flags |= (Instruction) (1 << j);
				}
				instructions[i] = flags;
			}
			return instructions;
		}

		/// <param name="text">[rule seed] [red total] [blue total] [black total] [colour] [letter]</param>
		public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
			var words = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
			var rules = GetRules(int.Parse(words[0]));

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

		[Flags]
		public enum Instruction {
			Never,
			CutA = 1,
			CutB = 2,
			CutC = 4
		}

		public class RuleSet {
			public Instruction[] RedRules { get; }
			public Instruction[] BlueRules { get; }
			public Instruction[] BlackRules { get; }

			public RuleSet(Instruction[] redRules, Instruction[] blueRules, Instruction[] blackRules) {
				this.RedRules = redRules;
				this.BlueRules = blueRules;
				this.BlackRules = blackRules;
			}

			public Instruction[] this[Colour colour] {
				get {
					switch (colour) {
						case Colour.Red: return this.RedRules;
						case Colour.Blue: return this.BlueRules;
						case Colour.Black: return this.BlackRules;
						default: throw new KeyNotFoundException();
					}
				}
			}
		}
	}
}
