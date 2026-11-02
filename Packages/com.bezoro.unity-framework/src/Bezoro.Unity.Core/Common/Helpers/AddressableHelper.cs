using System.Linq;
using Bezoro.Core.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Loads and releases Addressables assets and operation handles.
/// </summary>
public class AddressableHelper
{
	/// <summary>
	///     Checks if the Addressables system has been initialized.
	/// </summary>
	/// <returns>True if Addressables are initialized; otherwise false.</returns>
	public static bool AreAddressablesInitialized()
	{
		bool isInitialized = Addressables.ResourceLocators.Any();

		if (isInitialized)
			Logger.Log(
				"Addressables are initialized."
			);
		else
			Logger.LogWarning(
				"Addressables are not initialized."
			);

		return isInitialized;
	}

	/// <summary>
	///     Checks if an asset of type T exists in the Addressables system with the specified key.
	///     This only works in play mode, not in editor mode.
	/// </summary>
	/// <typeparam name="T">The type of asset to check for.</typeparam>
	/// <param name="key">The Addressables key to look for.</param>
	/// <returns>True if the asset exists in Addressables; otherwise false.</returns>
	public static bool DoesAssetExistInAddressables<T>(object key) where T : Object
	{
		if (key == null)
		{
			Logger.LogWarning("Cannot check Addressables with a null key.");
			return false;
		}

		Logger.Log($"Checking if asset of type {typeof(T).Name.Bold()} exists in Addressables with key: {key}");

		// Early return if not in play mode
		if (!UnityEngine.Application.isPlaying)
		{
			Logger.LogWarning(
				"Cannot check if asset exists in Addressables in editor mode. Returning false."
			);

			return false;
		}

		// Early return if Addressables are not initialized
		if (!AreAddressablesInitialized())
		{
			Logger.LogWarning(
				"Addressables are not initialized. Returning false."
			);

			return false;
		}

		// Search through all resource locators
		foreach (var locator in Addressables.ResourceLocators)
		{
			if (!locator.Locate(
					key,
					typeof(T),
					out _
				)) continue;

			Logger.Log(
				$"Asset of type {typeof(T).Name.Bold()} DOES exist in Addressables: {key}, using locator: {locator.GetType().Name.Bold()}"
			);

			return true;
		}

		// If we've checked all locators and haven't found the key
		Logger.Log(
			$"Asset of type {typeof(T).Name.Bold()} does NOT exist in Addressables: {key}"
		);

		return false;
	}

	/// <summary>
	///     Initializes the Addressables system if it is not already initialized.
	/// </summary>
	public static void InitializeAddressablesIfNeeded()
	{
		Logger.Log(
			"Initializing Addressables system if it is not already initialized."
		);

		if (AreAddressablesInitialized())
		{
			Logger.Log(
				"Addressables are already initialized."
			);

			return;
		}

		Addressables.InitializeAsync().WaitForCompletion();
		Logger.Log(
			"Addressables initialization complete."
		);
	}
}
