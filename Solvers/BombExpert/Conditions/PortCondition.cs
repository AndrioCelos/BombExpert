using Aiml;

namespace BombExpert.Conditions; 
public class PortCondition<TData> : Condition<TData> {
	public PortType PortType { get; }

	public PortCondition(PortType portType) : this(portType, false) { }
	public PortCondition(PortType portType, bool exactKey)
		: base(exactKey ? "Port" + portType : "Port", $"Port {portType}", $"there is {Utils.GetPortDescriptionAn(portType)} present on the bomb")
		=> this.PortType = portType;

	public override ConditionResult Query(RequestProcess process, TData data)
		=> bool.TryParse(process.User.GetPredicate("BombPort" + this.PortType), out var result)
			? ConditionResult.FromBool(result)
			: ConditionResult.Unknown("NeedEdgework Port " + this.PortType);
}
