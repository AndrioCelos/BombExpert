﻿using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BombExpert.Solvers;

using static BombExpert.Solvers.MazeSolver.MazeFlags;

namespace BombExpert.Tester {
	class Program {
		static void Main(string[] args) {
			Console.OutputEncoding = Encoding.UTF8;

			var solvers = new Dictionary<string, ISraixService>();
			foreach (var type in typeof(ButtonSolver).Assembly.GetExportedTypes().Where(typeof(ISraixService).IsAssignableFrom)) {
				solvers.Add(type.Name, (ISraixService) Activator.CreateInstance(type));
			}

			var user = new User("User", new Bot());

			string evaluate(string serviceName, string text) {
				return solvers[serviceName].Process(text, null, new RequestProcess(new RequestSentence(new Request(text, user, user.Bot), text), 0, false));
			}

			while (true) {
				Console.Write("> ");
				var fields = Console.ReadLine().Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);

				try {
					var text = string.Join(" ", fields.Skip(1));
					switch (fields[0].ToLower()) {
						case "reset":
							user = new User("User", new Bot());
							break;
						case "set":
							user.Predicates[fields[1]] = fields[2];
							break;
						case "button":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var rules = ButtonSolver.GetRules(int.Parse(fields[2]));

								for (int i = 0; i < rules.InitialRules.Length; ++i)
									Console.WriteLine($"{i + 1}. {rules.InitialRules[i]}");

								Console.WriteLine();
								Console.ForegroundColor = ConsoleColor.White;
								Console.WriteLine("Releasing a Held Button");
								Console.ResetColor();

								for (int i = 0; i < rules.HeldRules.Length; ++i)
									Console.WriteLine(rules.HeldRules[i].ToString());
								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(ButtonSolver), text));
							}
							break;
						case "complicatedwires":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var rules = ComplicatedWiresSolver.GetRules(int.Parse(fields[2]));

