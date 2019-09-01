#nullable enable

using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BombExpert;

using static BombExpert.Solvers.ComplicatedWiresSolver.Flags;

namespace BombExpert.Solvers {
	public class ComplicatedWiresSolver : ISraixService {
		private static readonly Instruction[] instructions = new[] { Instruction.Cut, Instruction.DoNotCut, Instruction.CutIfSerialNumberEven, Instruction.CutIfParallelPort, Instruction.CutIfTwoOrMoreBatteries };

		public static Instruction[] GetRules(int seed) {
			var array = new Instruction[16];
			var weights = new Dictionary<Instruction, double>();
			var random = new MonoRandom(seed);

			for (int i = 1; i < 16; ++i) {
				array[i] = GetInstruction(weights, random);
			}
			if (seed != 1 && array.Count(i => i == Instruction.Cut) >= 2)
				array[0] = GetInstruction(weights, random);
			return array;
		}

		private static Instruction GetInstruction(Dictionary<Instruction, double> weights, Random random) {
			return Utils.WeightedRoll(instructions, weights, random, i => i, 0.1);
		}

		public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
			var inputWords = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
			var rules = GetRules(int.Parse(inputWords[0]));
			var response = new StringBuilder();
			int wireNumber = 0;
			int wiresToCut = 0;
			int wiresToNotCut = 0;

			for (int pos = 1; pos < inputWords.Length; ++pos) {
				var flags = (Flags) 0;

				for (; pos < inputWords.Length; ++pos) {
					if (inputWords[pos].Equals("next", StringComparison.InvariantCultureIgnoreCase)) break;
					if (inputWords[pos].Equals("then", StringComparison.InvariantCultureIgnoreCase)) break;
					if (inputWords[pos].Equals("nothing", StringComparison.InvariantCultureIgnoreCase)) continue;
					if (inputWords[pos].Equals("plain", StringComparison.InvariantCultureIgnoreCase)) continue;
					if (inputWords[pos].Equals("white", StringComparison.InvariantCultureIgnoreCase)) continue;
					if (inputWords[pos].StartsWith("stripe", StringComparison.InvariantCultureIgnoreCase)) continue;
					flags |= (Flags) Enum.Parse(typeof(Flags), inputWords[pos], true);
				}


				bool cut;

				var instruction = rules[(int) flags];
				switch (instruction) {
					case Instruction.Cut: cut = true; break;
					case Instruction.DoNotCut: cut = false; break;
					case Instruction.CutIfSerialNumberEven:
						var predicate = process.User.GetPredicate("BombSerialNumberIsOdd");
						if (predicate.Equals("unknown", StringComparison.CurrentCultureIgnoreCase)) return "NeedEdgework SerialNumberIsOdd";
						cut = !bool.Parse(predicate);
						break;
					case Instruction.CutIfParallelPort:
						predicate = process.User.GetPredicate("BombPortParallel");
						if (predicate.Equals("unknown", StringComparison.CurrentCultureIgnoreCase)) return "NeedEdgework Port Parallel";
						cut = bool.Parse(predicate);
						break;
					case Instruction.CutIfTwoOrMoreBatteries:
						predicate = process.User.GetPredicate("BombBatteryCount");
						if (predicate.Equals("unknown", StringComparison.CurrentCultureIgnoreCase)) return "NeedEdgework BatteryCount";
						cut = int.Parse(predicate) >= 2;
						break;
					default:
						throw new InvalidOperationException("Unknown instruction");
				}

				if (cut) {
					++wiresToCut;
					if (response.Length > 0) response.Append(", ");
					switch (wireNumber) {
						case 0: response.Append("the first wire"); break;
						case 1: response.Append("the second wire"); break;
						case 2: response.Append("the third wire"); break;
						case 3: response.Append("the fourth wire"); break;
						case 4: response.Append("the fifth wire"); break;
						case 5: response.Append("the sixth wire"); break;
						default: return "There should not be that many wires.";
					}
				} else
					++wiresToNotCut;

				++wireNumber;
			}

			if (wiresToCut == 0) {
				if (wiresToNotCut == 1) return "Do not cut the wire.";
				return "Do not cut those wires.";
			}
			if (wiresToCut == 1) {
				if (wiresToNotCut == 0) return "Cut the wire.";
				return "Cut " + response.ToString() + ".";
			}
			if (wiresToNotCut == 0) {
				if (wiresToCut == 2) return "Cut both wires.";
				return "Cut all wires.";
			}
			return "Cut the following wires: " + response.ToString();
		}

		[Flags]
		public enum Flags {
			Light = 1,
			Star = 2,
			Blue = 4,
			Red = 8
		}

		public enum Instruction {
			Cut,
			DoNotCut,
			CutIfSerialNumberEven,
			CutIfParallelPort,
			CutIfTwoOrMoreBatteries
		}
	}
}
