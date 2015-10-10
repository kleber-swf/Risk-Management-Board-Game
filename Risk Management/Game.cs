﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJSON;

namespace RiskManagement {
	public class Game {
		private readonly Rules _rules;
		public readonly Player[] Players;
		private readonly PlayerState[] _playerStates;
		private readonly Dice _dice;
		private List<Card> _deck;

		public readonly List<int> Winners = new List<int>();
		public readonly List<int> Losers = new List<int>();
		private int _currentPlayer = int.MaxValue - 2;
		private int _currentTurn;
		private readonly List<int> _forcedRiskSprints = new List<int>();
		private readonly Random _random;

		private readonly StringBuilder _buffer;

		public Game(JSONNode json, StringBuilder buffer) {
			_buffer = buffer;
			_random = new Random((int)DateTime.Now.Ticks);
			_rules = Rules.Deserialize(json["rules"]);
			_dice = new Dice(_rules.RiskChance);

			var playersJson = json["players"];
			var playerCount = playersJson.Count;
			Players = new Player[playerCount];
			for (var i = 0; i < playerCount; i++)
				Players[i] = Player.Deserialize(playersJson[i]);

			_playerStates = new PlayerState[playerCount];
			for (var i = 0; i < playerCount; i++)
				_playerStates[i] = new PlayerState();
		}

		public bool Finished {
			get {
				foreach (var state in _playerStates)
					if (state.State == State.Playing) return false;
				return true;
			}
		}

		public void ShuffleCards() {
			_deck = new List<Card>(_rules.Cards);
			_deck.Shuffle();
		}

		public void Clear() {
			ShuffleCards();
			Winners.Clear();
			Losers.Clear();
			_currentPlayer = int.MaxValue - 2;
			_currentTurn = 0;

			_forcedRiskSprints.Clear();

			while (_forcedRiskSprints.Count < _rules.ForcedRiskCount) {
				var r = (int)Math.Floor(_random.NextDouble()*_rules.SprintCount);
				if (_forcedRiskSprints.Contains(r)) continue;
				_forcedRiskSprints.Add(r);
			}
		}

		public void PlanningState() {
			for (var i = 0; i < Players.Length; i++) {
				var player = Players[i];
				var state = _playerStates[i];

				state.Sprint = 0;
				state.Resources = _rules.InitialResources;
				state.State = State.Playing;

				var planningCardsCount = player.PlanningCardsCount;
				var over = 0;

				if (planningCardsCount == 0) state.Sprint = 1;
				else {
					over = planningCardsCount - _rules.NormalPlanningCount;
					if (player.UseOneMorePlanningSprint) {
						state.Sprint = -1;
						state.Resources -= over*_rules.NormalPlanningCost;
					} else if (over > 0)
						state.Resources -= over*_rules.OverPlanningCost;
					else over = 0;
				}
				state.Resources -= (planningCardsCount - over)*_rules.NormalPlanningCost;
			}
		}


		public void TurnState() {
			_currentPlayer++;
			if (_currentPlayer >= Players.Length) {
				_currentPlayer = 0;
				if (_currentTurn > 0) Print("\n\nGAME STATE\n{0}\n\n-- END OF TURN {1} --\n\n", this, _currentTurn);
				Print("-- BEGINNING OF TURN " + (++_currentTurn) + " --");
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
			if (!(ForceRisk(state.Sprint) || _dice.Roll())) {
				advance = true;
				Print("CARD: none, advance");
			} else {
				var card = DrawCard();
				var good = card.Impact < 0;
				var all = _rules.EconomicsAffectsAll && card.Type == Card.EconomicsIndex;
				int value;
				if (!good) {
					var diff = card.Impact - player.PlannedForCard(card.Type);
					advance = diff < _rules.StayOnSprintMinDiff;
					value = Math.Max(0, diff);
				} else {
					advance = true;
					value = card.Impact;
				}
				Print("CARD: {0}, {1}{2} {3} and {4}", card,
					all ? "ALL " : "", good ? "receives" : "pays", value, advance ? "advance" : "stay");

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
			if (state.Sprint > _rules.SprintCount) {
				state.State = State.Won;
				Winners.Add(playerIndex);
			}
			Print("\t" + state);
		}

		private bool ForceRisk(int sprint) {
			var r = _forcedRiskSprints.Contains(sprint);
			if (r) Print("Forced!");
			return r;
		}

		private Card DrawCard() {
			if (_deck.Count <= 0) ShuffleCards();
			var card = _deck[0];
			_deck.RemoveAt(0);
			return card;
		}


		public string ToString(bool initial) {
			if (initial)
				return string.Format("RULES\n-----\n\n{0}\n\n\nPLAYERS:\n{1}\n", _rules, Players.Aggregate("", (s, player) => s + player + "\n\n"));

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