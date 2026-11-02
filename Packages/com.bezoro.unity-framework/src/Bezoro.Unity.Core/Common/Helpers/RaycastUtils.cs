using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Creates rays and runs Unity physics raycasts.
/// </summary>
public static class RaycastUtils
{
	// Pre-allocate an array for RaycastNonAlloc to reduce garbage collection.
	// Adjust size based on expected maximum number of overlapping colliders.
	//TODO: Make configurable in the inspector (SO?)
	private static readonly RaycastHit[] Hits = new RaycastHit[64];

	public static bool TryGetAllUnderPointer(Camera? camera, out RaycastHit[] hits, bool sortByDistance = false)
	{
		int hitCount = RaycastAllUnderPointerNonAlloc(
			camera,
			Mathf.Infinity
		);

		if (hitCount <= 0)
		{
			hits = Array.Empty<RaycastHit>();
			return false;
		}

		hits = new RaycastHit[hitCount];
		Array.Copy(
			Hits,
			hits,
			hitCount
		);

		if (sortByDistance)
			Array.Sort(
				hits,
				static (a, b) => a.distance.CompareTo(b.distance)
			);

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetAllWithComponentUnderPointer<T>(
		Camera?                                    camera,
		out (T component, GameObject gameObject)[] objects)
		where T : Component
	{
		int hitCount = RaycastAllUnderPointerNonAlloc(camera, Mathf.Infinity);

		if (hitCount <= 0)
		{
			objects = Array.Empty<(T, GameObject)>();
			return false;
		}

		var foundObjects = new Dictionary<T, GameObject>();

		for (var i = 0; i < hitCount; i++)
		{
			var collider = Hits[i].collider;
			if (collider.IsUnityNull())
				continue;

			var go = collider.gameObject;
			if (go.TryGetComponentInGameObjectHierarchy(out T? component) && !component.IsUnityNull())
				foundObjects.TryAdd(component, go);
		}

		objects = foundObjects.Select(kvp => (kvp.Key, kvp.Value)).ToArray();

		return foundObjects.Count > 0;
	}

	/// <summary>
	///     Performs a RaycastNonAlloc from the current pointer position through the given camera.
	///     The results are stored in the private static Hits buffer.
	/// </summary>
	/// <returns>The number of colliders hit. (-1 if no camera)</returns>
	private static int RaycastAllUnderPointerNonAlloc(
		Camera? camera,
		float   maxHitDistance,
		int     hitLayerMask = Physics.DefaultRaycastLayers)
	{
		camera ??= Camera.main;
		if (!camera) return -1;

		var pointer = Pointer.current;
		if (pointer == null) return -1;

		Array.Clear(Hits, 0, Hits.Length);

		var pointerPos = pointer.position.ReadValue();
		var ray        = camera.ScreenPointToRay(pointerPos);

		return Physics.RaycastNonAlloc(ray, Hits, maxHitDistance, hitLayerMask);
	}
}
