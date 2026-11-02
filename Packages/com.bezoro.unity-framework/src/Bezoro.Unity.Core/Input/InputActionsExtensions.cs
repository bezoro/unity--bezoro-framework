using UnityEngine.InputSystem;

namespace Bezoro.Unity.Core.Input;

/// <summary>
///     Contains extension methods for Input Action.
/// </summary>
public static class InputActionExtensions
{
	public static void Disable(this InputActionAsset actionAsset)
	{
		InputActionExceptions.NullInputActionAssetException.ThrowIfNull(actionAsset);
		actionAsset.Disable();
	}

	public static void Disable(this InputAction action)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionException.ThrowIfNull(action);
		action.Disable();
	}

	public static void Disable(this InputActionMap actionMap)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionMapException.ThrowIfNull(actionMap);
		actionMap.Disable();
	}

	public static void Enable(this InputActionAsset actionAsset)
	{
		InputActionExceptions.NullInputActionAssetException.ThrowIfNull(actionAsset);
		actionAsset.Enable();
	}

	public static void Enable(this InputAction action)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionException.ThrowIfNull(action);
		action.Enable();
	}

	public static void Enable(this InputActionMap actionMap)
	{
		InputActionExceptions.NullInputActionAssetException.NullInputActionMapException.ThrowIfNull(actionMap);
		actionMap.Enable();
	}
}
