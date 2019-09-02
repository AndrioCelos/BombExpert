using System;
using System.Collections.Generic;
using System.Text;
using Aiml;

namespace BombExpert.Conditions {
	public class IndicatorCondition<TData> : Condition<TData> {
		public bool? State { get; }
		public string Label { get; }
		public string Predicate => $"BombIndicator{this.State switch { true => "Lit", false => "Unlit", null => "" }}{this.Label}";
		public string EdgeworkQuery => $"Indicator{this.State switch { true => "Lit", false => "Unlit", null => "" }} {this.Label}";

		public IndicatorCondition(bool? state, string label) : base(
				state switch { true => "IndicatorLit", false => "IndicatorUnlit", null => "Indicator" },
				$"there is {state switch { true => "a lit", false => "an unlit", null => "an" }} indicator with label {label}"
			) {
			this.State = state;
			this.Label = label;
		}

		public static IndicatorCondition<TData> Lit(string label) => new IndicatorCondition<TData>(true, label);
		public static IndicatorCondition<TData> Unlit(string label) => new IndicatorCondition<TData>(false, label);
		public static IndicatorCondition<TData> Any(string label) => new IndicatorCondition<TData>(null, label);

		public override ConditionResult Query(RequestProcess process, TData data) {
			if (bool.TryParse(process.User.GetPredicate(this.Predicate), out var result))
				return ConditionResult.FromBool(result);
			return ConditionResult.Unknown("NeedEdgework " + this.EdgeworkQuery);
		}
	}
}
