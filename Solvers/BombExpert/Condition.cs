using Aiml;
using System;
using System.Collections.Generic;
using System.Text;

namespace BombExpert {
	/// <summary>Represents a specific, queryable condition as used with modules such as Wires.</summary>
	/// <typeparam name="TData">A module-specific type passed to the delegate for the condition.</typeparam>
    public class Condition<TData> {
		public delegate ConditionResult ConditionDelegate(RequestProcess process, TData data);

		public string Key { get; }
		public virtual string Text { get; }
		public ConditionDelegate Delegate { get; }

		public Condition(string key, string text, ConditionDelegate @delegate) {
			this.Key = key;
			this.Text = text;
			this.Delegate = @delegate;
		}

		public override string ToString() => this.Text;

		private static ConditionResult CheckPort(RequestProcess process, PortType portType) {
			switch (process.User.GetPredicate("BombPort" + portType).ToLower()) {
				case "true": return ConditionResult.FromBool(true);
				case "false": return ConditionResult.FromBool(false);
				default: return ConditionResult.Unknown("NeedEdgework Port " + portType);
			}
		}

		public static Condition<TData> Port(PortType portType)
			=> new Condition<TData>(nameof(Port), $"there is {Utils.GetPortDescriptionAn(portType)} present on the bomb", (p, d) => CheckPort(p, portType));
		public static Condition<TData> PortExactKey(PortType portType)
			=> new Condition<TData>(nameof(Port) + portType, $"there is {Utils.GetPortDescriptionAn(portType)} present on the bomb", (p, d) => CheckPort(p, portType));
		public static Condition<TData> IndicatorLit(string label)
			=> new Condition<TData>(nameof(IndicatorLit), $"there is a lit indicator with label {label}",
				(p, d) => {
					switch (p.User.GetPredicate("BombIndicatorLit" + label).ToLower()) {
						case "true": return ConditionResult.FromBool(true);
						case "false": return ConditionResult.FromBool(false);
						default: return ConditionResult.Unknown("NeedEdgework IndicatorLit " + label);
					}
				});
		public static Condition<TData> MoreThanNBatteries(int n)
			=> new Condition<TData>(nameof(MoreThanNBatteries), $"there {(n == 1 ? "is" : "are")} more than {n} {(n == 1 ? "battery" : "batteries")} on the bomb",
				(p, d) => {
					var s = p.User.GetPredicate("BombBatteryCount").ToLower();
					if (s == "unknown") return ConditionResult.Unknown("NeedEdgework BatteryCount");
					return ConditionResult.FromBool(int.Parse(s) > n);
				});
		public static Condition<TData> EmptyPortPlate()
			=> new Condition<TData>(nameof(EmptyPortPlate), $"there is an empty port plate present on the bomb",
				(p, d) => {
					switch (p.User.GetPredicate("BombPortEmptyPlate").ToLower()) {
						case "true": return ConditionResult.FromBool(true);
						case "false": return ConditionResult.FromBool(false);
						default: return ConditionResult.Unknown("NeedEdgework EmptyPortPlate");
					}
				});
		public static Condition<TData> SerialNumberStartsWithLetter()
			=> new Condition<TData>(nameof(SerialNumberStartsWithLetter), $"the serial number starts with a letter",
				(p, d) => {
					switch (p.User.GetPredicate("BombSerialNumberStartsWithLetter").ToLower()) {
						case "true": return ConditionResult.FromBool(true);
						case "false": return ConditionResult.FromBool(false);
						default: return ConditionResult.Unknown("NeedEdgework SerialNumberStartsWithLetter");
					}
				});
		public static Condition<TData> SerialNumberIsOdd()
			=> new Condition<TData>(nameof(SerialNumberIsOdd), $"the last digit of the serial number is odd",
				(p, d) => {
					switch (p.User.GetPredicate("BombSerialNumberIsOdd").ToLower()) {
						case "true": return ConditionResult.FromBool(true);
						case "false": return ConditionResult.FromBool(false);
						default: return ConditionResult.Unknown("NeedEdgework SerialNumberIsOdd");
					}
				});
		public static Condition<TData> SerialNumberIsEven()
			=> new Condition<TData>(nameof(SerialNumberIsEven), $"the last digit of the serial number is even",
				(p, d) => {
					switch (p.User.GetPredicate("BombSerialNumberIsOdd").ToLower()) {
						case "true": return ConditionResult.FromBool(false);
						case "false": return ConditionResult.FromBool(true);
						default: return ConditionResult.Unknown("NeedEdgework SerialNumberIsOdd");
					}
				});

	}

	public enum ConditionResultCode {
		False,
		True,
		Unknown
	}

	public struct ConditionResult {
		public ConditionResultCode Code { get; }
		public string? Details { get; }

		public ConditionResult(ConditionResultCode code) : this(code, null) { }
		public ConditionResult(ConditionResultCode code, string? details) {
			this.Code = code;
			this.Details = details;
		}

		public static ConditionResult operator &(ConditionResult x, ConditionResult y) {
			if (x.Code == ConditionResultCode.False) return x;
			if (y.Code == ConditionResultCode.False) return y;
			if (x.Code == ConditionResultCode.Unknown) return x;
			return y;
		}

		public static bool operator true(ConditionResult value) => value.Code == ConditionResultCode.True;
		public static bool operator false(ConditionResult value) => value.Code == ConditionResultCode.False;

		public static ConditionResult FromBool(bool value) => new ConditionResult(value ? ConditionResultCode.True : ConditionResultCode.False);
		public static ConditionResult Unknown(string message) => new ConditionResult(ConditionResultCode.Unknown, message);
	}
}
