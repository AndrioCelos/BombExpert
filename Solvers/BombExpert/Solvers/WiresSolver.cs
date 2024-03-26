using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Aiml;

namespace BombExpert.Solvers;
public class WiresSolver : IModuleSolver {
	private static readonly Colour[] colours = [Colour.Black, Colour.Blue, Colour.Red, Colour.White, Colour.Yellow];

	private static readonly InstructionType CutWire1 = new(nameof(CutWire1), "cut the first wire", (p, c, w) => Result.CutFirstWire);
	private static readonly InstructionType CutWire2 = new(nameof(CutWire2), "cut the second wire", (p, c, w) => Result.CutSecondWire);
	private static readonly InstructionType CutWire3 = new(nameof(CutWire3), "cut the third wire", (p, c, w) => Result.CutThirdWire);
	private static readonly InstructionType CutWire4 = new(nameof(CutWire4), "cut the fourth wire", (p, c, w) => Result.CutFourthWire);
	private static readonly InstructionType CutWire5 = new(nameof(CutWire5), "cut the fifth wire", (p, c, w) => Result.CutFifthWire);
	private static readonly InstructionType CutWireLast = new(nameof(CutWireLast), "cut the last wire", (p, c, w) => Result.CutLastWire);
	private static readonly InstructionType CutWireColourSingle = new(nameof(CutWireColourSingle), "cut the {0} wire", ProcessCutWireColourFirst);
	private static readonly InstructionType CutWireColourFirst = new(nameof(CutWireColourFirst), "cut the first {0} wire", ProcessCutWireColourFirst);
	private static readonly InstructionType CutWireColourLast = new(nameof(CutWireColourLast), "cut the last {0} wire", ProcessCutWireColourLast);

	private static Result ProcessCutWireColourFirst(RequestProcess p, Colour colour, Colour[] wires) {
		var index = Array.IndexOf(wires, colour);
		return index < 0 ? throw new ArgumentException("No wires of the specified colour were found.")
			: index == wires.Length - 1 ? Result.CutLastWire
			: Result.CutFirstWire + index;
	}
	private static Result ProcessCutWireColourLast(RequestProcess p, Colour colour, Colour[] wires) {
		var index = Array.LastIndexOf(wires, colour);
		return index < 0 ? throw new ArgumentException("No wires of the specified colour were found.")
			: index == wires.Length - 1 ? Result.CutLastWire
			: Result.CutFirstWire + index;
	}

	private static ConditionType ExactlyOneColourWire => new(nameof(ExactlyOneColourWire), "there is exactly one {0} wire", true, 1,
		(colour, wires) => wires.Count(wire => wire == colour) == 1, CutWireColourSingle);
	private static ConditionType MoreThanOneColourWire => new(nameof(MoreThanOneColourWire), "there is more than one {0} wire", true, 2,
		(colour, wires) => wires.Count(wire => wire == colour) > 1, CutWireColourFirst, CutWireColourLast);
	private static ConditionType NoColourWire => new(nameof(NoColourWire), "there are no {0} wires", false, 0,
		(colour, wires) => !wires.Contains(colour));
	private static ConditionType LastWireIsColour => new(nameof(LastWireIsColour), "the last wire is {0}", true, 1,
		(colour, wires) => wires[^1] == colour);

