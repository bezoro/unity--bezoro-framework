using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.EditorWindows
{
	/// <summary>
	///     Copies selected non-transform components from one GameObject to another.
	/// </summary>
	public class ComponentCopyToolWindow : EditorWindow
	{
		private static readonly HashSet<Type> NonCopyableComponents = new()
		{
			typeof(Transform),
			typeof(RectTransform)
		};
		private readonly Dictionary<Component, bool> _componentSelection = new();
		private readonly List<Component>             _sourceComponents   = new();
		private          GameObject?                 _sourceGameObject;
		private          GameObject?                 _targetGameObject;
		private          int                         _lastComponentCount;
		private          Vector2                     _scrollPosition;

		#region Unity Lifecycle

		private void OnGUI()
		{
			EditorGUILayout.LabelField(
				"Component Copy Tool",
				EditorStyles.boldLabel
			);

			EditorGUILayout.Space();

			DrawSourceObjectField();
			EditorGUILayout.Space();
			DrawTargetObjectField();
			EditorGUILayout.Space();

			if (_sourceGameObject != null)
			{
				DrawComponentList();
				EditorGUILayout.Space();
				DrawCopyButtons();
			}
			else
			{
				EditorGUILayout.HelpBox(
					"Select a source GameObject to begin.",
					MessageType.Info
				);
			}
		}

		private void OnInspectorUpdate()
		{
			// Only refresh if the actual component count has changed
			if (_sourceGameObject != null)
			{
				var currentComponents = _sourceGameObject.GetComponents<Component>();
				if (currentComponents.Length != _lastComponentCount)
				{
					RefreshComponentList();
					Repaint();
				}
			}
		}

		private void OnSelectionChange()
		{
			// Auto-assign selected GameObject as source if nothing is selected
			if (_sourceGameObject == null && Selection.activeGameObject != null)
			{
				_sourceGameObject = Selection.activeGameObject;
				RefreshComponentList();
				Repaint();
			}
		}

		#endregion

		[MenuItem(
			Constants.BEZORO + "/Tools/Component Copy Tool"
		)]
		public static void ShowWindow()
		{
			GetWindow<ComponentCopyToolWindow>(
				"Component Copy Tool"
			);
		}

		private bool CanRemoveComponent(Component component) =>
			// Don't remove Transform, RectTransform, or other essential components
			!NonCopyableComponents.Contains(
				component.GetType()
			);

		private void CopyAllComponents()
		{
			if (_targetGameObject == null || EditorUtility.IsPersistent(_targetGameObject)) return;

			CopyComponents(_sourceComponents, _targetGameObject);

			Debug.Log(
				$"Copied {_sourceComponents.Count} components to {_targetGameObject.name}"
			);
		}

		internal static void CopyComponents(IEnumerable<Component> components, GameObject target)
		{
			if (EditorUtility.IsPersistent(target))
				throw new InvalidOperationException("Open the prefab in Prefab Mode to edit its components.");

			var nextIndex = new Dictionary<Type, int>();
			foreach (var source in components)
			{
				if (source == null || source.gameObject == target) continue;
				var type = source.GetType();
				nextIndex.TryGetValue(type, out var index);
				nextIndex[type] = index + 1;

				var matches = target.GetComponents(type).Where(c => c.GetType() == type).ToArray();
				var destination = index < matches.Length ? matches[index] : Undo.AddComponent(target, type);
				if (destination == null) continue;
				Undo.RecordObject(destination, "Copy Component");
				EditorUtility.CopySerialized(source, destination);
				PrefabUtility.RecordPrefabInstancePropertyModifications(destination);
			}
		}

		private void CopySelectedComponents()
		{
			if (_targetGameObject == null || EditorUtility.IsPersistent(_targetGameObject)) return;
			CopyComponents(_sourceComponents.Where(c => c != null && _componentSelection[c]), _targetGameObject);

			Debug.Log(
				$"Copied {_componentSelection.Values.Count(v => v)} components to {_targetGameObject.name}"
			);
		}

		private void DrawComponentList()
		{
			if (_sourceComponents.Count == 0)
			{
				EditorGUILayout.HelpBox(
					"No copyable components found on the source GameObject.",
					MessageType.Warning
				);

				return;
			}

			EditorGUILayout.LabelField(
				"Select Components to Copy:",
				EditorStyles.boldLabel
			);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(
					"Select All"
				))
				foreach (var component in _sourceComponents)
					_componentSelection[component] = true;

			if (GUILayout.Button(
					"Select None"
				))
				foreach (var component in _sourceComponents)
					_componentSelection[component] = false;

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			_scrollPosition = EditorGUILayout.BeginScrollView(
				_scrollPosition,
				GUILayout.Height(
					300
				)
			);

			foreach (var component in _sourceComponents)
			{
				if (component == null) continue;

				EditorGUILayout.BeginHorizontal();

				bool isSelected = _componentSelection.ContainsKey(
									  component
								  ) &&
								  _componentSelection[component];

				bool newSelection = EditorGUILayout.Toggle(
					isSelected,
					GUILayout.Width(
						20
					)
				);

				_componentSelection[component] = newSelection;

				// Component icon and name
				var componentContent = new GUIContent(
					ObjectNames.NicifyVariableName(
						component.GetType().Name
					),
					EditorGUIUtility.ObjectContent(
						component,
						component.GetType()
					).image
				);

				EditorGUILayout.LabelField(
					componentContent,
					GUILayout.ExpandWidth(
						true
					)
				);

				// Show component status
				if (_targetGameObject != null)
				{
					bool hasComponent = _targetGameObject.GetComponent(
											component.GetType()
										) !=
										null;

					if (hasComponent)
						EditorGUILayout.LabelField(
							"(Exists)",
							EditorStyles.miniLabel,
							GUILayout.Width(
								60
							)
						);
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
		}

		private void DrawCopyButtons()
		{
			if (_targetGameObject == null)
			{
				EditorGUILayout.HelpBox(
					"Select a target GameObject to copy components to.",
					MessageType.Info
				);

				return;
			}

			if (EditorUtility.IsPersistent(_targetGameObject))
			{
				EditorGUILayout.HelpBox("Open the target prefab in Prefab Mode to edit its components.", MessageType.Info);
				return;
			}
			if (_sourceGameObject == _targetGameObject)
			{
				EditorGUILayout.HelpBox(
					"Source and target GameObjects cannot be the same.",
					MessageType.Warning
				);

				return;
			}

			int selectedCount = _componentSelection.Values.Count(selected => selected
			);

			EditorGUILayout.BeginHorizontal();

			GUI.enabled = selectedCount > 0;
			if (GUILayout.Button(
					$"Copy Selected Components ({selectedCount})"
				)) CopySelectedComponents();

			GUI.enabled = true;
			if (GUILayout.Button(
					"Copy All Components"
				)) CopyAllComponents();

			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			// Options
			EditorGUILayout.LabelField(
				"Options:",
				EditorStyles.boldLabel
			);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(
					"Remove Duplicate Components"
				)) RemoveDuplicateComponents();

			EditorGUILayout.EndHorizontal();
		}

		private void DrawSourceObjectField()
		{
			EditorGUILayout.LabelField(
				"Source GameObject:",
				EditorStyles.boldLabel
			);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(
					"Use Selection",
					GUILayout.Width(
						100
					)
				))
				if (Selection.activeGameObject != null)
				{
					_sourceGameObject = Selection.activeGameObject;
					RefreshComponentList();
				}

			// Use a smaller label width to give more space to the field
			EditorGUIUtility.labelWidth = 50;
			var newSourceObject = (GameObject)EditorGUILayout.ObjectField(
				"Source",
				_sourceGameObject,
				typeof(GameObject),
				true
			);

			EditorGUILayout.EndHorizontal();

			if (newSourceObject != _sourceGameObject)
			{
				_sourceGameObject = newSourceObject;
				RefreshComponentList();
			}
		}

		private void DrawTargetObjectField()
		{
			EditorGUILayout.LabelField(
				"Target GameObject:",
				EditorStyles.boldLabel
			);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(
					"Use Selection",
					GUILayout.Width(
						100
					)
				))
				if (Selection.activeGameObject != null)
					_targetGameObject = Selection.activeGameObject;

			// Use a smaller label width to give more space to the field
			EditorGUIUtility.labelWidth = 50;
			var newTargetObject = (GameObject)EditorGUILayout.ObjectField(
				"Target",
				_targetGameObject,
				typeof(GameObject),
				true
			);

			EditorGUILayout.EndHorizontal();

			if (newTargetObject != _targetGameObject) _targetGameObject = newTargetObject;
		}

		private void RefreshComponentList()
		{
			// Store current selections before clearing
			var currentSelections = new Dictionary<Component, bool>();
			foreach (var kvp in _componentSelection)
			{
				if (kvp.Key != null)
					currentSelections[kvp.Key] = kvp.Value;
			}

			_sourceComponents.Clear();
			_componentSelection.Clear();

			if (_sourceGameObject == null)
			{
				_lastComponentCount = 0;
				return;
			}

			var allComponents = _sourceGameObject.GetComponents<Component>();

			foreach (var component in allComponents)
			{
				if (component == null) continue;

				var componentType = component.GetType();

				// Skip non-copyable components
				if (NonCopyableComponents.Contains(
						componentType
					)) continue;

				_sourceComponents.Add(
					component
				);

				// Restore previous selection if it existed
				bool wasSelected = currentSelections.TryGetValue(component, out var selected) && selected;

				_componentSelection[component] = wasSelected;
			}

			_lastComponentCount = allComponents.Length;
		}

		private void RemoveDuplicateComponents()
		{
			if (_targetGameObject == null || EditorUtility.IsPersistent(_targetGameObject)) return;

			IEnumerable<IGrouping<Type, Component>> componentGroups = _targetGameObject.GetComponents<Component>()
				.Where(c => c != null
				)
				.GroupBy(c => c.GetType()
				)
				.Where(g => g.Count() > 1
				);

			var removedCount = 0;
			foreach (var group in componentGroups)
			{
				var componentsToRemove = group.Skip(
					1
				).ToArray();

				foreach (var component in componentsToRemove)
				{
					if (CanRemoveComponent(
							component
						))
					{
						Undo.DestroyObjectImmediate(
							component
						);

						removedCount++;
					}
				}
			}

			EditorUtility.SetDirty(
				_targetGameObject
			);

			if (removedCount > 0)
				Debug.Log(
					$"Removed {removedCount} duplicate components from {_targetGameObject.name}"
				);
			else
				Debug.Log(
					"No duplicate components found to remove."
				);
		}
	}
}
