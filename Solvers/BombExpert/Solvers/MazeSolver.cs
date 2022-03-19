#nullable enable

using Aiml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using static BombExpert.Solvers.MazeSolver.MazeFlags;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace BombExpert.Solvers; 
public class MazeSolver : IModuleSolver {
	private const int MazeCount = 9;
	private const int MazeSize = 6;

	private static readonly Cache<int, Maze[]> mazeCache = new(Utils.CacheSize, GetMazesInternal);

	public static Maze[] GetMazes(int ruleSeed) => mazeCache.Get(ruleSeed);

	private static Maze[] GetMazesInternal(int ruleSeed) {
		var random = new MonoRandom(ruleSeed);
		var mazes = new Maze[MazeCount];
		var indicatorPoss = new List<Point>();
		for (int x = 0; x < MazeSize; ++x) {
			for (int y = 0; y < MazeSize; ++y) {
				indicatorPoss.Add(new Point(x, y));
			}
		}

		for (int i = 0; i < MazeCount; ++i) {
			var maze = new MazeFlags[MazeSize, MazeSize];
			var visited = new HashSet<(int x, int y)>();

			for (var j = 0; j < MazeSize * (MazeSize - 1) * 2; ++j)
				random.NextDouble();

			var stack = new Stack<(int x, int y)>();
			var (x, y) = (random.Next(MazeSize), random.Next(MazeSize));
			while (true) {
				visited.Add((x, y));

				var point = GetNeighbour(x, y, visited, random);
				if (point.HasValue) {
					var (x2, y2) = point.Value;
					if (x2 - x > 0) {
						maze[x, y] |= MazeFlags.East;
						maze[x2, y2] |= MazeFlags.West;
					} else if (x2 - x < 0) {
						maze[x, y] |= MazeFlags.West;
						maze[x2, y2] |= MazeFlags.East;
					} else if (y2 - y > 0) {
						maze[x, y] |= MazeFlags.South;
						maze[x2, y2] |= MazeFlags.North;
					} else if (y2 - y < 0) {
						maze[x, y] |= MazeFlags.North;
						maze[x2, y2] |= MazeFlags.South;
					}
					stack.Push((x, y));
					(x, y) = (x2, y2);
				} else if (stack.Count > 0)
					(x, y) = stack.Pop();
				else
					break;
			}

			var indicators = new[] {
				Utils.RemoveRandom(indicatorPoss, random),
				Utils.RemoveRandom(indicatorPoss, random)
			};

			mazes[i] = new Maze(indicators, maze);
		}

		return mazes;
	}

	private static (int x, int y)? GetNeighbour(int x, int y, HashSet<(int x, int y)> visited, Random random) {
		var list = new List<(int x, int y)>();
		if (x > 0 && !visited.Contains((x - 1, y))) list.Add((x - 1, y));
		if (x < MazeSize - 1 && !visited.Contains((x + 1, y))) list.Add((x + 1, y));
		if (y > 0 && !visited.Contains((x, y - 1))) list.Add((x, y - 1));
		if (y < MazeSize - 1 && !visited.Contains((x, y + 1))) list.Add((x, y + 1));

		if (list.Count == 0) return null;
		return list[random.Next(list.Count)];
	}

	public static bool TryGetMazeFromMarker(Maze[] mazes, int markerX, int markerY, [MaybeNullWhen(false)] out Maze maze) {
		maze = mazes.FirstOrDefault(m => m.Indicators.Contains(new Point(markerX, markerY)));
		return maze != null;
	}

	public string Process(string text, XmlAttributeCollection attributes, RequestProcess process) {
		var inputWords = text.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
		var mazes = GetMazes(int.Parse(inputWords[0]));

		// The inputs are all one-based. Convert them to zero-based indices.
		var circleX = int.Parse(inputWords[1]) - 1;
		var circleY = int.Parse(inputWords[2]) - 1;
		var startX = int.Parse(inputWords[3]) - 1;
		var startY = int.Parse(inputWords[4]) - 1;
		var goalX = int.Parse(inputWords[5]) - 1;
		var goalY = int.Parse(inputWords[6]) - 1;

		// Find which maze this is.
		if (!TryGetMazeFromMarker(mazes, circleX, circleY, out var maze))
			return "NoMaze";

		// Solve the maze.
		var path = Search(maze, startX, startY, goalX, goalY);
		if (path == null) return "NoPath";

		return string.Join(" ", path);
	}