	public string Process(string text, XElement element, RequestProcess process) {
		var fields = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
		var ruleSeed = int.Parse(fields[0]);
		var rules = GetRules(ruleSeed);

		if (fields.Length > 1 && fields[1].Equals("GetRule", StringComparison.InvariantCultureIgnoreCase)) {
			var numberOfWires = int.Parse(fields[3]);
			var ruleIndex = int.Parse(fields[4]) - 1;
			var ruleList = rules[numberOfWires - 3];
			if (fields[2].Equals("Condition", StringComparison.InvariantCultureIgnoreCase)) {
				var conditionIndex = int.Parse(fields[5]) - 1;
				if (ruleIndex >= ruleList.Length)
					return "nil";
				var queries = ruleList[ruleIndex].Queries;
				return conditionIndex >= queries.Length ? "nil" :
					queries[conditionIndex] is WireCondition ? $"Query {queries[conditionIndex].Code}" : $"EdgeworkQuery {queries[conditionIndex].Code}";
			} else {
				var solution = ruleList[ruleIndex].Solution;
				return solution.Colour != null ? $"{solution.Type.Key} {solution.Colour}" : solution.Type.Key;
			}
		} else {
			var colours = (from s in fields.Skip(1) select (Colour) Enum.Parse(typeof(Colour), s, true)).ToArray();

			foreach (var rule in rules[colours.Length - 3]) {
				var conditionResult = ConditionResult.FromBool(true);
				foreach (var condition in rule.Queries) {
					conditionResult = conditionResult && condition.Query(process, colours);
				}
				if (conditionResult) {
					var result = rule.Solution.Type.SolutionDelegate(process, rule.Solution.Colour ?? 0, colours);
					return result.ToString();
				} else if (conditionResult.Code == ConditionResultCode.Unknown) {
					return conditionResult.Details!;
				}
			}

			throw new InvalidOperationException("No rules matched?!");
		}
	}