								for (int i = 0; i < rules.Length; ++i) {
									Console.WriteLine($"{(Solvers.ComplicatedWiresSolver.Flags) i}: {rules[i]}");
								}
								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(ComplicatedWiresSolver), text));
							}
							break;
						case "keypad":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var columns = KeypadSolver.GetColumns(int.Parse(fields[2]));

								var glyphCharacters = new[] {
									'©', '★', '☆', 'ټ', 'Җ', 'Ω', 'Ѭ', 'ѽ', 'ϗ', 'ϫ',
									'Ϭ', 'Ϟ', 'Ѧ', 'æ', 'Ԇ', 'Ӭ', '҈', 'Ҋ', 'Ѯ', '¿',
									'¶', 'Ͼ', 'Ͽ', 'Ψ', 'Ѫ', 'Ҩ', '҂', 'Ϙ', 'ζ', 'ƛ',
									'Ѣ'
								};

								for (int i = 0; i < columns.Length; ++i) {
									Console.WriteLine(string.Join("", columns[i].Select(g => g.ToString().PadRight(14))));
								}
								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(KeypadSolver), text));
							}
							break;
						case "knob":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var rules = KnobSolver.GetRules(int.Parse(fields[2]));

								for (int i = 0; i < 4; ++i) {
									Console.ForegroundColor = ConsoleColor.White;
									Console.WriteLine($"{(Solvers.KnobSolver.Position) i} Position:");
									Console.ResetColor();

									var rule0 = rules[i * 2];
									var rule1 = rules[i * 2 + 1];

									for (int j = 0; j < 6; ++j) {
										Console.Write(rule0.Lights[j] ? 'X' : '.');
										Console.Write(' ');
									}
									Console.Write("    ");
									for (int j = 0; j < 6; ++j) {
										Console.Write(rule1.Lights[j] ? 'X' : '.');
										Console.Write(' ');
									}
									Console.WriteLine();
									for (int j = 6; j < 12; ++j) {
										Console.Write(rule0.Lights[j] ? 'X' : '.');
										Console.Write(' ');
									}
									Console.Write("    ");
									for (int j = 6; j < 12; ++j) {
										Console.Write(rule1.Lights[j] ? 'X' : '.');
										Console.Write(' ');
									}
									Console.WriteLine();
									Console.WriteLine();
								}
							} else {
								Console.WriteLine(evaluate(nameof(KnobSolver), text));
							}
							break;
						case "maze":
							char boxDrawingChar(Solvers.MazeSolver.MazeFlags flags) {
								switch (flags) {
									case 0: return ' ';
									case North: return '╵';
									case West: return '╴';
									case West | North: return '┘';
									case East: return '╶';
									case East | North: return '└';
									case East | West: return '─';
									case East | West | North: return '┴';
									case South: return '╷';
									case South | North: return '│';
									case South | West: return '┐';
									case South | West | North: return '┤';
									case South | East: return '┌';
									case South | East | North: return '├';
									case South | East | West: return '┬';
									case South | East | West | North: return '┼';
									default: return '?';
								}
							}

							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var mazes = MazeSolver.GetMazes(int.Parse(fields[2]));

								for (int i = 0; i < mazes.Length; ++i) {
									var maze = mazes[i];
									var maxx = maze.Cells.GetUpperBound(0);
									var maxy = maze.Cells.GetUpperBound(1);

									Console.ForegroundColor = ConsoleColor.White;
									Console.WriteLine($"Maze {i + 1}");
									Console.ResetColor();

									for (int y = 0; ; ++y) {
										for (int x = 0; ; ++x) {
											// Corner tile
											Solvers.MazeSolver.MazeFlags flags = 0;
											if (y > 0 && (x <= 0 || !maze.Cells[x - 1, y - 1].HasFlag(East))) flags |= North;
											if (x > 0 && (y <= 0 || !maze.Cells[x - 1, y - 1].HasFlag(South))) flags |= West;
											if (x <= maxx && (y <= 0 || !maze.Cells[x, y - 1].HasFlag(South))) flags |= East;
											if (y <= maxy && (x <= 0 || !maze.Cells[x - 1, y].HasFlag(East))) flags |= South;
											Console.Write(boxDrawingChar(flags));

											if (x > maxx) break;
											// Horizontal border tile
											flags = 0;
											if (y <= 0 || !maze.Cells[x, y - 1].HasFlag(South)) flags |= West | East;
											Console.Write(boxDrawingChar(flags));
										}
										Console.WriteLine();

										if (y > maxy) break;
										for (int x = 0; ; ++x) {
											// Vertical border tile
											Solvers.MazeSolver.MazeFlags flags = 0;
											if (x <= 0 || !maze.Cells[x - 1, y].HasFlag(East)) flags |= North | South;
											Console.Write(boxDrawingChar(flags));

											if (x > maxx) break;
											// Space tile
											if (maze.Indicators.Contains(new Solvers.MazeSolver.Point(x, y))) {
												Console.ForegroundColor = ConsoleColor.Green;
												Console.Write($"O");
												Console.ResetColor();
											} else
												Console.Write('·');
										}
										Console.WriteLine();
									}

									Console.WriteLine();
								}
							} else {
								Console.WriteLine(evaluate(nameof(MazeSolver), text));
							}
							break;
						case "memory":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var rules = MemorySolver.GetRules(int.Parse(fields[2]));

								for (int i = 0; i <= rules.GetUpperBound(0); ++i) {
									Console.ForegroundColor = ConsoleColor.White;
									Console.WriteLine($"Stage {i + 1}:");
									Console.ResetColor();

									for (int j = 0; j < 4; ++j) {
										Console.WriteLine($"If the display is {j + 1}, {rules[i, j]}.");
									}
									Console.WriteLine();
								}
							} else {
								Console.WriteLine(evaluate(nameof(MemorySolver), text));
							}
							break;
						case "morsecode":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var entries = MorseCodeSolver.GetRules(int.Parse(fields[2])).OrderBy(e => e.Value);

								Console.ForegroundColor = ConsoleColor.White;
								Console.WriteLine($"If the word is:  Respond at frequency:");
								Console.ResetColor();
								foreach (var entry in entries) {
									Console.WriteLine($"{entry.Key,-15}  {entry.Value} MHz");
								}
								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(MorseCodeSolver), text));
							}
							break;
						case "password":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var words = PasswordSolver.GetWords(int.Parse(fields[2]));

								for (int i = 0; i < words.Length; ++i) {
									Console.Write(words[i]);
									if (i % 5 == 4) Console.WriteLine();
									else Console.Write("  ");
								}
								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(PasswordSolver), text));
							}
							break;
						case "simonsays":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var colours = SimonSaysSolver.GetRules(int.Parse(fields[2]));

								for (int i = 1; i >= 0; --i) {
									Console.ForegroundColor = ConsoleColor.White;
									Console.WriteLine($"If the serial number {(i > 0 ? "contains" : "does not contain")} a vowel:");
									Console.ResetColor();
									Console.WriteLine($"            Red flash     Blue flash    Green flash   Yellow flash");
									for (int j = 0; j < 3; ++j) {
										Console.Write((j == 0 ? "No strikes" : j == 1 ? "1 strike" : "2+ strikes").PadRight(12));
										for (int k = 0; k < 4; ++k) {
											Console.Write(colours[i, j, k].ToString().PadRight(14));
										}
										Console.WriteLine();
									}
									Console.WriteLine();
								}
							} else {
								Console.WriteLine(evaluate(nameof(SimonSaysSolver), text));
							}
							break;
						case "whosonfirst":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var ruleSet = WhosOnFirstSolver.GetRules(int.Parse(fields[2]));

								Console.ForegroundColor = ConsoleColor.White;
								Console.WriteLine($"Step 1:");
								Console.ResetColor();

								int count = 0;
								StringBuilder builder1 = new StringBuilder(), builder2 = new StringBuilder(), builder3 = new StringBuilder(), builder4 = new StringBuilder();
								foreach (var entry in ruleSet.DisplayRules) {
									builder1.Append(entry.Key.PadRight(10));
									builder2.Append($" {(entry.Value == 0 ? 'O' : '.')}   {(entry.Value == 1 ? 'O' : '.')}    ");
									builder3.Append($" {(entry.Value == 2 ? 'O' : '.')}   {(entry.Value == 3 ? 'O' : '.')}    ");
									builder4.Append($" {(entry.Value == 4 ? 'O' : '.')}   {(entry.Value == 5 ? 'O' : '.')}    ");

									++count;
									if (count % 6 == 0) {
										Console.WriteLine(builder1.ToString());
										Console.WriteLine(builder2.ToString());
										Console.WriteLine(builder3.ToString());
										Console.WriteLine(builder4.ToString());
										Console.WriteLine();
										builder1.Clear();
										builder2.Clear();
										builder3.Clear();
										builder4.Clear();
									}
								}
								if (count % 6 > 0) {
									Console.WriteLine(builder1.ToString());
									Console.WriteLine(builder2.ToString());
									Console.WriteLine(builder3.ToString());
									Console.WriteLine(builder4.ToString());
									Console.WriteLine();
								}

								Console.ForegroundColor = ConsoleColor.White;
								Console.WriteLine($"Step 2:");
								Console.ResetColor();

								foreach (var entry in ruleSet.ButtonRules)
									Console.WriteLine($"{$"\"{entry.Key}\":",-12}{string.Join(", ", entry.Value)}");

								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(WhosOnFirstSolver), text));
							}
							break;
						case "wires":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								var rules = WiresSolver.GetRules(int.Parse(fields[2]));

								for (int i = 0; i < rules.Length; ++i) {
									Console.ForegroundColor = ConsoleColor.White;
									Console.WriteLine($"{i + 3} wires");
									Console.ResetColor();

									var otherwise = false;
									foreach (var rule in rules[i]) {
										if (otherwise) Console.Write("Otherwise, ");
										otherwise = true;
										Console.WriteLine(rule.ToString());
									}

									Console.WriteLine();
								}
							} else {
								Console.WriteLine(evaluate(nameof(WiresSolver), text));
							}
							break;
						case "wiresequences":
							if (fields[1].Equals("rules", StringComparison.CurrentCultureIgnoreCase)) {
								string describeInstruction(Solvers.WireSequencesSolver.Instruction instruction) {
									switch ((int) instruction) {
										case 0: return "never";
										case 1: return "A";
										case 2: return "B";
										case 3: return "A or B";
										case 4: return "C";
										case 5: return "A or C";
										case 6: return "B or C";
										case 7: return "A, B or C";
										default: return instruction.ToString();
									}
								}

								var rules = WireSequencesSolver.GetRules(int.Parse(fields[2]));

								Console.ForegroundColor = ConsoleColor.White;
								Console.WriteLine("         Red        Blue       Black");
								Console.ResetColor();
								for (int i = 0; i < 9; ++i) {
									string ordinal;
									switch (i) {
										case 0: ordinal = "first"; break;
										case 1: ordinal = "second"; break;
										case 2: ordinal = "third"; break;
										case 3: ordinal = "fourth"; break;
										case 4: ordinal = "fifth"; break;
										case 5: ordinal = "sixth"; break;
										case 6: ordinal = "seventh"; break;
										case 7: ordinal = "eighth"; break;
										default: ordinal = "ninth"; break;
									}
									Console.WriteLine($"{ordinal,-7}  {describeInstruction(rules.RedRules[i]),-9}  {describeInstruction(rules.BlueRules[i]),-9}  {describeInstruction(rules.BlackRules[i])}");
								}
								Console.WriteLine();
							} else {
								Console.WriteLine(evaluate(nameof(WireSequencesSolver), text));
							}
							break;
						case "q":
							return;
					}
				} catch (Exception ex) {
					Console.Error.WriteLine(ex);
				}
			}
		}
	}
}
