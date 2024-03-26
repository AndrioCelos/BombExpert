﻿using System;
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

public class BatteriesCondition<TData>(Operations operation, int number) : Condition<TData>(
	"Batteries" + operation,
	$"Batteries {operation} {number}",
	$"there {(number == 1 ? "is" : "are")} {operation switch { LessThan => "less than", LessThanOrEqualTo => "at most", EqualTo => "exactly", MoreThanOrEqualTo => "at least", MoreThan => "more than", _ => "" }} {number} {(number == 1 ? "battery" : "batteries")} on the bomb"
) {

	public Operations Operation { get; } = operation;
	public int Number { get; } = number;

	public override ConditionResult Query(RequestProcess process, TData data) {
		var s = process.User.GetPredicate("BatteryCount").ToLower();
		if (s == "unknown") return ConditionResult.Unknown("NeedEdgework BatteryCount");

		var batteries = int.Parse(s);
		return this.Operation switch {
			LessThan => batteries < this.Number,
			LessThanOrEqualTo => batteries <= this.Number,
			EqualTo => batteries == this.Number,
			MoreThanOrEqualTo => batteries >= this.Number,
			MoreThan => batteries > this.Number,
			_ => throw new InvalidOperationException("Unknown operation"),
		};
	}
}
