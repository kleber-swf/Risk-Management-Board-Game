using SimpleJSON;

namespace RiskManagement {
	public class Player {
		public string Name { get; private set; }
		private int[] PlanningCards { get; set; }
		public bool UseOneMorePlanningSprint { get; private set; }

		public int PlannedForCard(int cardTypeIndex) { return PlanningCards.Length > 0 ? PlanningCards[cardTypeIndex] : 0; }

		public int PlanningCardsCount {
			get {
				var result = 0;
				foreach (var card in PlanningCards) {
					if (card > 0) result++;
				}
				return result;
			}
		}

		private Player() { }

		public override string ToString() {
			return string.Format(
				"name: {0},\nplanning-cards: [{1}]\nuse-one-more-planning-sprint: {2}",
				Name,
				PlanningCardsToString(),
				UseOneMorePlanningSprint);
		}

		public string PlanningCardsToString() {
			var result = " ";
			for (var i = 0; i < PlanningCards.Length; i++) {
				var c = PlanningCards[i];
				if (c == 0) continue;
				result += Card.CardTypeNames[i] + "x" + c + " ";
			}
			return result;
		}

		public static Player Deserialize(JSONNode json) {
			return new Player {
				Name = json["name"].Value,
				PlanningCards = ParsePlanningCards(json["planning-cards"]),
				UseOneMorePlanningSprint = json["use-one-more-planning-sprint"].AsBool
			};
		}

		private static int[] ParsePlanningCards(JSONNode json) {
			var len = json.Count;
			var result = new int[len];
			for (var i = 0; i < len; i++)
				result[i] = json[i].AsInt;
			return result;
		}
	}
}