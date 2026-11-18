using UnityEngine.InputSystem;

namespace Bezoro.Unity.Core.Input;

/// <summary>
///     Contains extension methods for Input Action.
/// </summary>
public static class InputActionExtensions
{
	public static void Disable(InputActionAsset actionAsset) => actionAsset.DisableChecked();

	public static void DisableChecked(this InputActionAsset actionAsset)
	{
		InputActionExceptions.NullInputActionAssetException.ThrowIfNull(actionAsset);
		actionAsset.Disable();
	}

	public static void Disable(InputAction action) => action.DisableChecked();

	public static void DisableChecked(this InputAction action)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionException.ThrowIfNull(action);
		action.Disable();
	}

	public static void Disable(InputActionMap actionMap) => actionMap.DisableChecked();

	public static void DisableChecked(this InputActionMap actionMap)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionMapException.ThrowIfNull(actionMap);
		actionMap.Disable();
	}

	public static void Enable(InputActionAsset actionAsset) => actionAsset.EnableChecked();

	public static void EnableChecked(this InputActionAsset actionAsset)
	{
		InputActionExceptions.NullInputActionAssetException.ThrowIfNull(actionAsset);
		actionAsset.Enable();
	}

	public static void Enable(InputAction action) => action.EnableChecked();

	public static void EnableChecked(this InputAction action)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionException.ThrowIfNull(action);
		action.Enable();
	}

	public static void Enable(InputActionMap actionMap) => actionMap.EnableChecked();

	public static void EnableChecked(this InputActionMap actionMap)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionMapException.ThrowIfNull(actionMap);
		actionMap.Enable();
	}
}
