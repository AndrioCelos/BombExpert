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
	public class WhosOnFirstSolver : ISraixService {
		private static readonly string[] displayTexts = new[] {
			"YES", "FIRST", "DISPLAY", "OKAY", "SAYS", "NOTHING", "", "BLANK", "NO", "LED",
			"LEAD", "READ", "RED", "REED", "LEED", "HOLD ON", "YOU", "YOU ARE", "YOUR",
			"YOU'RE", "UR", "THERE", "THEY'RE", "THEIR", "THEY ARE", "SEE", "C", "CEE"
		};
        private static readonly string[][] buttonTexts = new[] {
            new[] { "READY", "FIRST", "NO", "BLANK", "NOTHING", "YES", "WHAT", "UHHH", "LEFT", "RIGHT", "MIDDLE", "OKAY", "WAIT", "PRESS" },
			new[] { "YOU", "YOU ARE", "YOUR", "YOU'RE", "UR", "U", "UH HUH", "UH UH", "WHAT?", "DONE", "NEXT", "HOLD", "SURE", "LIKE" }
        };

		public static RuleSet GetRules(int ruleSeed) {
			var random = new MonoRandom(ruleSeed);

			var displayRules = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
			var buttonRules = new Dictionary<string, string[]>(StringComparer.CurrentCultureIgnoreCase);

			foreach (var word in displayTexts)
				displayRules[word] = random.Next(6);

			for (int i = 0; i < buttonTexts.Length; ++i) {
				for (int j = 0; j < buttonTexts[i].Length; ++j) {
					var array = (string[]) buttonTexts[i].Clone();
					random.ShuffleFisherYates(array);
					buttonRules[buttonTexts[i][j]] = array;
				}
			}

			return new RuleSet(displayRules, buttonRules);
		}

		/// <param name="text">[rule seed] (Display|Button) [text]</param>
		public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
			try {
				var words = text.Split((char[]?) null, 3, StringSplitOptions.RemoveEmptyEntries);
				var rules = GetRules(int.Parse(words[0]));
				var label = words.Length > 2 ? words[2] : "nil";

				if (words[1].Equals("Display", StringComparison.CurrentCultureIgnoreCase)) {
					switch (label) {
						case "HOLD_ON": label = "HOLD ON"; break;
						case "YOU_ARE": label = "YOU ARE"; break;
						case "YOU_RE": label = "YOU'RE"; break;
						case "THEY_RE": label = "THEY'RE"; break;
						case "THEY_ARE": label = "THEY ARE"; break;
						case "nil": label = ""; break;
					}

					var result = rules.DisplayRules[label];
					switch (result) {
						case 0: return "top left";
						case 1: return "top right";
						case 2: return "middle left";
						case 3: return "middle right";
						case 4: return "bottom left";
						case 5: return "bottom right";
						default: throw new InvalidOperationException("Unknown button position");
					}
				} else {
					switch (label) {
						case "YOU_ARE": label = "YOU ARE"; break;
						case "YOU_RE": label = "YOU'RE"; break;
						case "UH_HUH": label = "UH HUH"; break;
						case "UH_UH": label = "UH UH"; break;
						case "WHATQ": label = "WHAT?"; break;
					}

					var builder = new StringBuilder();
					var result = rules.ButtonRules[label];
					foreach (var label2 in result) {
						string label3;
						switch (label2) {
							case "YOU ARE": label3 = "YOU_ARE"; break;
							case "YOU'RE": label3 = "YOU_RE"; break;
							case "UH HUH": label3 = "UH_HUH"; break;
							case "UH UH": label3 = "UH_UH"; break;
							case "WHAT?": label3 = "WHATQ"; break;
							default: label3 = label2; break;
						}

						if (builder.Length > 0) builder.Append(" ");
						builder.Append(label3);

						if (label2 == label) break;
					}
					return builder.ToString();
				}
			} catch (KeyNotFoundException) {
				return "unknown";
			}
		}

		public class RuleSet {
			public Dictionary<string, int> DisplayRules;
			public Dictionary<string, string[]> ButtonRules;

			public RuleSet(Dictionary<string, int> displayRules, Dictionary<string, string[]> buttonRules) {
				this.DisplayRules = displayRules;
				this.ButtonRules = buttonRules;
			}
		}
	}
}
