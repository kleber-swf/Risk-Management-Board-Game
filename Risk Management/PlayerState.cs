namespace RiskManagement {
	public class PlayerState {
		public int Sprint;
		public int Resources;
		public State State;

		public override string ToString() { return string.Format("sprint: {0}, resources: {1}, state: {2}", Sprint, Resources, State); }
	}
}