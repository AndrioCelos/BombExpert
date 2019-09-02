using Aiml;
using BombExpert.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BombExpert {
	/// <summary>Represents a specific, queryable condition as used with modules such as Wires.</summary>
	/// <typeparam name="TData">A module-specific type passed to the delegate for the condition.</typeparam>
    public abstract class Condition<TData> {
		public delegate ConditionResult ConditionDelegate(RequestProcess process, TData data);

		public string Key { get; }
		public virtual string Text { get; }

		public Condition(string key, string text) {
			this.Key = key;
			this.Text = text;
		}

		public abstract ConditionResult Query(RequestProcess process, TData data);

		public override string ToString() => this.Text;
		public static SerialNumberParityCondition<TData> SerialNumberIsOdd() => new SerialNumberParityCondition<TData>(true);
		public static SerialNumberStartsWithLetterCondition<TData> SerialNumberStartsWithLetter() => new SerialNumberStartsWithLetterCondition<TData>();
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
