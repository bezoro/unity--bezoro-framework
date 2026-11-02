using System;
using Bezoro.Logging;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Editor.EditorWindows;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace Bezoro.Unity.Core.Editor.Overlays
{
	/// <summary>
	///     SceneView overlay for the small set of game settings that are frequently
	///     changed while testing scenes. It edits the GameSettings asset with undo
	///     support and keeps the dedicated settings window in sync.
	/// </summary>
	[Overlay(typeof(UnityEditor.SceneView), "Game Settings")]
	public class GameSettingsOverlay : Overlay
	{
		private const int    ITEM_SPACING = 5;
		private const int    ROOT_PADDING = 10;
		private const string UNDO_GROUP   = "Game Settings Changed";
		private       bool   _isSubscribed;

		private GameSettings? _settings;

		/// <summary>
		///     Creates the overlay root immediately and fills it on the next editor
		///     delay callback after the GameSettings singleton can be resolved.
		/// </summary>
		public override VisualElement CreatePanelContent()
		{
			var root = CreateRoot();

			EditorApplication.delayCall += () =>
			{
				if (!TryInitialize())
					return;

				BuildContent(root);
			};

			return root;
		}

		/// <summary>
		///     Cleans up the static GameSettings change subscription when Unity
		///     destroys the overlay instance.
		/// </summary>
		public override void OnWillBeDestroyed()
		{
			UnsubscribeFromSettingsChanged();
			base.OnWillBeDestroyed();
		}

		private static Button CreateButton(string text, Action onClick) =>
			new(onClick)
			{
				text  = text,
				style = { marginBottom = ITEM_SPACING }
			};

		private static Button CreateCloseSettingsButton() =>
			CreateButton(
				"Close Settings",
				() => GameSettingsWindow.Instance?.Close()
			);

		private static Button CreateOpenSettingsButton() =>
			CreateButton(
				"Open Settings",
				() =>
				{
					if (GameSettingsWindow.Instance != null)
					{
						GameSettingsWindow.Instance.Focus();
						return;
					}

					GameSettingsWindow.Show_Window();
				}
			);

		private static Toggle CreateToggle(string label, bool value, EventCallback<ChangeEvent<bool>> callback)
		{
			var toggle = new Toggle(label)
			{
				value = value,
				style = { marginBottom = ITEM_SPACING }
			};

			toggle.RegisterValueChangedCallback(callback);
			return toggle;
		}

		private static VisualElement CreateRoot() =>
			new()
			{
				style =
				{
					paddingTop    = ROOT_PADDING,
					paddingBottom = ROOT_PADDING,
					paddingLeft   = ROOT_PADDING,
					paddingRight  = ROOT_PADDING
				}
			};

		private static void OnShouldLogToggleChanged(ChangeEvent<bool> evt) =>
			LoggerSettings.Enabled = evt.newValue;

		private static void RefreshSettingsWindow()
		{
			var window = GameSettingsWindow.Instance;
			window?.Load_Settings();
			window?.Repaint();
		}

		private bool TryInitialize()
		{
			_settings = GameSettings.Instance;
			if (_settings == null)
			{
				Logger.LogWarning("GameSettings is null.");
				return false;
			}

			SubscribeToSettingsChanged();
			return true;
		}

		private void BuildContent(VisualElement root)
		{
			if (_settings == null)
				return;

			root.Clear();
			root.Add(CreateOpenSettingsButton());
			root.Add(CreateCloseSettingsButton());
			root.Add(CreateToggle("Emulate Build", _settings.EmulateBuild, OnEmulateBuildToggleChanged));
			root.Add(CreateToggle("Should Log",    LoggerSettings.Enabled, OnShouldLogToggleChanged));
		}

		private void OnAnySettingsChanged()
		{
			if (_settings == null)
				return;

			EditorUtility.SetDirty(_settings);
			EditorApplication.delayCall += SaveSettingsIfDirty;
		}

		private void OnEmulateBuildToggleChanged(ChangeEvent<bool> evt)
		{
			if (_settings == null)
				return;

			Undo.RecordObject(_settings, UNDO_GROUP);
			_settings.EmulateBuild = evt.newValue;
			SaveSettingsIfDirty();
			RefreshSettingsWindow();
		}

		private void SaveSettingsIfDirty()
		{
			if (_settings == null)
				return;

			EditorUtility.SetDirty(_settings);
			AssetDatabase.SaveAssetIfDirty(_settings);
		}

		private void SubscribeToSettingsChanged()
		{
			if (_isSubscribed)
				return;

			GameSettings.AnySettingsChanged += OnAnySettingsChanged;
			_isSubscribed                   =  true;
		}

		private void UnsubscribeFromSettingsChanged()
		{
			if (!_isSubscribed)
				return;

			GameSettings.AnySettingsChanged -= OnAnySettingsChanged;
			_isSubscribed                   =  false;
		}
	}
}
