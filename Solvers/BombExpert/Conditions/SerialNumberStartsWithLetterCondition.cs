using System;
using System.Collections.Generic;
using System.Text;
using Aiml;

namespace BombExpert.Conditions {
	public class SerialNumberStartsWithLetterCondition<TData> : Condition<TData> {
		public SerialNumberStartsWithLetterCondition()
			: base($"SerialNumberStartsWithLetter", $"the serial number starts with a letter") { }

		public override ConditionResult Query(RequestProcess process, TData data) {
			if (bool.TryParse(process.User.GetPredicate("SerialNumberStartsWithLetter"), out var result))
				return ConditionResult.FromBool(result);
			return ConditionResult.Unknown("NeedEdgework SerialNumberStartsWithLetter");
		}
	}
}
