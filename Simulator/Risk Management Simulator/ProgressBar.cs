using System;

namespace RiskManagement {
	public class ProgressBar {
		private int _lastValue;

		public float Value {
			set {
				var p = value;
				var pi = (int)(p*100) + 1;
				if (pi == _lastValue) {
					Console.SetCursorPosition(pi + 3, 1);
					Console.Write("{0:P1} ", p);
					return;
				}
				_lastValue = pi;
				Console.SetCursorPosition(pi + 1, 1);
				Console.Write("█ {0:P1} ", p);
			}
		}
	}
}