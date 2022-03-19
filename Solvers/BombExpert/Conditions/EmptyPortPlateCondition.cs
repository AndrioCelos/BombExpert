using Aiml;

namespace BombExpert.Conditions; 
public class EmptyPortPlateCondition<TData> : Condition<TData> {
	public EmptyPortPlateCondition()
		: base("EmptyPortPlate", "there is an empty port plate present on the bomb") { }

	public override ConditionResult Query(RequestProcess process, TData data)
		=> bool.TryParse(process.User.GetPredicate("BombPortEmptyPlate"), out var result)
			? ConditionResult.FromBool(result)
			: ConditionResult.Unknown("NeedEdgework EmptyPortPlate");
}
