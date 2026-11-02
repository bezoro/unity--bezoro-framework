using System;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Raycast.
/// </summary>
public static class RaycastExtensions
{
	public static bool Raycast2D(
		this Vector2     mousePosition,
		Camera           mainCamera,
		RaycastHit2D[]   raycastHits2D,
		out GameObject[] hitObjects
	)
	{
		if (mainCamera == null || raycastHits2D == null || raycastHits2D.Length == 0)
		{
			hitObjects = Array.Empty<GameObject>();
			return false;
		}

		var worldPoint = mainCamera.ScreenToWorldPoint(
			mousePosition
		);

		var contactFilter = new ContactFilter2D();
		int hitCount = Physics2D.Raycast(
			worldPoint,
			Vector2.zero,
			contactFilter,
			raycastHits2D,
			200f
		);

		if (hitCount > 0)
		{
			var hitObjectsArray = new GameObject[hitCount];

			var objectCount = 0;
			for (var i = 0; i < hitCount; i++)
			{
				if (raycastHits2D[i].collider == null) continue;

				hitObjectsArray[objectCount++] = raycastHits2D[i].collider.gameObject;
			}

			if (objectCount == 0)
			{
				hitObjects = Array.Empty<GameObject>();
				return false;
			}

			if (objectCount != hitObjectsArray.Length)
				Array.Resize(ref hitObjectsArray, objectCount);

			hitObjects = hitObjectsArray;
			return true;
		}

		hitObjects = Array.Empty<GameObject>();
		return false;
	}

	public static bool Raycast3D(this Ray ray, RaycastHit[] hits, out GameObject[] hitGameObjects)
	{
		if (hits == null || hits.Length == 0)
		{
			hitGameObjects = Array.Empty<GameObject>();
			return false;
		}

		int hitCount = Physics.RaycastNonAlloc(
			ray,
			hits,
			Mathf.Infinity
		);

		if (hitCount > 0)
		{
			var hitObjectsArray = new GameObject[hitCount];

			var objectCount = 0;
			for (var i = 0; i < hitCount; i++)
			{
				if (hits[i].collider == null) continue;

				hitObjectsArray[objectCount++] = hits[i].collider.gameObject;
			}

			if (objectCount == 0)
			{
				hitGameObjects = Array.Empty<GameObject>();
				return false;
			}

			if (objectCount != hitObjectsArray.Length)
				Array.Resize(ref hitObjectsArray, objectCount);

			hitGameObjects = hitObjectsArray;
			return true;
		}

		hitGameObjects = Array.Empty<GameObject>();
		return false;
	}
}
