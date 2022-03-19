using Aiml;
using BombExpert.Conditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BombExpert.Solvers; 
public class ButtonSolver : IModuleSolver {
	private const int MaxInitialRules = 6;
	private const int MaxHeldRules = 3;

		public class ButtonColourCondition : Condition<ButtonData> {
			public Colour Colour { get; }
			public ButtonColourCondition(Colour colour) : base(nameof(ButtonColourCondition), $"the button is {colour.ToString().ToLower()}") {
				this.Colour = colour;
			}
			public override ConditionResult Query(RequestProcess process, ButtonData data) => ConditionResult.FromBool(data.Colour == this.Colour);
		}

		public class ButtonLabelCondition : Condition<ButtonData> {
			public Label Label { get; }
			public ButtonLabelCondition(Label label) : base(nameof(ButtonLabelCondition), $"the button says '{label}'") {
				this.Label = label;
			}
			public override ConditionResult Query(RequestProcess process, ButtonData data) => ConditionResult.FromBool(data.Label == this.Label);
		}

	public static RuleSet GetRules(int ruleSeed) {
		if (ruleSeed == 1) return new RuleSet(
			new[] {
				new InitialRule(new List<Condition<ButtonData>>() { new ButtonColourCondition(Colour.Blue), new ButtonLabelCondition(Label.Abort) }, InitialSolution.Hold),
				new InitialRule(new List<Condition<ButtonData>>() { new BatteriesCondition<ButtonData>(Operations.MoreThan, 1), new ButtonLabelCondition(Label.Detonate) }, InitialSolution.Tap),
				new InitialRule(new List<Condition<ButtonData>>() { new ButtonColourCondition(Colour.White), IndicatorCondition<ButtonData>.Lit("CAR") }, InitialSolution.Hold),
				new InitialRule(new List<Condition<ButtonData>>() { new BatteriesCondition<ButtonData>(Operations.MoreThan, 2), IndicatorCondition<ButtonData>.Lit("FRK") }, InitialSolution.Tap),
				new InitialRule(new List<Condition<ButtonData>>() { new ButtonColourCondition(Colour.Yellow) }, InitialSolution.Hold),
				new InitialRule(new List<Condition<ButtonData>>() { new ButtonColourCondition(Colour.Red), new ButtonLabelCondition(Label.Hold) }, InitialSolution.Tap),
				new InitialRule(InitialSolution.Hold)
			},
			new[] {
				new HeldRule(Colour.Blue, new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, 4)),
				new HeldRule(Colour.White, new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, 1)),
				new HeldRule(Colour.Yellow, new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, 5)),
				new HeldRule(null, new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, 1)),
			}
		);

		var random = new MonoRandom(ruleSeed);
		var conditionWeights = new WeightMap<Condition<ButtonData>, string>(c => c.Key);
		var initialInstructionWeights = new WeightMap<InitialSolution, InitialSolution>(s => s);
		initialInstructionWeights.SetWeight(InitialSolution.Tap, 0.1f);
		initialInstructionWeights.SetWeight(InitialSolution.TapWhenSecondsMatch, 0.05f);
		var heldInstructionWeights = new WeightMap<HeldSolution, string>(s => s.Key);

		// Build condition lists.
		var PrimaryConditions = new List<Condition<ButtonData>>() {
				new ButtonColourCondition(Colour.Red), new ButtonColourCondition(Colour.Blue), new ButtonColourCondition(Colour.Yellow), new ButtonColourCondition(Colour.White),
				new BatteriesCondition<ButtonData>(Operations.MoreThan, 1), new BatteriesCondition<ButtonData>(Operations.MoreThan, 2)
			};
		var IndicatorColorConditions = new List<Colour?>() { Colour.Red, Colour.Blue, Colour.Yellow, Colour.White };
		var SecondaryConditions = new List<Condition<ButtonData>>() { new ButtonLabelCondition(Label.Press), new ButtonLabelCondition(Label.Hold), new ButtonLabelCondition(Label.Abort), new ButtonLabelCondition(Label.Detonate) };

		for (int i = 0; i < 3; ++i)
			SecondaryConditions.Add(IndicatorCondition<ButtonData>.Lit(random.Pick(Utils.IndicatorLabels)));

		var ports = new List<PortType>() { PortType.DviD, PortType.PS2, PortType.RJ45, PortType.StereoRCA, PortType.Parallel, PortType.Serial };
		for (int i = 0; i < 3; ++i) {
			var port = Utils.RemoveRandom(ports, random);
			SecondaryConditions.Add(new PortCondition<ButtonData>(port));
		}
		foreach (var port in ports)
			PrimaryConditions.Add(new PortCondition<ButtonData>(port));
		PrimaryConditions.Add(new EmptyPortPlateCondition<ButtonData>());
		SecondaryConditions.Add(Condition<ButtonData>.SerialNumberStartsWithLetter());
		SecondaryConditions.Add(Condition<ButtonData>.SerialNumberIsOdd());

		// Generate initial rules.
		var initialRules = new List<InitialRule>();
		while (initialRules.Count < MaxInitialRules && PrimaryConditions.Count > 0) {
			var query = Utils.RemoveRandom(PrimaryConditions, random);
			if (random.Next(2) == 0)
				initialRules.Add(CreateInitialRule(new List<Condition<ButtonData>>() { query }, initialInstructionWeights, random));
			else
				initialRules.Add(CreateInitialRule(new List<Condition<ButtonData>>() { query, Utils.RemoveRandom(SecondaryConditions, random) }, initialInstructionWeights, random));
		}
		initialRules.Add(new InitialRule(InitialSolution.Hold));

		// Generate held rules.
		var heldRules = new List<HeldRule>();
		while (heldRules.Count < MaxHeldRules && IndicatorColorConditions.Count > 0) {
			var condition = Utils.RemoveRandom(IndicatorColorConditions, random);
			heldRules.Add(CreateHeldRule(condition, heldInstructionWeights, random));
		}
		heldRules.Add(CreateHeldRule(null, heldInstructionWeights, random));

		RemoveRedundantRules(initialRules, initialInstructionWeights, random, PrimaryConditions, SecondaryConditions);

		return new RuleSet(initialRules.ToArray(), heldRules.ToArray());
	}

	private static void RemoveRedundantRules(List<InitialRule> rules, WeightMap<InitialSolution, InitialSolution> weights, Random random, List<Condition<ButtonData>> PrimaryQueryList, List<Condition<ButtonData>> SecondaryQueryList) {
		// Since the last rule always has the instruction 'hold the button',
		// force the second-last one to have a different instruction.
		if (rules[^2].Solution == InitialSolution.Hold)
			rules[^2] = new InitialRule(rules[^2].Conditions, weights.Roll(GetInitialSolutions(false), random));

		var twobattery = -1;
		var onebattery = -1;
		InitialSolution? solution = null;
		var samesolution = true;

		for (var i = rules.Count - 1; i >= 0; i--) {
			var condition = rules[i].Conditions.OfType<BatteriesCondition<ButtonData>>().FirstOrDefault();
			if (condition != null) {
				switch (condition.Number) {
					case 1:
						onebattery = i;
						if (twobattery > -1) {
							samesolution &= rules[i].Solution == solution;
							break;
						}
						solution = rules[i].Solution;
						break;
					case 2:
						twobattery = i;
						if (onebattery > -1) {
							samesolution &= rules[i].Solution == solution;
							break;
						}
						solution = rules[i].Solution;
						break;
				}
			}
			if (solution.HasValue)
				samesolution &= rules[i].Solution == solution;
		}
		if (onebattery < 0 || twobattery < 0) {
			return;
		}

		if (onebattery < twobattery && rules[onebattery].Conditions.Count == 1) {
			// We have a 'if there is more than 1 battery' rule above a 'if there are more than 2 batteries' rule.
			if (random.Next(2) == 0) {
				rules[onebattery].Conditions.Add(Utils.RemoveRandom(SecondaryQueryList, random));
			} else {
				rules[onebattery].Conditions[0] = Utils.RemoveRandom(PrimaryQueryList, random);
			}
		} else if (rules[onebattery].Conditions.Count == 1 && rules[twobattery].Conditions.Count == 1 && samesolution) {
			// We have a 'if there is more than 1 battery' rule below a 'if there are more than 2 batteries' rule,
			// and every rule in between has the same instruction.
			switch (random.Next(7)) {
				// Add a secondary query to one or both of the battery rules.
				case 0:
					rules[onebattery].Conditions.Add(Utils.RemoveRandom(SecondaryQueryList, random));
					break;
				case 1:
					rules[twobattery].Conditions.Add(Utils.RemoveRandom(SecondaryQueryList, random));
					break;
				case 2:
					rules[twobattery].Conditions.Add(Utils.RemoveRandom(SecondaryQueryList, random));
					goto case 0;

				// Replace one or both of the battery rules with a new primary query.
				case 3:
					rules[onebattery].Conditions[0] = Utils.RemoveRandom(PrimaryQueryList, random);
					break;
				case 4:
					rules[twobattery].Conditions[0] = Utils.RemoveRandom(PrimaryQueryList, random);
					break;
				case 5:
					rules[twobattery].Conditions[0] = Utils.RemoveRandom(PrimaryQueryList, random);
					goto case 3;

				// Replace one of the solutions in between the minimum and maximum battery.
				default:
					var replacementsolution = rules[onebattery].Solution == InitialSolution.Tap ? InitialSolution.Hold : InitialSolution.Tap;
					if (Math.Abs(onebattery - twobattery) == 1)
						rules[Math.Min(onebattery, twobattery)].Solution = replacementsolution;
					else
						rules[random.Next(Math.Min(onebattery, twobattery), Math.Max(onebattery, twobattery))].Solution = replacementsolution;
					break;
			}
		}
	}

	private static InitialRule CreateInitialRule(List<Condition<ButtonData>> conditions, WeightMap<InitialSolution, InitialSolution> weights, Random random)
		=> new(conditions, weights.Roll(GetInitialSolutions(true), random));

	private static HeldRule CreateHeldRule(Colour? colour, WeightMap<HeldSolution, string> weights, Random random)
		=> new(colour, weights.Roll(GetHeldSolutions(), random));

	private static List<InitialSolution> GetInitialSolutions(bool holdAllowed) {
		var solutions = new List<InitialSolution>();
		if (holdAllowed) solutions.Add(InitialSolution.Hold);
		solutions.Add(InitialSolution.Tap);
		solutions.Add(InitialSolution.TapWhenSecondsMatch);
		return solutions;
	}
	private static List<HeldSolution> GetHeldSolutions() {
		var list = new List<HeldSolution>() {
			new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, 5)
		};
		for (int i = 1; i <= 4; ++i)
			list.Add(new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, i));
		for (int i = 6; i <= 9; ++i)
			list.Add(new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, i));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseOnTimerDigit, 0));
		for (int i = 0; i <= 9; ++i)
			list.Add(new HeldSolution(HeldSolutionType.ReleaseOnSecondsOnesDigit, i));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseAnyTime));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseOnSecondsMultipleOfFour));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseOnSecondsDigitSum, 5));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseOnSecondsDigitSum, 7));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseOnSecondsDigitSum, 3));
		list.Add(new HeldSolution(HeldSolutionType.ReleaseOnSecondsPrimeOrZero));

		return list;
	}

	/// <param name="text">Usage: [rule seed] [colour] [label]?</param>
	public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
		var fields = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
		var colour = (Colour) Enum.Parse(typeof(Colour), fields[1], true);
		var rules = GetRules(int.Parse(fields[0]));

		if (fields.Length > 2) {
			// Initial stage
			var label = (Label) Enum.Parse(typeof(Label), fields[2], true);
			InitialSolution? instruction = null;

			foreach (var rule in rules.InitialRules) {
				var result = ConditionResult.FromBool(true);
				foreach (var condition in rule.Conditions)
					result = result && condition.Query(process, new ButtonData(colour, label));

				if (result.Code == ConditionResultCode.True) {
					instruction = rule.Solution;
					break;
				} else if (result.Code == ConditionResultCode.Unknown) return result.Details!;
			}
			if (instruction == null) throw new InvalidOperationException("No rules matched?!");

			return instruction.Value.ToString();
		} else {
			// Held stage
			var rule = rules.HeldRules.FirstOrDefault(r => r.Colour == colour) ?? rules.HeldRules.Single(r => r.Colour == null);
			return $"{rule.Solution.Type} {rule.Solution.Digit}";
		}
	}

	public void GenerateAiml(string path, int ruleSeed) {
		var rules = GetRules(ruleSeed);
		using var writer = new StreamWriter(Path.Combine(path, "aiml", $"button{ruleSeed}.aiml"));
		writer.Write("<?xml version='1.0' encoding='UTF-8'?>\n" +
			"<aiml version='2.0'>\n" +
			"<category>\n" + 
			$"<pattern>SolverFallback Button {ruleSeed} <set>BombColours</set> <set>ButtonLabels</set></pattern>\n" + 
			"<template>\n" + 
			"<think>\n" + 
			"<set var='colour'><star/></set>\n" + 
			"<set var='label'><star index='2'/></set>\n");

		for (int i = 0; i < rules.InitialRules.Length; ++i) {
			var rule = rules.InitialRules[i];

			writer.WriteLine();
			writer.WriteLine($"<!-- {i + 1}. {rule} -->\n<condition var='result' value='unknown'>");

			// Check button conditions before edgework ones.
			static bool isButtonCondition(Condition<ButtonData> condition) => condition is ButtonColourCondition || condition is ButtonLabelCondition;
			var conditions = rule.Conditions;
			if (conditions.Count == 2 && isButtonCondition(conditions[1]) && !isButtonCondition(conditions[0]))
				conditions = new List<Condition<ButtonData>>() { conditions[1], conditions[0] };

			var closeTags = new Stack<string>();
			foreach (var condition in conditions) {
				switch (condition) {
					case ButtonColourCondition buttonColourCondition:
						writer.WriteLine($"<condition var='colour' value='{buttonColourCondition.Colour}'>");
						closeTags.Push("</condition>");
						break;
					case ButtonLabelCondition buttonLabelCondition:
						writer.WriteLine($"<condition var='label' value='{buttonLabelCondition.Label}'>");
						closeTags.Push("</condition>");
						break;
					case BatteriesCondition<ButtonData> batteriesCondition when batteriesCondition.Operation == Operations.MoreThan:
						writer.WriteLine("<condition name='BombBatteryCount'>\n<li value='unknown'><set var='result'>NeedEdgework BatteryCount</set></li>");
						for (int j = 0; j <= batteriesCondition.Number; ++j)
							writer.WriteLine($"<li value='{j}'></li>");
						writer.WriteLine("<li>");
						closeTags.Push("</li>\n</condition>");
						break;
					case IndicatorCondition<ButtonData> indicatorCondition:
						writer.WriteLine($"<condition name='{indicatorCondition.Predicate}'>\n" +
							$"<li value='unknown'><set var='result'>NeedEdgework {indicatorCondition.EdgeworkQuery}</set></li>\n" +
							"<li value='true'>");
						closeTags.Push("</li>\n</condition>");
						break;
					case PortCondition<ButtonData> portCondition:
						writer.WriteLine($"<condition name='BombPort{portCondition.PortType}'>\n<li value='true'>");
						closeTags.Push($"</li>\n<li value='unknown'><set var='result'>NeedEdgework Port {portCondition.PortType}</set></li>\n</condition>");
						break;
					case EmptyPortPlateCondition<ButtonData> _:
						writer.WriteLine("<condition name='BombEmptyPortPlate'>\n<li value='true'>");
						closeTags.Push("</li>\n<li value='unknown'><set var='result'>NeedEdgework EmptyPortPlate</set></li>\n</condition>");
						break;
					case SerialNumberStartsWithLetterCondition<ButtonData> _:
						writer.WriteLine("<condition name='BombSerialNumberStartsWithLetter'>\n<li value='true'>");
						closeTags.Push("</li>\n<li value='unknown'><set var='result'>NeedEdgework SerialNumberStartsWithLetter</set></li>\n</condition>");
						break;
					case SerialNumberParityCondition<ButtonData> serialNumberParityCondition:
						writer.WriteLine($"<condition name='BombSerialNumberIsOdd'>\n<li value='{serialNumberParityCondition.Odd}'>");
						closeTags.Push("</li>\n<li value='unknown'><set var='result'>NeedEdgework SerialNumberIsOdd</set></li>\n</condition>");
						break;
					default:
						throw new InvalidOperationException("Unknown condition");
				}
			}

			writer.WriteLine($"<set var='result'>{rule.Solution}</set>");
			while (closeTags.Count > 0)
				writer.WriteLine(closeTags.Pop());
			writer.WriteLine("</condition>");
		}
		writer.WriteLine("</think>");
		writer.WriteLine("<get var='result'/>");
		writer.WriteLine("</template>");
		writer.WriteLine("</category>");

		writer.WriteLine("<category>");
		writer.WriteLine($"<pattern>SolverFallback Button {ruleSeed} <set>BombColours</set></pattern>");
		writer.WriteLine("<template>");
		writer.WriteLine("<think><set var='colour'><star/></set></think>");
		writer.WriteLine("<condition var='colour'>");
		
		foreach (var rule in rules.HeldRules) {
			writer.Write(rule.Colour.HasValue ? $"<li value='{rule.Colour}'>" : "<li>");
			writer.WriteLine($"{rule.Solution.Type} {rule.Solution.Digit}</li>");
		}

		writer.WriteLine("</condition>");
		writer.WriteLine("</template>");
		writer.WriteLine("</category>");
		writer.WriteLine("</aiml>");
	}

	public enum Colour {
		Red,
		Yellow,
		Blue,
		White
	}

	public enum Label {
		Press,
		Abort,
		Detonate,
		Hold
	}

	public enum InitialSolution {
		Hold,
		Tap,
		TapWhenSecondsMatch
	}

	public enum HeldSolutionType {
		ReleaseOnTimerDigit,
		ReleaseOnSecondsDigitSum,
		ReleaseOnSecondsPrimeOrZero,
		ReleaseOnSecondsMultipleOfFour,
		ReleaseOnSecondsOnesDigit,
		ReleaseAnyTime
	}

	public class HeldSolution {
		public string Key { get; }
		public HeldSolutionType Type { get; }
		public int Digit { get; }

		public HeldSolution(HeldSolutionType type) : this(type, 0) { }
		public HeldSolution(HeldSolutionType type, int digit) {
			this.Key = type.ToString() + digit;
			this.Type = type;
			this.Digit = digit;
		}

		public override string ToString() => this.Type switch {
			HeldSolutionType.ReleaseOnTimerDigit => $"release when the countdown timer has a {this.Digit} in any position",
			HeldSolutionType.ReleaseOnSecondsDigitSum => $"release when the two seconds digits add up to {this.Digit}{(this.Digit <= 14 ? " or 1" + this.Digit : "")}",
			HeldSolutionType.ReleaseOnSecondsPrimeOrZero => $"release when the number of seconds displayed is either prime or 0",
			HeldSolutionType.ReleaseOnSecondsMultipleOfFour => $"release when the two seconds digits add up to a multiple of 4",
			HeldSolutionType.ReleaseOnSecondsOnesDigit => $"release when right most seconds digit is {this.Digit}",
			HeldSolutionType.ReleaseAnyTime => $"release at any time",
			_ => throw new InvalidOperationException("Unknown solution type."),
		};
	}

	public class InitialRule {
		public List<Condition<ButtonData>> Conditions { get; }
		public InitialSolution Solution { get; internal set; }

		public InitialRule(InitialSolution solution) {
			this.Conditions = new List<Condition<ButtonData>>();
			this.Solution = solution;
		}
		public InitialRule(List<Condition<ButtonData>> conditions, InitialSolution solution) {
			this.Conditions = conditions;
			this.Solution = solution;
		}

		public override string ToString()
			=> "If " + (this.Conditions.Count == 0 ? "none of the above apply" : string.Join(" and ", this.Conditions)) + ", " +
				this.Solution switch {
					InitialSolution.Hold => "hold the button and refer to “Releasing a Held Button”",
					InitialSolution.Tap => "press and immediately release the button",
					InitialSolution.TapWhenSecondsMatch => "press and immediately release when the two seconds digits on the timer match",
					_ => ""
				} + ".";
	}

	public class HeldRule {
		public Colour? Colour { get; }
		public HeldSolution Solution { get; }

		public HeldRule(Colour? colour, HeldSolution solution) {
			this.Colour = colour;
			this.Solution = solution;
		}

		public override string ToString()
			=> (this.Colour != null ? this.Colour.ToString() + " strip: " : "Any other color strip: ") +
			this.Solution;
	}

	public class RuleSet {
		public InitialRule[] InitialRules;
		public HeldRule[] HeldRules;

		public RuleSet(InitialRule[] initialRules, HeldRule[] heldRules) {
			this.InitialRules = initialRules;
			this.HeldRules = heldRules;
		}
	}

	public struct ButtonData {
		public Colour Colour;
		public Label Label;

		public ButtonData(Colour colour, Label label) {
			this.Colour = colour;
			this.Label = label;
		}
	}
}
