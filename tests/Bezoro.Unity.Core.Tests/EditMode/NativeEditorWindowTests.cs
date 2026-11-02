using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bezoro.Unity.Core.Editor.EditorWindows;
using Bezoro.Unity.Core.Editor.Tools;
using Bezoro.Unity.Core.Scenes;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Exercises the native hosts and IMGUI lifecycle of the framework tool windows.
	/// </summary>
	[TestFixture]
	[Category("Integration")]
	public sealed class NativeEditorWindowTests
	{
		private readonly List<EditorWindow> _ownedWindows = new();
		private EditorWindow? _originalFocus;
		private UnityEngine.Object[] _originalSelection = Array.Empty<UnityEngine.Object>();
		private Scene _previewScene;
		private string? _registeredGuid;

		[SetUp]
		public void SetUp()
		{
			_originalFocus = EditorWindow.focusedWindow;
			_originalSelection = Selection.objects;
		}

		[TearDown]
		public void TearDown()
		{
			try
			{
				for (var i = _ownedWindows.Count - 1; i >= 0; i--)
				{
					var window = _ownedWindows[i];
					if (window != null)
						window.Close();
				}
			}
			finally
			{
				_ownedWindows.Clear();
				if (_registeredGuid != null)
				{
					LogAssert.Expect(LogType.Log, new Regex(Regex.Escape($"Unregistered GUID: <b>{_registeredGuid}</b>")));
					SceneObjectRegistry.Unregister(_registeredGuid);
					_registeredGuid = null;
				}
				if (_previewScene.IsValid())
					EditorSceneManager.ClosePreviewScene(_previewScene);
				_previewScene = default;
				Selection.objects = _originalSelection;
				if (_originalFocus != null)
					_originalFocus.Focus();
			}
		}

		[UnityTest]
		[Category("Boundary")]
		public IEnumerator ArrayHelpersConfigWindow_WithNoConfiguration_RepaintsAndClosesWithoutErrors()
		{
			var window = CreateOwnedWindow<ArrayHelpersConfigWindow>();
			window.config = null;
			yield return ShowRepaintAndClose(window);
		}

		[UnityTest]
		[Category("Boundary")]
		public IEnumerator SceneObjectRegistryWindow_WithoutNewRegistrations_RepaintsAndClosesWithoutErrors()
		{
			yield return ShowRepaintAndClose(CreateOwnedWindow<SceneObjectRegistryWindow>());
		}

		[UnityTest]
		[Category("Boundary")]
		public IEnumerator PrefabVariantToolsWindow_WithNoPrefabs_RepaintsAndClosesWithoutErrors()
		{
			var window = CreateOwnedWindow<PrefabVariantToolsWindow>();
			window.originalPrefab = null!;
			window.basePrefab = null!;
			yield return ShowRepaintAndClose(window);
		}

		[UnityTest]
		[Category("Boundary")]
		public IEnumerator PrefabVariantRetargetTool_WithNoPrefabs_RepaintsAndClosesWithoutErrors()
		{
			var window = CreateOwnedWindow<PrefabVariantRetargetTool>();
			window.variant = null!;
			window.target = null!;
			yield return ShowRepaintAndClose(window);
		}

		[UnityTest]
		[Category("HappyPath")]
		public IEnumerator SceneObjectRegistryWindow_WithRegisteredSceneObject_RepaintsWithoutChangingRegistration()
		{
			_previewScene = EditorSceneManager.NewPreviewScene();
			var registeredObject = new GameObject("NativeEditorWindowTests_RegisteredObject")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(registeredObject, _previewScene);
			_registeredGuid = Guid.NewGuid().ToString();
			LogAssert.Expect(LogType.Log, new Regex(Regex.Escape($"Registered Object: <b>{registeredObject.name}</b> with GUID: <b>{_registeredGuid}</b>")));
			SceneObjectRegistry.Register(registeredObject, _registeredGuid);
			Assert.That(SceneObjectRegistry.GetObjectByGuid(_registeredGuid), Is.SameAs(registeredObject));

			yield return ShowRepaintAndClose(CreateOwnedWindow<SceneObjectRegistryWindow>());

			Assert.That(SceneObjectRegistry.GetObjectByGuid(_registeredGuid), Is.SameAs(registeredObject),
				"Inspecting and closing the debugger must preserve the live registry entry.");
			LogAssert.NoUnexpectedReceived();
		}

		private T CreateOwnedWindow<T>() where T : EditorWindow
		{
			var window = ScriptableObject.CreateInstance<T>();
			_ownedWindows.Add(window);
			window.titleContent = new GUIContent(typeof(T).Name);
			window.position = new Rect(100, 100, 600, 400);
			return window;
		}

		private static IEnumerator ShowRepaintAndClose(EditorWindow window)
		{
			window.Show();
			window.Focus();
			var deadline = EditorApplication.timeSinceStartup + 10;
			var settledUpdates = 0;
			while (settledUpdates < 3 && EditorApplication.timeSinceStartup < deadline)
			{
				window.Repaint();
				EditorApplication.QueuePlayerLoopUpdate();
				yield return null;
				settledUpdates = window.rootVisualElement.panel != null ? settledUpdates + 1 : 0;
			}

			Assert.That(settledUpdates, Is.EqualTo(3), "The shown window must attach to a native panel before closing.");
			LogAssert.NoUnexpectedReceived();
			window.Close();
			yield return null;
			Assert.That(window == null, Is.True, "Closing the test-owned native host must destroy its EditorWindow.");
			LogAssert.NoUnexpectedReceived();
		}
	}
}
