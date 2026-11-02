using System;
using Bezoro.Core.Extensions;
using Bezoro.Unity.Core.Common.Serialization;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Scenes;

[Serializable]
/// <summary>
/// Stores a reference to Scene Object.
/// </summary>
public class SceneObjectReference
{
	#region Serialized Fields

	[SerializeField] private SerializableGUID guid = new();

	#endregion

	private bool _hasAttemptedResolve;

	private GameObject? _sceneObjectCache;

	public GameObject? Reference
	{
		get
		{
			if (_sceneObjectCache == null && !_hasAttemptedResolve)
				Resolve();

			return _sceneObjectCache;
		}
	}

	public SerializableGUID SerializableGUID => guid;

	public string? GUID
	{
		get => guid?.Value;
		set
		{
			guid ??= new();

			string  newValue      = value ?? string.Empty;
			string? previousValue = guid.Value;

			if (previousValue == newValue) return;

			guid.Set_GUID(
				newValue
			);

			_sceneObjectCache    = null;
			_hasAttemptedResolve = false;
		}
	}

	public void ForceResolve()
	{
		_hasAttemptedResolve = false;
		Resolve();
	}

	public void InitializeGUID() =>
		guid = new();

	private void Resolve()
	{
		if (guid != null &&
			GUIDUtils.Check_GUID_Validity(
				guid.Value
			))
		{
			Logger.Log(
				$"Resolving GUID: {guid.Value.Bold()}"
			);

			_sceneObjectCache = SceneObjectRegistry.GetObjectByGuid(
				guid.Value
			);
		}
		else
		{
			_sceneObjectCache = null;
		}

		_hasAttemptedResolve = true;
	}
}

// Interface for components that have a SceneObjectReference
// (Used by the original SceneObjectRegistry Register method, less needed if component self-registers)
/// <summary>
///     Defines access to a SceneObjectReference value.
/// </summary>
public interface IHasSceneObjectReference
{
	SceneObjectReference SceneObjectReference { get; }
}
