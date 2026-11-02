namespace Bezoro.Unity.Core.Common.Enums;

/// <summary>
///     Defines shared DefaultExecutionOrder values used by framework behaviours.
/// </summary>
public static class ExecutionOrder
{
	public const int Default      = 0;
	public const int Early        = -1;
	public const int EarlyManager = int.MinValue + 1;
	public const int Fundamental  = int.MinValue;
	public const int Late         = 1;
	public const int LateManager  = int.MinValue + 3;
	public const int Manager      = int.MinValue + 2;
}
