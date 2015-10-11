using System.Collections.Generic;
using SimpleJSON;

namespace RiskManagement {
	public class Rules {
		public int SprintCount { get; private set; }
		public int MaxImpact { get; private set; }
		public Card[] Cards { get; private set; }

		public int InitialResources { get; private set; }
		public int NormalPlanningCount { get; private set; }
		public int NormalPlanningCost { get; private set; }
		public int OverPlanningCost { get; private set; }
		public int SprintsWonWithoutPlanning { get; private set; }

		public float RiskChance { get; private set; }

		public bool EconomicsAffectsAll { get; private set; }
		public int StayOnSprintMinDiff { get; private set; }
		public int ForcedRiskCount { get; private set; }
		public bool OnlyOneWinner { get; private set; }

		private Rules() { }

		public static Rules Deserialize(JSONNode json) {
			var maxImpact = json["max-impact"].AsInt;
			return new Rules {
				SprintCount = json["sprints"].AsInt,
				MaxImpact = maxImpact,
				Cards = ParseDeck(json["deck"], maxImpact),
				InitialResources = json["initial-resources"].AsInt,
				NormalPlanningCount = json["normal-planning-count"].AsInt,
				NormalPlanningCost = json["normal-planning-cost"].AsInt,
				OverPlanningCost = json["over-planning-cost"].AsInt,
				SprintsWonWithoutPlanning = json["sprints-won-without-planning"].AsInt,
				RiskChance = json["risk-chance"].AsFloat,
				EconomicsAffectsAll = json["economics-affects-all"].AsBool,
				StayOnSprintMinDiff = json["stay-on-sprint-min-diff"].AsInt,
				ForcedRiskCount = json["forced-risks"].AsInt,
				OnlyOneWinner = json["only-one-winner"].AsBool
			};
		}

		private static Card[] ParseDeck(JSONNode json, int maxImpact) {
			var length = json.Count;
			var result = new List<Card>();
			for (var i = 0; i < length; i++) {
				var node = json[i];
				for (var j = 0; j < maxImpact*2; j++) {
					for (var k = 0; k < node[j].AsInt; k++) {
						var good = j < maxImpact;
						result.Add(new Card {
							Impact = good ? -(maxImpact - j) : maxImpact*2 - j,
							Type = i
						});
					}
				}
			}
			return result.ToArray();
		}

		public override string ToString() {
			return string.Format("sprints: {0},\nmax-impact:{1},\ndeck: \n{2},\n\ninitial-resources: {3},\n\nnormal-planning-count: {4},\n" +
			                     "normal-planning-cost: {5},\nover-planning-cost: {6},\nsprints-won-without-planning: {7},\n\n" +
			                     "risk-chance: {8},\n\n" + "economics-affects-all: {9},\nstay-on-sprint-min-diff: {10},\n" +
			                     "forced-risks: {11},\nonly-one-winner: {12}",
				SprintCount,
				MaxImpact,
				CardsToString(),
				InitialResources,
				NormalPlanningCount,
				NormalPlanningCost,
				OverPlanningCost,
				SprintsWonWithoutPlanning,
				RiskChance,
				EconomicsAffectsAll,
				StayOnSprintMinDiff,
				ForcedRiskCount,
				OnlyOneWinner
				);
		}

		private string CardsToString() {
			var result = "[\n\t";
			var lastType = 0;
			foreach (var card in Cards) {
				if (lastType != card.Type) {
					result += "\n\t";
					lastType = card.Type;
				}
				result += card + " ";
			}
			return result + "\n]";
		}
	}
}