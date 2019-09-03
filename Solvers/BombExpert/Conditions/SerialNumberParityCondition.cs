using Aiml;

namespace BombExpert.Conditions {
	public class SerialNumberParityCondition<TData> : Condition<TData> {
		public bool Odd { get; }

		public SerialNumberParityCondition(bool odd)
			: base($"SerialNumberIs{(odd ? "Odd" : "Even")}", $"the last digit of the serial number is {(odd ? "odd" : "even")}") {
			this.Odd = odd;
		}

		public override ConditionResult Query(RequestProcess process, TData data) {
			if (bool.TryParse(process.User.GetPredicate("BombSerialNumberIsOdd"), out var result))
				return ConditionResult.FromBool(result == this.Odd);
			return ConditionResult.Unknown("NeedEdgework SerialNumberIsOdd");
		}
	}
}
