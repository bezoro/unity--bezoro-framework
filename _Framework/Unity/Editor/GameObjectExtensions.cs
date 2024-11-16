using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor
{
	public static class GameObjectExtensions
	{
		public static string TryGetAssetGUID( this GameObject gameObject )
		{
			var path = AssetDatabase.GetAssetPath( gameObject );
			if ( !string.IsNullOrEmpty( path ) )
			{
				return AssetDatabase.AssetPathToGUID( path );
			}

			BDebug.LogError( "GameObject is not an asset or is not part of the project." );

			return null;
		}

		public static bool IsPrefabVariant( this GameObject gameObject ) =>
			PrefabUtility.GetPrefabAssetType( gameObject ) == PrefabAssetType.Variant;

		public static bool IsNotPrefabVariant( this GameObject gameObject ) =>
			PrefabUtility.GetPrefabAssetType( gameObject ) != PrefabAssetType.Variant;

		public static bool IsPrefab( this GameObject gameObject ) =>
			PrefabUtility.GetPrefabAssetType( gameObject ) != PrefabAssetType.NotAPrefab;

		public static bool IsNotPrefab( this GameObject gameObject ) =>
			PrefabUtility.GetPrefabAssetType( gameObject ) == PrefabAssetType.NotAPrefab;

		public static void SetLayerRecursively( this GameObject gameObject, int layer )
		{
			gameObject.layer = layer;
			foreach ( Transform child in gameObject.transform )
			{
				child.gameObject.SetLayerRecursively( layer );
			}
		}

		public static void SetActiveChildren( this GameObject gameObject, bool isActive )
		{
			foreach ( Transform child in gameObject.transform )
			{
				child.gameObject.SetActive( isActive );
			}
		}

		public static T GetComponentInChildrenIncludingInactive<T>( this GameObject gameObject ) where T : Component
		{
			foreach ( var childTransform in gameObject.GetComponentsInChildren<Transform>( true ) )
			{
				var component = childTransform.GetComponent<T>();
				if ( component != null )
				{
					return component;
				}
			}

			return null;
		}

		public static void DestroyChildren( this GameObject gameObject )
		{
			foreach ( Transform child in gameObject.transform )
			{
				Object.Destroy( child.gameObject );
			}
		}

		public static void ResetTransform( this GameObject gameObject )
		{
			var transform = gameObject.transform;
			transform.position   = Vector3.zero;
			transform.rotation   = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		public static bool HasComponent<T>
			( this GameObject gameObject ) where T : Component =>
			gameObject.GetComponent<T>() != null;

		public static void CopyComponentsFrom( this GameObject destination, GameObject source )
		{
			foreach ( var sourceComponent in source.GetComponents<Component>() )
			{
				if ( sourceComponent is Transform )
				{
					continue;
				}

				var destinationComponent = destination.GetComponent( sourceComponent.GetType() );
				if ( !destinationComponent )
				{
					destinationComponent = destination.AddComponent( sourceComponent.GetType() );
				}

				foreach ( var field in sourceComponent.GetType()
													  .GetFields(
														  BindingFlags.Public
														| BindingFlags.NonPublic
														| BindingFlags.Instance
													  ) )
				{
					field.SetValue( destinationComponent, field.GetValue( sourceComponent ) );
				}

				foreach ( var prop in sourceComponent.GetType().GetProperties(
							 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
						 ) )
				{
					if ( prop.CanWrite )
					{
						prop.SetValue( destinationComponent, prop.GetValue( sourceComponent, null ), null );
					}
				}
			}
		}

		public static void SetParentAndReset( this GameObject child, GameObject parent )
		{
			child.transform.SetParent( parent.transform );
			child.transform.localPosition = Vector3.zero;
			child.transform.localRotation = Quaternion.identity;
			child.transform.localScale    = Vector3.one;
		}
	}
}
