using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	/// <summary>
	///     Provides native managed-reference selection for interface property drawers.
	/// </summary>
	public class SerializedReferenceInterfaceDrawer : PropertyDrawer
	{
		protected static bool CanCreateImplementation(Type type, Type interfaceType) =>
			type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters &&
			type.IsSerializable && interfaceType.IsAssignableFrom(type) &&
			!typeof(Object).IsAssignableFrom(type) &&
			type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				null, Type.EmptyTypes, null) != null;

		protected static void ShowImplementationMenu(SerializedProperty property, Type interfaceType)
		{
			var menu = new GenericMenu();
			var selectedType = property.managedReferenceValue?.GetType();
			var targets = property.serializedObject.targetObjects;
			var propertyPath = property.propertyPath;
			menu.AddItem(new GUIContent("None"), selectedType == null,
				() => AssignImplementation(targets, propertyPath, null));

			var candidates = TypeCache.GetTypesDerivedFrom(interfaceType)
				.Where(type => CanCreateImplementation(type, interfaceType))
				.OrderBy(type => type.FullName, StringComparer.Ordinal);
			foreach (var candidate in candidates)
			{
				var implementation = candidate;
				menu.AddItem(new GUIContent(implementation.FullName.Replace('.', '/')),
					selectedType == implementation,
					() => AssignImplementation(targets, propertyPath, implementation));
			}

			menu.ShowAsContext();
		}

		private static void AssignImplementation(Object[] targets, string propertyPath, Type? implementation)
		{
			// Create every value before writing so a throwing constructor leaves all targets unchanged.
			object?[] values;
			try
			{
				values = targets.Select(_ => implementation == null
					? null : Activator.CreateInstance(implementation, true)).ToArray();
			}
			catch (Exception exception)
			{
				Debug.LogError($"Cannot create interface implementation: {exception.GetBaseException().Message}");
				return;
			}

			Undo.SetCurrentGroupName("Change interface implementation");
			var undoGroup = Undo.GetCurrentGroup();
			for (var i = 0; i < targets.Length; i++)
			{
				if (targets[i] == null) continue;
				using var serializedObject = new SerializedObject(targets[i]);
				var property = serializedObject.FindProperty(propertyPath);
				if (property == null || property.propertyType != SerializedPropertyType.ManagedReference) continue;
				property.managedReferenceValue = values[i];
				serializedObject.ApplyModifiedProperties();
			}
			Undo.CollapseUndoOperations(undoGroup);
		}
	}
}
