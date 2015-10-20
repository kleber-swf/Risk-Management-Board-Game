using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJSON;

namespace RiskManagement {
	public class Game {
		public readonly Rules Rules;
		public readonly Player[] Players;
		private readonly PlayerState[] _playerStates;
		private readonly Dice _dice;
		private List<Card> _deck;

		public readonly List<int> Winners = new List<int>();
		public readonly List<int> Losers = new List<int>();
		private int _currentPlayer = int.MaxValue - 2;
		private int _currentTurn;
//		private readonly List<int> _forcedRiskSprints = new List<int>();
		private List<float> _chances;
		private readonly Random _random;

		private readonly StringBuilder _buffer;

		public Game(JSONNode json, StringBuilder buffer) {
			_buffer = buffer;
			_random = new Random((int)DateTime.Now.Ticks);
			Rules = Rules.Deserialize(json["rules"]);
			_dice = new Dice();

			var playersJson = json["players"];
			var playerCount = playersJson.Count;
			Players = new Player[playerCount];
			for (var i = 0; i < playerCount; i++)
				Players[i] = Player.Deserialize(playersJson[i]);

			_playerStates = new PlayerState[playerCount];
			for (var i = 0; i < playerCount; i++)
				_playerStates[i] = new PlayerState();
		}

		private bool _finished;
		private int _currentChanceIndex;

		public bool Finished {
			get {
				if (_finished) return true;
				foreach (var state in _playerStates)
					if (state.State == State.Playing) return false;
				return true;
			}
		}

		public void ShuffleCards() {
			_deck = new List<Card>(Rules.Cards);
			_deck.Shuffle();
		}

		public void StartNew() {
			ShuffleCards();
			Winners.Clear();
			Losers.Clear();
			_currentPlayer = Players.Length;
			_currentTurn = 0;
			_finished = false;

			_currentChanceIndex = -1;
			_chances = new List<float>(Rules.RiskChances);
			_chances.Shuffle();
		}

		public void PlanningState() {
			for (var i = 0; i < Players.Length; i++) {
				var player = Players[i];
				var state = _playerStates[i];

				state.Sprint = 0;
				state.Resources = Rules.InitialResources;
				state.State = State.Playing;

				var planningCardsCount = player.PlanningCardsCount;
				var over = 0;

				if (planningCardsCount == 0) state.Sprint = 1;
				else {
					over = planningCardsCount - Rules.NormalPlanningCount;
					if (player.UseOneMorePlanningSprint) {
						state.Sprint = -1;
						state.Resources -= over*Rules.NormalPlanningCost;
					} else if (over > 0)
						state.Resources -= over*Rules.OverPlanningCost;
					else over = 0;
				}
				state.Resources -= (planningCardsCount - over)*Rules.NormalPlanningCost;
			}
		}


		public void TurnState() {
			_currentPlayer++;
			if (_currentPlayer >= Players.Length) {
				_currentPlayer = 0;
				if (_currentTurn > 0) Print("\n\nGAME STATE\n{0}\n\n-- END OF TURN {1} --\n\n", this, _currentTurn);
				Print("-- BEGINNING OF TURN " + (++_currentTurn) + " --");
				_currentChanceIndex = (_currentChanceIndex + 1)%_chances.Count;
				Print("RISK CHANCE: " + Rules.RiskChances[_currentChanceIndex]);
			}
			PlayerTurn(_currentPlayer);
		}

		private void PlayerTurn(int playerIndex) {
			var player = Players[playerIndex];
			var state = _playerStates[playerIndex];

			Print("\nPLAYER " + playerIndex);

			if (state.State != State.Playing) {
				Print(state.State);
				return;
			}

			if (state.Sprint < 0) {
				state.Sprint++;
				Print("Still planning");
				return;
			}

			bool advance;
			if (!_dice.Roll(_chances[_currentChanceIndex])) {
				advance = true;
				Print("CARD: none, advance");
			} else {
				var card = DrawCard();
				var good = card.Impact < 0;
				var all = Rules.EconomicsAffectsAll && card.Type == Card.EconomicsIndex;
				int value;
				if (!good) {
					var diff = card.Impact - player.PlannedForCard(card.Type);
					advance = diff < Rules.StayOnSprintMinDiff;
					value = Math.Max(0, diff);
				} else {
					advance = true;
					value = card.Impact;
				}
				Print("CARD: {0}, {1}{2} {3} and {4}", card,
					all ? "ALL " : "", good ? "receives" : "pays", Math.Abs(value), advance ? "advance" : "stay");

				if (!all) {
					state.Resources -= value;
					if (state.Resources < 0) {
						state.State = State.Lose;
						if (!Losers.Contains(playerIndex)) Losers.Add(playerIndex);
						advance = false;
					}
				} else {
					for (var i = 0; i < _playerStates.Length; i++) {
						var s = _playerStates[i];
						s.Resources -= Math.Max(0, value - Players[i].PlannedForCard(card.Type));
						if (s.Resources >= 0) continue;
						s.State = State.Lose;
						if (!Losers.Contains(i)) Losers.Add(i);
					}
				}
			}


			if (advance) state.Sprint++;
			if (state.Sprint > Rules.SprintCount) {
				state.State = State.Won;
				Winners.Add(playerIndex);
				if (Rules.OnlyOneWinner) {
					_finished = true;
					for (var i = 0; i < _playerStates.Length; i++) {
						if (i == playerIndex) continue;
						if (!Losers.Contains(i)) Losers.Add(i);
					}
				}
			}
			Print("\t" + state);
		}

		private Card DrawCard() {
			if (_deck.Count <= 0) ShuffleCards();
			var card = _deck[0];
			_deck.RemoveAt(0);
			return card;
		}


		public string ToString(bool initial) {
			if (initial)
				return string.Format("RULES\n-----\n\n{0}\n\n\nPLAYERS:\n{1}\n", Rules, Players.Aggregate("", (s, player) => s + player + "\n\n"));

			var result = "";
			for (var i = 0; i < _playerStates.Length; i++)
				result += i + ": " + _playerStates[i] + "\n";
			return result;
		}

		public override string ToString() { return ToString(false); }


		private void Print(object text, params object[] more) {
			var line = string.Format(text.ToString(), more);
			_buffer.AppendLine(line);
		}
	}
}