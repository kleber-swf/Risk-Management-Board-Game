using System;
using System.IO;
using System.Linq;
using System.Text;
using SimpleJSON;

namespace RiskManagement {
	class Program {
		private static readonly string BufferFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\result.txt";
		private static readonly ProgressBar ProgressBar = new ProgressBar();
		private static readonly StringBuilder Buffer = new StringBuilder();

		private static int _totalLoops;
		private static int _noWinners;
		private static int _noLosers;

		static void Main(string[] args) {
			if (args.Length == 0 || !int.TryParse(args[0], out _totalLoops))
				_totalLoops = 0;

			File.WriteAllText(BufferFilePath, "");

			do {
				Cleanup();

				var json = JSON.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\config.json"));
				var loops = _totalLoops;
				var manual = loops == 0;
				var innerBuffer = new StringBuilder();
				var game = new Game(json, innerBuffer);

				var wins = SetupResult(game.Players.Length);
				var loses = SetupResult(game.Players.Length);

				if (!manual) Console.WriteLine("\n");
				Print("INITIAL STATE", game.ToString(true), true);

				if (manual) ManualLoop(game, innerBuffer, wins, loses);
				else AutomaticLoop(game, innerBuffer, wins, loses, loops);

				Print("TEST RESULTS", string.Format("loops: {0}\n", _totalLoops), true);
				PrintResult("WINS", game.Players.Length, wins, game.Rules.OnlyOneWinner, true);
				PrintResult("LOSES", game.Players.Length, loses, game.Rules.OnlyOneWinner, true);

				if (!manual) {
					Print(string.Format("ALL WIN: {0} ({1:P1})", _noLosers, _noLosers/(float)_totalLoops));
					Print(string.Format("ALL LOSE: {0} ({1:P1})", _noWinners, _noWinners/(float)_totalLoops));
				}

				Buffer.Append("\n");
				File.AppendAllText(BufferFilePath, Buffer.ToString());
				Beep();

				Console.CursorVisible = true;
				Console.WriteLine("\n\nPRESS <ESC> TO EXIT OR <R> TO REPEAT ...");
				ConsoleKey key;
				do {
					key = Console.ReadKey().Key;
				} while (key != ConsoleKey.Escape && key != ConsoleKey.R);
				if (key == ConsoleKey.Escape) break;
			} while (true);
		}

		private static void ManualLoop(Game game, StringBuilder innerBuffer, int[][] wins, int[][] loses) {
			while (Console.ReadKey().Key != ConsoleKey.Escape) {
				game.StartNew();
				// PLANNING
				game.PlanningState();
				Print("PLANNING", game.ToString(), true);
				Console.ReadKey();

				// GAME

				while (!game.Finished) {
					innerBuffer.Clear();
					game.TurnState();
					PrintToBuffer(innerBuffer, true);
					Console.ReadKey();
				}

				// RESULTS
				_totalLoops++;
				Print("WINNERS", game.Winners.Aggregate("", (s, i) => s + i + " ") + "\n", true);
				Print("LOSERS", game.Losers.Aggregate("", (s, i) => s + i + " ") + "\n", true);
				Console.WriteLine("\n\nPRESS <ENTER> TO CONTINUE ...");
				while (Console.ReadKey().Key != ConsoleKey.Enter) { }


				for (var i = 0; i < game.Winners.Count; i++) wins[game.Winners[i]][i]++;
				for (var i = 0; i < game.Losers.Count; i++) loses[game.Losers[i]][i]++;

				if (game.Winners.Count == 0) _noWinners++;
				if (game.Losers.Count == 0) _noLosers++;

				File.AppendAllText(BufferFilePath, Buffer.ToString());
				Console.WriteLine("\n\nPRESS <ESC> TO EXIT OR ANY OTHER KEY TO CONTINUE ...");
				Buffer.Clear();
			}
		}

		private static void AutomaticLoop(Game game, StringBuilder innerBuffer, int[][] wins, int[][] loses, int loops) {
			var r = Console.CursorTop;
			Console.CursorVisible = false;

			while (loops > 0) {
				loops--;
				game.StartNew();
				// PLANNING
				game.PlanningState();

				// GAME
				while (!game.Finished) {
					innerBuffer.Clear();
					game.TurnState();
				}

				ProgressBar.Value = (_totalLoops - loops)/(float)_totalLoops;

				for (var i = 0; i < game.Winners.Count; i++) wins[game.Winners[i]][i]++;
				for (var i = 0; i < game.Losers.Count; i++) loses[game.Losers[i]][i]++;

				if (game.Winners.Count == 0) _noWinners++;
				if (game.Losers.Count == 0) _noLosers++;

				Buffer.Clear();
			}
			Console.SetCursorPosition(1, 1);
			Console.Write("".PadRight(160));
			Console.SetCursorPosition(0, r);
		}

		private static int[][] SetupResult(int count) {
			var result = new int[count][];
			for (var i = 0; i < count; i++)
				result[i] = new int[count];
			return result;
		}

		private static void PrintToBuffer(StringBuilder buffer, bool sendToConsole) {
			Buffer.Append(buffer);
			if (sendToConsole) Console.WriteLine(buffer);
		}

		private static void Print(string message) {
			Buffer.Append(message);
			Console.WriteLine(message);
		}

		private static void Print(string title, string info, bool sendToConsole) {
			var ts = "".PadLeft(title.Length + 16, '=');
			var line = string.Format("\n{0}\n        {1}\n{2}\n\n{3}", ts, title, ts, info);
			Buffer.AppendLine(line);
			if (sendToConsole) Console.WriteLine(line);
		}

		private static void PrintResult(string title, int count, int[][] array, bool onlyOneWinner, bool sendToConsole) {
			var result = string.Format("{0}\n{1}\n", title, "".PadRight(title.Length, '-'));
			var d = (float)_totalLoops;
			for (var i = 0; i < count; i++) {
				var line = "PLAYER " + i + ":\t";
				var sum = 0;
				foreach (var value in array[i]) {
					if (!onlyOneWinner) line += string.Format("{0} ({1:P1})\t", value, value/d);
					sum += value;
				}

				result += string.Format("{0}total: {1} ({2:P1})\n", line, sum, sum/d);
			}
			result += "\n";
			Buffer.AppendLine(result);
			if (sendToConsole) Console.WriteLine(result);
		}

		private static void Cleanup() {
			Buffer.Clear();
			_noWinners = 0;
			_noLosers = 0;
			Console.Clear();
		}

		private static void Beep() { Console.Beep(2500, 800); }
	}
}