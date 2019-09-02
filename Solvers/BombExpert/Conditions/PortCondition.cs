using Aiml;

namespace BombExpert.Conditions {
	public class PortCondition<TData> : Condition<TData> {
		public PortType PortType { get; }

        public PortCondition(PortType portType) : this(portType, false) { }
        public PortCondition(PortType portType, bool exactKey)
			: base(exactKey ? "Port" + portType : "Port", $"there is {Utils.GetPortDescriptionAn(portType)} present on the bomb") {
			this.PortType = portType;
		}

		public override ConditionResult Query(RequestProcess process, TData data) {
			if (bool.TryParse(process.User.GetPredicate("BombPort" + this.PortType), out var result))
				return ConditionResult.FromBool(result);
			return ConditionResult.Unknown("NeedEdgework Port " + this.PortType);
		}
	}
}
