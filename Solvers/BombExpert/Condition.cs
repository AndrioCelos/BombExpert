using Aiml;
using BombExpert.Conditions;

namespace BombExpert; 
/// <summary>Represents a specific, queryable condition as used with modules such as Wires.</summary>
/// <typeparam name="TData">A module-specific type passed to the delegate for the condition. Not all conditions use this type.</typeparam>
public abstract class Condition<TData> {
	public delegate ConditionResult ConditionDelegate(RequestProcess process, TData data);

	/// <summary>Returns a value used to associate a weight to this condition for random selection.</summary>
	/// <remarks>
	///		Some modules group related conditions by giving them the same key.
	///		This means that all of those conditions will share the same weight and modifications to this weight.
	///		For example, Wires does this with port conditions, but The Button does not.
	/// </remarks>
	public string Key { get; }
	public virtual string Code { get; }
	/// <summary>Returns a human-readable description of this condition.</summary>
	public virtual string Text { get; }

	public Condition(string key, string code, string text) {
		this.Key = key;
		this.Code = code;
		this.Text = text;
	}

	/// <summary>Queries this condition for the specified <see cref="RequestProcess"/> and returns the result.</summary>
	/// <param name="process">The <see cref="RequestProcess"/> currently being processed.</param>
	/// <param name="data">A <typeparamref name="TData"/> containing any necessary module-specific data such as wire colours.</param>
	/// <returns>A <see cref="ConditionResult"/> representing the result of the query.</returns>
	/// <remarks>Currently, some conditions may access predicates such as edgework information from the <see cref="RequestProcess"/>.</remarks>
	public abstract ConditionResult Query(RequestProcess process, TData data);

	/// <summary>Returns the value of the <see cref="Text"/> property.</summary>
	public override string ToString() => this.Text;

	/// <summary>Returns a <see cref="Condition{TData}"/> that checks whether the bomb's serial number ends with an odd digit.</summary>
	public static SerialNumberParityCondition<TData> SerialNumberIsOdd() => new(true);
	/// <summary>Returns a <see cref="Condition{TData}"/> that checks whether the bomb's serial number starts with a letter.</summary>
	public static SerialNumberStartsWithLetterCondition<TData> SerialNumberStartsWithLetter() => new();
}

/// <summary>Specifies whether a condition is met or not met.</summary>
public enum ConditionResultCode {
	/// <summary>The condition is not met.</summary>
	False,
	/// <summary>The condition is met.</summary>
	True,
	/// <summary>Not enough information is available to query the condition. Details should be provided in the <see cref="ConditionResult"/> value.</summary>
	Unknown
}

/// <summary>Specifies the result of a condition query.</summary>
public struct ConditionResult {
	/// <summary>The <see cref="ConditionResultCode"/> indicating whether the condition is met or not met.</summary>
	public ConditionResultCode Code { get; }
	/// <summary>
	///		If <see cref="ConditionResultCode"/> is <see cref="ConditionResultCode.Unknown"/>,
	///		returns a code indicating what information is missing, or null otherwise.
	/// </summary>
	public string? Details { get; }

	private ConditionResult(ConditionResultCode code) : this(code, null) { }
	private ConditionResult(ConditionResultCode code, string? details) {
		this.Code = code;
		this.Details = details;
	}

	/// <summary>Returns the logical conjunction of two <see cref="ConditionResult"/> values.</summary>
	/// <returns>False if either value is false; otherwise, an unknown operand if either is unknown; otherwise, true.</returns>
	public static ConditionResult operator &(ConditionResult x, ConditionResult y) {
		if (x.Code == ConditionResultCode.False) return x;
		if (y.Code == ConditionResultCode.False) return y;
		if (x.Code == ConditionResultCode.Unknown) return x;
		return y;
	}

	/// <summary>Returns true if this value is definitely true.</summary>
	public static bool operator true(ConditionResult value) => value.Code == ConditionResultCode.True;
	/// <summary>Returns true if this value is definitely false.</summary>
	public static bool operator false(ConditionResult value) => value.Code == ConditionResultCode.False;

	/// <summary>Creates a <see cref="Condition{TData}"/> from a <see cref="bool"/> value.</summary>
	public static ConditionResult FromBool(bool value) => new(value ? ConditionResultCode.True : ConditionResultCode.False);
	/// <summary>Creates an unknown <see cref="Condition{TData}"/> from a detail code.</summary>
	public static ConditionResult Unknown(string message) => new(ConditionResultCode.Unknown, message);
}
