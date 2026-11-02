namespace Bezoro.Unity.Core.Common.Interfaces;

/// <summary>
///     Defines an object that can respond to click input.
/// </summary>
public interface IClickable
{
	public bool IsClick     => IsClickDown || IsClickUp;
	public bool IsClickDown { get; }
	public bool IsClickUp   { get; }

	public void On_Click();
	public void On_Click_Down();
	public void On_Click_Up();
}
