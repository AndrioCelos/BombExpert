using Aiml;

namespace BombExpert.Conditions; 
public class SerialNumberStartsWithLetterCondition<TData> : Condition<TData> {
	public SerialNumberStartsWithLetterCondition()
		: base("SerialNumberStartsWithLetter", "SerialNumberStartsWithLetter", "the serial number starts with a letter") { }

	public override ConditionResult Query(RequestProcess process, TData data)
		=> bool.TryParse(process.User.GetPredicate("SerialNumberStartsWithLetter"), out var result)
			? ConditionResult.FromBool(result)
			: ConditionResult.Unknown("NeedEdgework SerialNumberStartsWithLetter");
}
