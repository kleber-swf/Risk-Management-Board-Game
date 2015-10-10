using System;

namespace RiskManagement {
	public class Card {
		public static readonly string[] CardTypeNames = { "F", "M", "T", "C", "E" };
		public const int EconomicsIndex = 4;

		public int Type;
		public int Impact;

		public override string ToString() { return string.Format("{0}{1}{2}", CardTypeNames[Type], Impact < 0 ? "G" : "B", Math.Abs(Impact)); }
	}
}