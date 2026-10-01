using System;
using System.Linq;
using System.Reflection;
using Bezoro.Unity.Core.Editor.Tools.CompilationSoundPlayer;
using Bezoro.Unity.Core.Editor.Utilities;
using NUnit.Framework;
using UnityEditor;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class EditorAudioFeedbackSettingsTests
	{
		[Test]
		public void EditorAudioFeedbackSettings_WhenEditorDomainLoads_RegistersInitializer()
		{
			var initializeMethods = typeof(EditorAudioFeedbackSettings)
				.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(method => method.GetCustomAttribute<InitializeOnLoadMethodAttribute>() != null)
				.ToArray();

			Assert.That(
				initializeMethods,
				Is.Not.Empty,
				$"{nameof(EditorAudioFeedbackSettings)} must initialize on editor load so compilation/build/test events are subscribed after domain reloads."
			);
		}

		[Test]
		public void EditorAudioFeedbackSettings_WhenRegisteredInProjectSettings_ProvidesProjectSettingsPage()
		{
			var providerType = typeof(EditorAudioFeedbackSettings).Assembly.GetType(
				"Bezoro.Unity.Core.Editor.Tools.CompilationSoundPlayer.EditorAudioFeedbackSettingsProvider"
			);

			Assert.That(
				providerType,
				Is.Not.Null,
				"Editor audio feedback should be configurable from Project Settings."
			);

			var providerMethod = providerType!
				.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.SingleOrDefault(method => method.GetCustomAttribute<SettingsProviderAttribute>() != null);

			Assert.That(
				providerMethod,
				Is.Not.Null,
				"Editor audio feedback should register a SettingsProvider."
			);

			var provider = (SettingsProvider)providerMethod!.Invoke(null, null)!;

			Assert.That(
				provider.settingsPath,
				Is.EqualTo("Project/Bezoro/Editor Audio Feedback")
			);
		}

		[Test]
		public void EditorAudioFeedbackSettings_WhenBuildStarts_ExposesConfigurableBuildStartAudio()
		{
			var serializedObject = new SerializedObject(
				EditorAudioFeedbackSettings.instance
			);

			Assert.That(
				serializedObject.FindProperty("buildStartedClip"),
				Is.Not.Null
			);
			Assert.That(
				serializedObject.FindProperty("buildStartedVolume"),
				Is.Not.Null
			);
		}

		[Test]
		public void EditorAudioFeedbackSettings_WhenInitialized_ListensForDelayedBuildCompletion()
		{
			var settings = EditorAudioFeedbackSettings.instance;

			InvokeStaticLifecycle(
				"InitializeAudioFeedback"
			);

			var buildCompletionSubscribers = GetStaticEventSubscriberTargets(
				typeof(EditorBuildMonitor),
				"BuildCompleted"
			);

			Assert.That(
				buildCompletionSubscribers,
				Has.Some.SameAs(settings),
				"Successful build audio should wait until Unity reports that the build pipeline has stopped before playing."
			);
		}

		[Test]
		public void EditorAudioFeedbackSettings_WhenVolumesAreNormalized_ClampsMasterAndEventVolumes()
		{
			var settings = EditorAudioFeedbackSettings.instance;
			var masterVolumeField = GetVolumeField("masterVolume");
			var compilationStartVolumeField = GetVolumeField("compilationStartVolume");
			var originalMasterVolume = (float)masterVolumeField.GetValue(settings);
			var originalStartVolume = (float)compilationStartVolumeField.GetValue(settings);

			try
			{
				masterVolumeField.SetValue(settings, 2f);
				compilationStartVolumeField.SetValue(settings, -0.5f);

				InvokeLifecycle(
					settings,
					"NormalizeVolumes"
				);

				Assert.That(
					(float)masterVolumeField.GetValue(settings),
					Is.EqualTo(1f)
				);
				Assert.That(
					(float)compilationStartVolumeField.GetValue(settings),
					Is.EqualTo(0f)
				);
			}
			finally
			{
				masterVolumeField.SetValue(settings, originalMasterVolume);
				compilationStartVolumeField.SetValue(settings, originalStartVolume);
			}
		}

		[Test]
		public void EditorAudioFeedbackSettings_WhenCalculatingEffectiveVolume_MultipliesMasterAndEventVolume()
		{
			var settings              = EditorAudioFeedbackSettings.instance;
			var masterVolumeField    = GetVolumeField("masterVolume");
			var originalMasterVolume = (float)masterVolumeField.GetValue(settings);

			try
			{
				masterVolumeField.SetValue(settings, 0.5f);

				var effectiveVolume = InvokeEffectiveVolume(
					settings,
					0.25f
				);

				Assert.That(
					effectiveVolume,
					Is.EqualTo(0.125f).Within(0.0001f)
				);
			}
			finally
			{
				masterVolumeField.SetValue(settings, originalMasterVolume);
			}
		}

		private static FieldInfo GetVolumeField(string fieldName)
		{
			var field = typeof(EditorAudioFeedbackSettings).GetField(
				fieldName,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
			);

			Assert.That(
				field,
				Is.Not.Null,
				$"{fieldName} should be serialized by {nameof(EditorAudioFeedbackSettings)}."
			);

			return field!;
		}

		private static float InvokeEffectiveVolume(EditorAudioFeedbackSettings settings, float eventVolume)
		{
			var method = typeof(EditorAudioFeedbackSettings).GetMethod(
				"GetEffectiveVolume",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
			);

			Assert.That(
				method,
				Is.Not.Null,
				$"{nameof(EditorAudioFeedbackSettings)} should calculate effective volume from master and event volume."
			);

			return (float)method!.Invoke(settings, new object[] { eventVolume })!;
		}

		private static object[] GetStaticEventSubscriberTargets(Type publisherType, string eventName)
		{
			var field = publisherType.GetField(
				eventName,
				BindingFlags.Static | BindingFlags.NonPublic
			);

			Assert.That(
				field,
				Is.Not.Null,
				$"{publisherType.Name}.{eventName} should be backed by a static event field."
			);

			var eventDelegate = field!.GetValue(null) as MulticastDelegate;
			return eventDelegate
				?.GetInvocationList()
				.Select(handler => handler.Target)
				.Where(target => target != null)
				.ToArray()!
				?? Array.Empty<object>();
		}

		private static void InvokeStaticLifecycle(string methodName) =>
			typeof(EditorAudioFeedbackSettings)
				.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
				.Invoke(null, null);

		private static void InvokeLifecycle(EditorAudioFeedbackSettings settings, string methodName) =>
			typeof(EditorAudioFeedbackSettings)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(settings, null);
	}
}
