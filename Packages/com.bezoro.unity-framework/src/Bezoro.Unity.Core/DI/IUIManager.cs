namespace Bezoro.Unity.Core.DI;

/// <summary>
///     Service abstraction for UI systems that expose menus by string key.
/// </summary>
public interface IUIManager : IService
{
	/// <summary>
	///     Shows the menu identified by <paramref name="menuName" />.
	/// </summary>
	public void ShowMenu(string menuName);
}