	public static Rule[][] GetRules(int ruleSeed) {
		if (ruleSeed == 1) {
			return [
				// 3 wires
				[
					new Rule([NoColourWire.GetCondition(Colour.Red)], new Instruction(CutWire2)),
					new Rule([LastWireIsColour.GetCondition(Colour.White)], new Instruction(CutWireLast)),
					new Rule([MoreThanOneColourWire.GetCondition(Colour.Blue)], new Instruction(CutWireColourLast, Colour.Blue)),
					new Rule(new Instruction(CutWireLast))
				],
				// 4 wires
				[
					new Rule([MoreThanOneColourWire.GetCondition(Colour.Red), Condition<Colour[]>.SerialNumberIsOdd], new Instruction(CutWireColourLast, Colour.Red)),
					new Rule([LastWireIsColour.GetCondition(Colour.Yellow), NoColourWire.GetCondition(Colour.Red)], new Instruction(CutWire1)),
					new Rule([ExactlyOneColourWire.GetCondition(Colour.Blue)], new Instruction(CutWire1)),
					new Rule([MoreThanOneColourWire.GetCondition(Colour.Yellow)], new Instruction(CutWireLast)),
					new Rule(new Instruction(CutWire2))
				],
				// 5 wires
				[
					new Rule([LastWireIsColour.GetCondition(Colour.Black), Condition<Colour[]>.SerialNumberIsOdd], new Instruction(CutWire4)),
					new Rule([ExactlyOneColourWire.GetCondition(Colour.Red), MoreThanOneColourWire.GetCondition(Colour.Yellow)], new Instruction(CutWire1)),
					new Rule([NoColourWire.GetCondition(Colour.Black)], new Instruction(CutWire2)),
					new Rule(new Instruction(CutWire1))
				],
				// 6 wires
				[
					new Rule([NoColourWire.GetCondition(Colour.Yellow), Condition<Colour[]>.SerialNumberIsOdd], new Instruction(CutWire3)),
					new Rule([ExactlyOneColourWire.GetCondition(Colour.Yellow), MoreThanOneColourWire.GetCondition(Colour.White)], new Instruction(CutWire4)),
					new Rule([NoColourWire.GetCondition(Colour.Red)], new Instruction(CutWireLast)),
					new Rule(new Instruction(CutWire4))
				]
			];
		}

		var random = new MonoRandom(ruleSeed);
		var ruleSets = new Rule[4][];
		for (int i = 3; i <= 6; ++i) {
			ConditionType[][] list;
			var conditionWeights = new WeightMap<ConditionType, string>(c => c.Key);
			var instructionWeights = new WeightMap<InstructionType, string>(s => s.Key);
			var list2 = new List<Rule>();
			list = ruleSeed == 1
				? new[] {
					new[] { new ConditionType(Condition<Colour[]>.SerialNumberStartsWithLetter), new ConditionType(Condition<Colour[]>.SerialNumberIsOdd) },
					new[] { ExactlyOneColourWire, NoColourWire, LastWireIsColour, MoreThanOneColourWire }
				}
				: [
					[new ConditionType(Condition<Colour[]>.SerialNumberStartsWithLetter), new ConditionType(Condition<Colour[]>.SerialNumberIsOdd)],
					[ExactlyOneColourWire, NoColourWire, LastWireIsColour, MoreThanOneColourWire],
					[
						new ConditionType(new Conditions.PortCondition<Colour[]>(PortType.DviD, true)),
						new ConditionType(new Conditions.PortCondition<Colour[]>(PortType.PS2, true)),
						new ConditionType(new Conditions.PortCondition<Colour[]>(PortType.RJ45, true)),
						new ConditionType(new Conditions.PortCondition<Colour[]>(PortType.StereoRCA, true)),
						new ConditionType(new Conditions.PortCondition<Colour[]>(PortType.Parallel, true)),
						new ConditionType(new Conditions.PortCondition<Colour[]>(PortType.Serial, true)),
						new ConditionType(Condition<Colour[]>.EmptyPortPlate)
					]
				];

			var rules = new Rule[random.NextDouble() < 0.6 ? 3 : 4];
			ruleSets[i - 3] = rules;
			for (int j = 0; j < rules.Length; ++j) {
				var colours = new List<Colour>(WiresSolver.colours);
				var list3 = new List<Colour>();

				var conditions = new Condition<Colour[]>[random.NextDouble() < 0.6 ? 1 : 2];
				var num = i - 1;
				for (int k = 0; k < conditions.Length; ++k) {
					var possibleQueryableProperties = GetPossibleConditions(list, num, k > 0);
#if TRACE
					if (System.Diagnostics.Debugger.IsAttached) {
						System.Diagnostics.Trace.WriteLine("queryWeights:");
						foreach (var entry in conditionWeights)
							System.Diagnostics.Trace.WriteLine($"  -- {entry.Key} = {entry.Value}");
					}
#endif
					var conditionType = conditionWeights.Roll(possibleQueryableProperties, random, 0.1f);
					if (conditionType.UsesColour) {
						num -= conditionType.WiresInvolved;

						var i4 = random.Next(0, colours.Count);
						var colour = colours[i4];
						colours.RemoveAt(i4);
						if (conditionType.ColourAvailableForSolution)
							list3.Add(colour);

						conditions[k] = conditionType.GetCondition(colour);
					} else
						conditions[k] = conditionType.GetCondition(0);
				}

				var instructions = GetPossibleInstructions(i, conditions);
				Instruction solution;
				var instructionType = instructionWeights.Roll(instructions, random);
				solution = list3.Count > 0 ? new Instruction(instructionType, list3[random.Next(0, list3.Count)]) : new Instruction(instructionType);

				var rule = new Rule(conditions, solution);
				if (ruleSeed == 1 || rule.IsValid)
					list2.Add(rule);
				else
					--j;
			}

			Utils.StableSort(list2, r => -r.Queries.Length);

			var list4 = GetPossibleInstructions(i, null);
			if (ruleSeed != 1) {
				// Remove redundant rules.
				var forbiddenId = list2[^1].Solution.Type.Key;
				list4.RemoveAll(l => l.Key == forbiddenId);
			}
			list2.Add(new Rule(new Instruction(random.Pick(list4), null)));
			ruleSets[i - 3] = [.. list2];
		}
		return ruleSets;
	}

