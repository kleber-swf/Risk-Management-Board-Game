﻿using System;
using System.Collections.Generic;

namespace RiskManagement {
	public static class ScrambleExtention {
		private static readonly Random Random = new Random();

		public static void Shuffle<T>(this IList<T> list) {
			var n = list.Count;
			while (n > 1) {
				n--;
				var k = Random.Next(n + 1);
				var value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}