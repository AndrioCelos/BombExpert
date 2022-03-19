using System;
using Aiml;
using static BombExpert.Conditions.Operations;

namespace BombExpert.Conditions; 
public enum Operations {
	LessThan,
	LessThanOrEqualTo,
	EqualTo,
	MoreThan,
	MoreThanOrEqualTo
}

public class BatteriesCondition<TData> : Condition<TData> {

	public Operations Operation { get; }
	public int Number { get; }

	public BatteriesCondition(Operations operation, int number) : base(
			"Batteries" + operation,
			$"there {(number == 1 ? "is" : "are")} {operation switch { LessThan => "less than", LessThanOrEqualTo => "at most", EqualTo => "exactly", MoreThanOrEqualTo => "at least", MoreThan => "more than", _ => "" }} {number} {(number == 1 ? "battery" : "batteries")} on the bomb"
		) {
		this.Operation = operation;
		this.Number = number;
	}

	public override ConditionResult Query(RequestProcess process, TData data) {
		var s = process.User.GetPredicate("BombBatteryCount").ToLower();
		if (s == "unknown") return ConditionResult.Unknown("NeedEdgework BatteryCount");

		var batteries = int.Parse(s);
		return this.Operation switch {
			LessThan => ConditionResult.FromBool(batteries < this.Number),
			LessThanOrEqualTo => ConditionResult.FromBool(batteries <= this.Number),
			EqualTo => ConditionResult.FromBool(batteries == this.Number),
			MoreThanOrEqualTo => ConditionResult.FromBool(batteries >= this.Number),
			MoreThan => ConditionResult.FromBool(batteries > this.Number),
			_ => throw new InvalidOperationException("Unknown operation"),
		};
	}
}