	private static List<ConditionType> GetPossibleConditions(IList<IList<ConditionType>> lists, int wiresAvailableInQuery, bool edgeworkAllowed) {
		var list = new List<ConditionType>();
		for (var i = 0; i < lists.Count; i++) {
			for (var j = 0; j < lists[i].Count; j++) {
				var conditionType = lists[i][j];
				if ((edgeworkAllowed || conditionType.UsesColour) && conditionType.WiresInvolved <= wiresAvailableInQuery)
				list.Add(conditionType);
			}
		}

		return list;
	}

		private static List<InstructionType> GetPossibleInstructions(int wireCount, IEnumerable<Condition<Colour[]>>? conditions) {
		var list = new List<InstructionType> { CutWire1, CutWire2, CutWireLast };
		if (wireCount >= 4) list.Add(CutWire3);
		if (wireCount >= 5) list.Add(CutWire4);
		if (wireCount >= 6) list.Add(CutWire5);

		if (conditions != null) {
			foreach (var condition in conditions) {
				if (condition is WireCondition wireCondition && wireCondition.Type.AdditionalSolutions != null)
					list.AddRange(wireCondition.Type.AdditionalSolutions);
			}
		}

		return list;
	}

	public void GenerateAiml(string path, int ruleSeed) {
		var rules = GetRules(ruleSeed);
		var allStars = new StringBuilder("<star/> <star index='2'/>");
		var allWires = new StringBuilder();
		var builder = new StringBuilder();
		builder.Append("<?xml version='1.0' encoding='UTF-8'?>\n<aiml>\n");

		for (int i = 0; i < 4; ++i) {
			var wireCount = i + 3;
			allStars.Append($" <star index='{wireCount}'/>");
			allWires.Append(i switch {
				0 => "CutFirstWire CutSecondWire",
				1 => " CutThirdWire",
				2 => " CutFourthWire",
				3 => " CutFifthWire",
				_ => ""
			});

			builder.Append($"<category>\n<!-- {wireCount} wires -->\n<pattern>SolverFallback Wires {ruleSeed}");
			for (int j = 0; j < wireCount; ++j)
				builder.Append(" <set>BombColours</set>");
			builder.AppendLine("</pattern>\n<template>\n<think>\n<set var='result'>unknown</set>");

			var otherwise = false;
			foreach (var rule in rules[i]) {
				var closeTags = new Stack<string>();
				builder.AppendLine($"<!-- {(otherwise ? "Otherwise, " : "")}{rule} -->\n<condition var='result' value='unknown'>");
				otherwise = true;
				foreach (var condition in rule.Queries) {
					switch (condition) {
						case WireCondition wireCondition:
							switch (wireCondition.Key) {
								case nameof(ExactlyOneColourWire):
									builder.AppendLine($"<set var='temp'><srai>XCountMatch {wireCondition.Colour} XS {allStars}</srai></set>\n<condition var='temp' value='1'>");
									break;
								case nameof(MoreThanOneColourWire):
									builder.AppendLine($"<set var='temp'><srai>XCompareDigits <srai>XCountMatch {wireCondition.Colour} XS {allStars}</srai> XS 1</srai></set>\n<condition var='temp' value='1'>");
									break;
								case nameof(NoColourWire):
									builder.AppendLine($"<set var='temp'><srai>XContains {wireCondition.Colour} XS {allStars}</srai></set>\n<condition var='temp' value='false'>");
									break;
								case nameof(LastWireIsColour):
									builder.AppendLine($"<set var='temp'><star index='{wireCount}'/></set>\n<condition var='temp' value='{wireCondition.Colour}'>");
									break;
								default:
									throw new InvalidOperationException("Unknown wire condition");
							}
							closeTags.Push("</condition>");
							break;
						case Conditions.SerialNumberStartsWithLetterCondition<Colour[]> _:
							builder.AppendLine("<condition name='SerialNumberStartsWithLetter'>\n<li value='true'>");
							closeTags.Push("</li>\n<li value='unknown'><set var='result'>NeedEdgework SerialNumberStartsWithLetter</set></li>\n</condition>");
							break;
						case Conditions.SerialNumberParityCondition<Colour[]> _:
							builder.AppendLine("<condition name='SerialNumberIsOdd'>\n<li value='true'>");
							closeTags.Push("</li>\n<li value='unknown'><set var='result'>NeedEdgework SerialNumberIsOdd</set></li>\n</condition>");
							break;
						case Conditions.PortCondition<Colour[]> portCondition:
							builder.AppendLine($"<condition name='Port{portCondition.PortType}'>\n<li value='true'>");
							closeTags.Push($"</li>\n<li value='unknown'><set var='result'>NeedEdgework Port {portCondition.PortType}</set></li>\n</condition>");
							break;
						case Conditions.EmptyPortPlateCondition<Colour[]> _:
							builder.AppendLine("<condition name='EmptyPortPlate'>\n<li value='true'>");
							closeTags.Push("</li>\n<li value='unknown'><set var='result'>NeedEdgework EmptyPortPlate</set></li>\n</condition>");
							break;
						default:
							throw new InvalidOperationException("Unknown condition");
					}
				}

				switch (rule.Solution.Type.Key) {
					case nameof(CutWire1):
						builder.AppendLine("<set var='result'>CutFirstWire</set>");
						break;
					case nameof(CutWire2):
						builder.AppendLine("<set var='result'>CutSecondWire</set>");
						break;
					case nameof(CutWire3):
						builder.AppendLine("<set var='result'>CutThirdWire</set>");
						break;
					case nameof(CutWire4):
						builder.AppendLine("<set var='result'>CutFourthWire</set>");
						break;
					case nameof(CutWire5):
						builder.AppendLine("<set var='result'>CutFifthWire</set>");
						break;
					case nameof(CutWireLast):
						builder.AppendLine("<set var='result'>CutLastWire</set>");
						break;
					case nameof(CutWireColourSingle):
						case nameof(CutWireColourFirst):
						builder.AppendLine($"<set var='result'><srai>XItem <srai>XIndex {rule.Solution.Colour} XS {allStars}</srai> XS {allWires} CutLastWire</srai></set>");
						break;
						case nameof(CutWireColourLast):
							builder.AppendLine($"<set var='result'><srai>XItem <srai>XLastIndex {rule.Solution.Colour} XS {allStars}</srai> XS {allWires} CutLastWire</srai></set>");
							break;
						default:
							throw new InvalidOperationException("Unknown solution");
					}

				while (closeTags.Count > 0)
					builder.AppendLine(closeTags.Pop());
				builder.AppendLine("</condition>");
				}

				builder.AppendLine("</think>\n<get var='result'/>\n</template>\n</category>");
		}
		builder.AppendLine("</aiml>");

			File.WriteAllText(Path.Combine(path, "aiml", $"wires{ruleSeed}.aiml"), builder.ToString());
	}