	private static Stack<MazeFlags>? Search(Maze maze, int x, int y, int goalX, int goalY) {
		var path = new Stack<MazeFlags>();
		var openSet = new Queue<(int x, int y, MazeFlags dir)>();
		openSet.Enqueue((x, y, 0));
		var closedSet = new Dictionary<(int x, int y), MazeFlags>();

		// Perform a breadth-first search to find a path.
		while (openSet.Count > 0) {
			var (px, py, dir) = openSet.Dequeue();

			MazeFlags reverseDir;
			switch (dir) {
				case North: reverseDir = South; break;
				case South: reverseDir = North; break;
				case West: reverseDir = East; break;
				case East: reverseDir = West; break;
				default: reverseDir = 0; break;
			}
			closedSet.Add((px, py), dir);

			if (px == goalX && py == goalY) {
				// Goal found! Now retrace the path.
				while (true) {
					dir = closedSet[(px, py)];
					if (dir == 0) return path;
					path.Push(dir);
					// The value is which direction we moved to get here, so reverse it.
					switch (dir) {
						case North: ++py; break;
						case South: --py; break;
						case West: ++px; break;
						case East: --px; break;
					}
				}
			}

			var validDirections = maze.Cells[px, py] & ~reverseDir;
			if (validDirections.HasFlag(North)) openSet.Enqueue((px, py - 1, North));
			if (validDirections.HasFlag(South)) openSet.Enqueue((px, py + 1, South));
			if (validDirections.HasFlag(West )) openSet.Enqueue((px - 1, py, West ));
			if (validDirections.HasFlag(East )) openSet.Enqueue((px + 1, py, East ));
		}

		return null;
	}

	public void GenerateAiml(string path, int ruleSeed) {
		var mazes = GetMazes(ruleSeed);

		using (var writer = new StreamWriter(Path.Combine(path, "maps", $"MazeData{ruleSeed}.txt"))) {
			for (int mazeIndex = 0; mazeIndex < MazeCount; ++mazeIndex) {
				for (int x = 0; x < MazeSize; ++x) {
					for (int y = 0; y < MazeSize; ++y) {
						// In this library, the flags indicate valid directions.
						// In the AIML map, the flags indicate directions with walls.
						// We must invert the flags here.
						var flags = mazes[mazeIndex].Cells[x, y] ^ (MazeFlags) 0xF;
						writer.Write($"{mazeIndex + 1} {x + 1} {y + 1}:");
						if (flags == 0) writer.WriteLine("nil");
						else {
							if (flags.HasFlag(North)) writer.Write(nameof(North) + " ");
							if (flags.HasFlag(West)) writer.Write(nameof(West) + " ");
							if (flags.HasFlag(East)) writer.Write(nameof(East) + " ");
							if (flags.HasFlag(South)) writer.Write(nameof(South) + " ");
							writer.Flush();
							writer.BaseStream.Seek(-1, SeekOrigin.Current);  // Overwrite the last space.
							writer.WriteLine();
						}
					}
				}
			}
		}

		using (var writer = new StreamWriter(Path.Combine(path, "maps", $"MazeMarkers{ruleSeed}.txt"))) {
			for (int mazeIndex = 0; mazeIndex < MazeCount; ++mazeIndex) {
				foreach (var point in mazes[mazeIndex].Indicators) {
					writer.WriteLine($"{point.X + 1} {point.Y + 1}:{mazeIndex + 1}");
				}
			}
		}
	}

	[Flags]
	public enum MazeFlags {
		North = 1,
		West = 2,
		East = 4,
		South = 8
	}

	public struct Point {
		public int X;
		public int Y;

		public Point(int x, int y) {
			this.X = x;
			this.Y = y;
		}

		public void Deconstruct(out int x, out int y) {
			x = this.X;
			y = this.Y;
		}

		public override string ToString() => $"({this.X}, {this.Y})";
	}

	public class Maze {
		public Point[] Indicators;
		public MazeFlags[,] Cells;

		public Maze(Point[] indicators, MazeFlags[,] cells) {
			this.Indicators = indicators;
			this.Cells = cells;
		}
	}
}
