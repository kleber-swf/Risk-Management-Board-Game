using System;

namespace RiskManagement {
	public class Dice {
		private readonly Random _random;

		public Dice() { _random = new Random((int)DateTime.Now.Ticks); }

		public bool Roll(float chance) {
			var n = _random.NextDouble();
			return n <= chance;
		}
	}
}