	public enum Result {
		CutFirstWire,
		CutSecondWire,
		CutThirdWire,
		CutFourthWire,
		CutFifthWire,
		CutLastWire
	}

	public delegate Result SolutionDelegate(RequestProcess process, Colour conditionColour, Colour[] wireColours);

	public record InstructionType(string Key, string Text, SolutionDelegate SolutionDelegate) {
		public override string ToString() => $"{{{this.Key}}}";
	}

	/// <summary>Represents a class of conditions, which can be used to construct conditions by providing a wire colour.</summary>
	public class ConditionType {
		public string Key { get; }
		public bool UsesColour { get; }
		public bool ColourAvailableForSolution { get; }
		public int WiresInvolved { get; }
		public InstructionType[] AdditionalSolutions { get; }
		public Func<Colour, Condition<Colour[]>> Delegate { get; }

		/// <summary>Initialises a colour-dependent <see cref="ConditionType"/> with the specified properties.</summary>
		/// <param name="key">The condition identifier.</param>
		/// <param name="text">The manual description of the solution. <c>{0}</c> is replaced with the colour.</param>
		/// <param name="colourAvailableForSolution">If true, the provided colour may be used in the instruction.</param>
		/// <param name="wiresInvolved">Always true.</param>
		/// <param name="func">A delegate that creates the condition from the colour.</param>
		/// <param name="additionalSolutions">A list of instructions that may be used in a rule with a condition of this type.</param>
		public ConditionType(string key, string text, bool colourAvailableForSolution, int wiresInvolved, Func<Colour, Colour[], bool> func, params InstructionType[] additionalSolutions) {
			this.Key = key;
			this.UsesColour = true;
			this.ColourAvailableForSolution = colourAvailableForSolution;
			this.WiresInvolved = wiresInvolved;
			this.AdditionalSolutions = additionalSolutions;
			this.Delegate = c => new WireCondition(this, string.Format(text, c.ToString().ToLower()), c, (p, w) => ConditionResult.FromBool(func(c, w)));
		}
		/// <summary>Initialises a <see cref="ConditionType"/> that produces the specified condition independent of colour.</summary>
		public ConditionType(Condition<Colour[]> condition) {
			this.Key = condition.Key;
			this.Delegate = c => condition;
			this.AdditionalSolutions = [];
		}

