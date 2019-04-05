#nullable enable

using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BombExpert;

using static BombExpert.Colour;

namespace BombExpert.Solvers {
	public class SimonSaysSolver : ISraixService {
		public static Colour[,,] GetRules(int ruleSeed) {
			if (ruleSeed == 1) return new[,,] {
				// If the serial number does not contain a vowel
				{
					{ Blue, Yellow, Green, Red },
					{ Red, Blue, Yellow, Green },
					{ Yellow, Green, Blue, Red }
				},
				// If the serial number contains a vowel
				{
					{ Blue, Red, Yellow, Green },
					{ Yellow, Green, Blue, Red },
					{ Green, Red, Yellow, Blue }
				}
			};

			var random = new MonoRandom(ruleSeed);
			var colours = new Colour[2, 3, 4];
			for (int i = 1; i >= 0; --i) {
				for (int j = 0; j < 3; ++j) {
					for (int k = 0; k < 4; ++k)
						colours[i, j, k] = (Colour) random.Next(4);
					// Make sure they're not all the same.
					while (colours[i, j, 1] == colours[i, j, 0] &&
						colours[i, j, 2] == colours[i, j, 0] &&
						colours[i, j, 3] == colours[i, j, 0])
						colours[i, j, random.Next(4)] = (Colour) random.Next(4);
				}
			}

			return colours;
		}

		/// <param name="text">[rule seed] [vowel] [strikes] [colours ...]</param>
		public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
			var words = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
			var ruleSeed = int.Parse(words[0]);
			var vowel = bool.Parse(words[1]);
			var strikes = int.Parse(words[2]);
			var colours = from s in words.Skip(3) select (Colour) Enum.Parse(typeof(Colour), s, true);
			return string.Join(" ", Solve(ruleSeed, vowel, strikes, colours.ToList()));
		}

		public static Colour[] Solve(int ruleSeed, bool vowel, int strikes, IList<Colour> colours) {
			if (strikes > 2) strikes = 2;
			var rules = GetRules(ruleSeed);

			var result = new Colour[colours.Count];
			for (int i = 0; i < colours.Count; ++i) {
				result[i] = rules[vowel ? 1 : 0, strikes, (int) colours[i]];
			}
			return result;
		}
	}
}
