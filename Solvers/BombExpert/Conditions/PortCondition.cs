using Aiml;

namespace BombExpert.Conditions; 
public class PortCondition<TData>(PortType portType, bool exactKey) : Condition<TData>(
	exactKey ? "Port" + portType : "Port",
	$"Port {portType}",
	$"there is {Utils.GetPortDescriptionAn(portType)} present on the bomb"
) {
	public PortType PortType { get; } = portType;

	public PortCondition(PortType portType) : this(portType, false) { }

	public override ConditionResult Query(RequestProcess process, TData data)
		=> bool.TryParse(process.User.GetPredicate("BombPort" + this.PortType), out var result)
			? result
			: ConditionResult.Unknown("NeedEdgework Port " + this.PortType);
}