		public Condition<Colour[]> GetCondition(Colour colour) => this.Delegate(colour);

		public override string ToString() => $"{{{this.Key}}}";
	}

	public class WireCondition(ConditionType type, string text, Colour colour, Condition<Colour[]>.ConditionDelegate conditionDelegate) : Condition<Colour[]>(type.Key, $"{type.Key} {colour}", text) {
		public Colour Colour { get; } = colour;
		public ConditionDelegate Delegate { get; } = conditionDelegate;
		public ConditionType Type { get; } = type;

		public override ConditionResult Query(RequestProcess process, Colour[] data) => this.Delegate.Invoke(process, data);
	}

	public record struct Instruction(InstructionType Type, Colour? Colour) {
		public Instruction(InstructionType type) : this(type, null) { }
		public override readonly string ToString() => string.Format(this.Type.Text, this.Colour);
	}

	public record Rule(Condition<Colour[]>[] Queries, Instruction Solution) {
		public Rule(Instruction solution) : this([], solution) { }

		private bool Has(string conditionKey, Colour colour)
			=> this.Queries.Any(c => c is WireCondition c2 && c2.Key == conditionKey && c2.Colour == colour);

		public bool IsValid {
			get {
				if (this.Queries.Length == 1) return true;

				for (int i = 0; i < 4 /* sic */; ++i) {
					var colour = colours[i];
					var count = 0;
					// There may not be more than one 'colour' query for the same colour.
					foreach (var query in this.Queries) {
						if ((query.Key == nameof(ExactlyOneColourWire) || query.Key == nameof(NoColourWire) || query.Key == nameof(MoreThanOneColourWire))
							&& ((WireCondition) query).Colour == colour)
							++count;
					}
					if (count >= 2) return false;

					if (this.Has(nameof(LastWireIsColour), colour)) {
						// There may not be a 'last wire is <colour>' and 'there are no <colour> wires' clauses for the same colour.
						if (this.Has(nameof(NoColourWire), colour)) return false;
						// There may not be more than one 'last wire is <colour>' clause for different colours.
						for (var j = i + 1; j < 5; ++j) {
							if (this.Has(nameof(LastWireIsColour), colours[j]))
								return false;
						}
					}
				}

				return true;
			}
		}

		public override string ToString()
			=> (this.Queries.Length > 0 ? "if " + string.Join(" and ", (IEnumerable<Condition<Colour[]>>) this.Queries) + ", " : "") + this.Solution.ToString();
	}
}
