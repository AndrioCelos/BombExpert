using Aiml;

namespace BombExpert.Conditions; 
public class SerialNumberParityCondition<TData> : Condition<TData> {
	public bool Odd { get; }

	public SerialNumberParityCondition(bool odd)
		: base($"SerialNumberIs{(odd ? "Odd" : "Even")}", $"SerialNumberIs{(odd ? "Odd" : "Even")}", $"the last digit of the serial number is {(odd ? "odd" : "even")}")
		=> this.Odd = odd;

	public override ConditionResult Query(RequestProcess process, TData data)
		=> bool.TryParse(process.User.GetPredicate("BombSerialNumberIsOdd"), out var result)
			? ConditionResult.FromBool(result == this.Odd)
			: ConditionResult.Unknown("NeedEdgework SerialNumberIsOdd");
}
