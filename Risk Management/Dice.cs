using System;

namespace RiskManagement {
	public class Dice {
		private readonly float _chance;
		private readonly Random _random;

		public Dice(float chance) {
			_chance = chance;
			_random = new Random((int)DateTime.Now.Ticks);
		}

		public bool Roll() {
			var n = _random.NextDouble();
			return n <= _chance;
		}
	}
}