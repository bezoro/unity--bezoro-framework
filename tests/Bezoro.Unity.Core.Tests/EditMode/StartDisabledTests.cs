using System;
using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.AI;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.Loading;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.ReflectionProbes;
using Bezoro.Unity.Core.StateMachines.Utilities;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies StartDisabled toggles configured objects and ignores missing entries.
	/// </summary>
	[TestFixture]
	public sealed class StartDisabledTests
	{
		private GameObject _host;
		private GameObject _target;

		[TearDown]
		public void TearDown()
		{
			if (_target != null)
				Object.DestroyImmediate(_target);

			if (_host != null)
				Object.DestroyImmediate(_host);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AIAgentModel_ShouldRegister_ReturnsOnlyWhenManagerExists(bool hasManager)
		{
			var model = new AIAgentModel();

			Assert.AreEqual(hasManager, model.ShouldRegister(hasManager));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AIAgentModel_ShouldRemove_ReturnsOnlyWhenManagerExists(bool hasManager)
		{
			var model = new AIAgentModel();

			Assert.AreEqual(hasManager, model.ShouldRemove(hasManager));
		}

		[Test]
		public void AITickManagerModel_AddUnique_AddsAgentOnce()
		{
			var model = new AITickManagerModel<object>(2);
			var agent = new object();

			Assert.IsTrue(model.AddUnique(agent));
			Assert.IsFalse(model.AddUnique(agent));

			Assert.AreEqual(1, model.Agents.Count);
			Assert.AreSame(agent, model.Agents[0]);
		}

		[Test]
		public void AITickManagerModel_Remove_ReturnsWhetherAgentWasRegistered()
		{
			var model = new AITickManagerModel<object>(2);
			var agent = new object();

			Assert.IsFalse(model.Remove(agent));

			model.AddUnique(agent);

			Assert.IsTrue(model.Remove(agent));
			Assert.AreEqual(0, model.Agents.Count);
		}

		[TestCase(false, false, false, AlphaRaycastSetupAction.None,                    0f)]
		[TestCase(true,  false, false, AlphaRaycastSetupAction.ApplyThreshold,          0.1f)]
		[TestCase(true,  true,  true,  AlphaRaycastSetupAction.ApplyThreshold,          0.1f)]
		[TestCase(true,  true,  false, AlphaRaycastSetupAction.ReportUnreadableTexture, 0f)]
		public void AlphaRaycastEnablerModel_DecideSetup_ReturnsThresholdOrWarningDecision(
			bool                    hasImage,
			bool                    hasTexture,
			bool                    isTextureReadable,
			AlphaRaycastSetupAction expectedAction,
			float                   expectedThreshold)
		{
			var model = new AlphaRaycastEnablerModel();

			var decision = model.DecideSetup(hasImage, hasTexture, isTextureReadable);

			Assert.AreEqual(expectedAction,    decision.Action);
			Assert.AreEqual(expectedThreshold, decision.Threshold);
		}

		[TestCase(false, true)]
		[TestCase(true,  false)]
		public void AsyncGameObjectModel_EditorMaintenance_RunsOnlyOutsidePlayMode(
			bool isPlaying,
			bool expected)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(expected, model.ShouldRebuildCaches(isPlaying));
			Assert.AreEqual(expected, model.ShouldProcessUndoModifications(isPlaying));
		}

		[Test]
		public void AsyncGameObjectModel_GetInitialLoadedState_ReturnsUnloaded()
		{
			var model = new AsyncGameObjectModel();

			Assert.IsFalse(model.GetInitialLoadedState());
		}

		[TestCase(true,  "Board",   "Board")]
		[TestCase(false, "Ignored", "<Destroyed GameObject>")]
		public void AsyncGameObjectModel_GetLoadingName_ReturnsObjectNameOrDestroyedFallback(
			bool   objectExists,
			string gameObjectName,
			string expected)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(expected, model.GetLoadingName(objectExists, gameObjectName));
		}

		[TestCase(false, true,  true)]
		[TestCase(true,  true,  false)]
		[TestCase(false, false, false)]
		public void AsyncGameObjectModel_ShouldEnableHierarchyCache_RequiresActiveTokenAndCache(
			bool isCancellationRequested,
			bool hasHierarchyCache,
			bool expected)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(
				expected,
				model.ShouldEnableHierarchyCache(isCancellationRequested, hasHierarchyCache)
			);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncGameObjectModel_ShouldRegisterWithManager_ReturnsOnlyWhenManagerExists(bool hasManager)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(hasManager, model.ShouldRegisterWithManager(hasManager));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncGameObjectModel_ShouldRemoveFromManager_ReturnsOnlyWhenManagerIsAvailable(
			bool managerIsAvailable)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(managerIsAvailable, model.ShouldRemoveFromManager(managerIsAvailable));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncGameObjectModel_ShouldRunAwake_ReturnsOnlyWhenEnabled(bool isEnabled)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(isEnabled, model.ShouldRunAwake(isEnabled));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncGameObjectModel_ShouldTrackUndoModification_ReturnsOnlyWhenAsyncGameObjectExists(
			bool hasAsyncGameObject)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(hasAsyncGameObject, model.ShouldTrackUndoModification(hasAsyncGameObject));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncGameObjectModel_ShouldUseHierarchyCache_ReturnsOnlyWhenCacheExists(bool hasHierarchyCache)
		{
			var model = new AsyncGameObjectModel();

			Assert.AreEqual(hasHierarchyCache, model.ShouldUseHierarchyCache(hasHierarchyCache));
		}

		[Test]
		public void AsyncLoadingManagerModel_GetHighestPriority_ReturnsHighestPriorityItem()
		{
			var model          = new AsyncLoadingManagerModel<TestLoadItem>(item => item.Priority);
			var lowPriority    = new TestLoadItem(1);
			var highPriority   = new TestLoadItem(5);
			var middlePriority = new TestLoadItem(3);

			model.Register(lowPriority);
			model.Register(highPriority);
			model.Register(middlePriority);

			Assert.AreSame(highPriority, model.GetHighestPriority());
		}

		[TestCase(3, 1, 1, 3)]
		[TestCase(1, 2, 4, 6)]
		public void AsyncLoadingManagerModel_GetTrackedTotalItemsToLoad_KeepsHighestKnownTotal(
			int currentTotalItemsToLoad,
			int itemsLoaded,
			int queuedItems,
			int expectedTotalItemsToLoad)
		{
			var model = new AsyncLoadingManagerModel<TestLoadItem>(item => item.Priority);

			for (var i = 0; i < queuedItems; i++)
				model.Register(new(i));

			Assert.AreEqual(
				expectedTotalItemsToLoad,
				model.GetTrackedTotalItemsToLoad(currentTotalItemsToLoad, itemsLoaded)
			);
		}

		[Test]
		public void AsyncLoadingManagerModel_RegisterAndRemove_TracksQueueCount()
		{
			var model = new AsyncLoadingManagerModel<TestLoadItem>(item => item.Priority);
			var item  = new TestLoadItem(1);

			model.Register(item);

			Assert.AreEqual(1, model.QueueCount);
			Assert.IsTrue(model.Remove(item));
			Assert.AreEqual(0, model.QueueCount);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncLoadingManagerModel_ShouldRegister_ReturnsOnlyWhenItemExists(bool hasItem)
		{
			var model = new AsyncLoadingManagerModel<TestLoadItem>(item => item.Priority);

			Assert.AreEqual(hasItem, model.ShouldRegister(hasItem));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AsyncLoadingManagerModel_ShouldRemove_ReturnsOnlyWhenItemExists(bool hasItem)
		{
			var model = new AsyncLoadingManagerModel<TestLoadItem>(item => item.Priority);

			Assert.AreEqual(hasItem, model.ShouldRemove(hasItem));
		}

		[Test]
		public void AudioFaderModel_GetAudioStartTime_ReturnsClipRelativePosition()
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(12.5f, model.GetAudioStartTime(0.25f, 50f));
		}

		[Test]
		public void AudioFaderModel_GetFadeOutStartTime_ReturnsClipTimeBeforeFadeDuration()
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(8f, model.GetFadeOutStartTime(10f, 2f));
		}

		[Test]
		public void AudioFaderModel_GetFadeVolumes_ReturnsInterpolatedValues()
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(0.25f, model.GetFadeInVolume(0.5f, 2f, 1f));
			Assert.AreEqual(0.75f, model.GetFadeOutVolume(1f, 0.25f));
		}

		[Test]
		public void AudioFaderModel_GetNormalizedFadeProgress_ReturnsElapsedOverDuration()
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(0.25f, model.GetNormalizedFadeProgress(0.5f, 2f));
		}

		[TestCase(true,  true,  true)]
		[TestCase(true,  false, false)]
		[TestCase(false, true,  false)]
		public void AudioFaderModel_ShouldStartFadeIn_RequiresFlagAndAudioSource(
			bool fadeIn,
			bool hasAudioSource,
			bool expected)
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(expected, model.ShouldStartFadeIn(fadeIn, hasAudioSource));
		}

		[TestCase(true,  true,  true,  true)]
		[TestCase(true,  true,  false, false)]
		[TestCase(true,  false, true,  false)]
		[TestCase(false, true,  true,  false)]
		public void AudioFaderModel_ShouldTrackFadeOut_RequiresFlagAudioSourceAndClip(
			bool fadeOut,
			bool hasAudioSource,
			bool hasClip,
			bool expected)
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(expected, model.ShouldTrackFadeOut(fadeOut, hasAudioSource, hasClip));
		}

		[TestCase(true,  false, false, true)]
		[TestCase(true,  true,  false, false)]
		[TestCase(true,  false, true,  false)]
		[TestCase(false, false, false, false)]
		public void AudioFaderModel_ShouldTriggerFadeOut_RequiresActiveCancellationSourceAndNoRunningFade(
			bool hasCancellationSource,
			bool isCancellationRequested,
			bool isFadingOut,
			bool expected)
		{
			var model = new AudioFaderModel();

			Assert.AreEqual(
				expected,
				model.ShouldTriggerFadeOut(hasCancellationSource, isCancellationRequested, isFadingOut)
			);
		}

		[Test]
		public void AudioManagerModel_GetAudioSourceDefaults_ReturnsBGMConfiguration()
		{
			var model = new AudioManagerModel();

			Assert.IsTrue(model.GetBGMLoop());
			Assert.IsFalse(model.GetBGMPlayOnAwake());
			Assert.AreEqual(0.5f, model.GetConfiguredBGMVolume(1f, 0.5f));
		}

		[TestCase(-1f,  0f)]
		[TestCase(0.5f, 0.5f)]
		[TestCase(2f,   1f)]
		public void AudioManagerModel_GetClampedVolume_ClampsToUnitRange(float volume, float expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.GetClampedVolume(volume));
		}

		[Test]
		public void AudioManagerModel_GetFadeValues_ReturnsHalfDurationAndInterpolatedVolume()
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(2f,   model.GetHalfFadeDuration(4f));
			Assert.AreEqual(0.5f, model.GetFadeVolume(0f, 1f, 1f, 2f));
		}

		[TestCase("Master", "Master", true)]
		[TestCase("Music",  "Master", false)]
		public void AudioManagerModel_IsMasterMixerGroup_MatchesByName(
			string groupName,
			string masterGroupName,
			bool   expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.IsMasterMixerGroup(groupName, masterGroupName));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AudioManagerModel_ShouldAssignMixerGroup_ReturnsOnlyWhenGroupExists(bool hasMixerGroup)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(hasMixerGroup, model.ShouldAssignMixerGroup(hasMixerGroup));
		}

		[TestCase(0.25f, 0.5f, true)]
		[TestCase(0.5f,  0.5f, false)]
		public void AudioManagerModel_ShouldContinueFade_ReturnsWhileElapsedIsBelowDuration(
			float elapsed,
			float duration,
			bool  expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.ShouldContinueFade(elapsed, duration));
		}

		[TestCase(0f, false)]
		[TestCase(1f, true)]
		public void AudioManagerModel_ShouldFade_ReturnsOnlyForPositiveDuration(float duration, bool expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.ShouldFade(duration));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AudioManagerModel_ShouldPlayBGM_ReturnsOnlyWhenSourceExists(bool hasAudioSource)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(hasAudioSource, model.ShouldPlayBGM(hasAudioSource));
		}

		[TestCase(true,  true,  true)]
		[TestCase(true,  false, false)]
		[TestCase(false, true,  false)]
		public void AudioManagerModel_ShouldPlaySFX_RequiresSourceAndClip(
			bool hasAudioSource,
			bool hasClip,
			bool expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.ShouldPlaySFX(hasAudioSource, hasClip));
		}

		[TestCase(true,  true,  true)]
		[TestCase(true,  false, false)]
		[TestCase(false, true,  false)]
		public void AudioManagerModel_ShouldSkipBGM_ReturnsOnlyForSamePlayingClip(
			bool isSameClip,
			bool isPlaying,
			bool expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.ShouldSkipBGM(isSameClip, isPlaying));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AudioManagerModel_ShouldStopBGM_ReturnsOnlyWhenSourceExists(bool hasAudioSource)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(hasAudioSource, model.ShouldStopBGM(hasAudioSource));
		}

		[TestCase(0, false)]
		[TestCase(1, true)]
		public void AudioManagerModel_ShouldUseFallbackMixerGroup_ReturnsOnlyWhenAnyGroupExists(
			int  groupCount,
			bool expected)
		{
			var model = new AudioManagerModel();

			Assert.AreEqual(expected, model.ShouldUseFallbackMixerGroup(groupCount));
		}

		[Test]
		public void BootSceneLoaderModel_DecidePathsToLoad_WhenCollectionHasPaths_UsesCollection()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(true, new[] { "Assets/Main.unity" }, 0, 2);

			Assert.AreEqual(BootSceneLoaderPathStatus.UseSceneCollection, decision.Status);
			CollectionAssert.AreEqual(new[] { "Assets/Main.unity" }, decision.Paths);
			Assert.IsFalse(decision.NextBuildIndex.HasValue);
		}

		[Test]
		public void BootSceneLoaderModel_DecidePathsToLoad_WhenCollectionIsEmpty_ReturnsEmptyCollectionStatus()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(true, Array.Empty<string>(), 0, 2);

			Assert.AreEqual(BootSceneLoaderPathStatus.EmptySceneCollection, decision.Status);
			Assert.IsNull(decision.Paths);
			Assert.IsFalse(decision.NextBuildIndex.HasValue);
		}

		[Test]
		public void BootSceneLoaderModel_DecidePathsToLoad_WhenNoCollection_UsesNextBuildIndex()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(false, null, 1, 3);

			Assert.AreEqual(BootSceneLoaderPathStatus.UseNextBuildScene, decision.Status);
			Assert.AreEqual(2,                                           decision.NextBuildIndex);
			Assert.IsNull(decision.Paths);
		}

		[Test]
		public void BootSceneLoaderModel_DecidePathsToLoad_WhenNoNextBuildScene_ReturnsNoNextSceneStatus()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(false, null, 1, 2);

			Assert.AreEqual(BootSceneLoaderPathStatus.NoNextBuildScene, decision.Status);
			Assert.IsNull(decision.Paths);
			Assert.IsFalse(decision.NextBuildIndex.HasValue);
		}

		[Test]
		public void CameraManagerModel_FindFirst_ReturnsFirstPredicateMatch()
		{
			var model  = new CameraManagerModel<object>(3);
			var first  = new object();
			var second = new object();

			model.Items[0] = first;
			model.Items[1] = second;

			Assert.AreSame(second, model.FindFirst(item => ReferenceEquals(item, second)));
		}

		[Test]
		public void CameraManagerModel_RegisterMain_UsesPrimarySlot()
		{
			var model  = new CameraManagerModel<object>(4);
			var camera = new object();

			model.RegisterMain(camera);

			Assert.AreSame(camera, model.Items[0]);
			Assert.IsTrue(model.Contains(camera));
		}

		[Test]
		public void CameraManagerModel_RegisterSecondary_ReturnsFalseWhenNoSlotIsAvailable()
		{
			var model = new CameraManagerModel<object>(2);

			model.Items[1] = new();

			Assert.IsFalse(model.RegisterSecondary(new(), item => item == null));
		}

		[Test]
		public void CameraManagerModel_RegisterSecondary_UsesFirstAvailableSecondarySlot()
		{
			var model    = new CameraManagerModel<object>(4);
			var occupied = new object();
			var camera   = new object();

			model.Items[1] = occupied;

			Assert.IsTrue(model.RegisterSecondary(camera, item => item == null));
			Assert.AreSame(occupied, model.Items[1]);
			Assert.AreSame(camera,   model.Items[2]);
		}

		[Test]
		public void CameraManagerModel_RemoveWhere_ClearsMatchingSlots()
		{
			var model  = new CameraManagerModel<object>(3);
			var keep   = new object();
			var remove = new object();

			model.Items[0] = keep;
			model.Items[1] = remove;

			Assert.AreEqual(1, model.RemoveWhere(item => ReferenceEquals(item, remove)));
			Assert.AreSame(keep, model.Items[0]);
			Assert.IsNull(model.Items[1]);
		}

		[TestCase(true,  false, true)]
		[TestCase(true,  true,  false)]
		[TestCase(false, false, false)]
		public void CameraManagerModel_ShouldRegister_RequiresCameraAndNoDuplicate(
			bool hasCamera,
			bool isAlreadyRegistered,
			bool expected)
		{
			var model = new CameraManagerModel<object>(4);

			Assert.AreEqual(expected, model.ShouldRegister(hasCamera, isAlreadyRegistered));
		}

		[Test]
		public void CameraManagerModel_Unregister_ClearsRegisteredCamera()
		{
			var model  = new CameraManagerModel<object>(2);
			var camera = new object();

			model.RegisterMain(camera);

			Assert.IsTrue(model.Unregister(camera));
			Assert.IsNull(model.Items[0]);
			Assert.IsFalse(model.Unregister(camera));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CameraRegistrationModel_ShouldRegister_ReturnsOnlyWhenCameraExists(bool hasCamera)
		{
			var model = new CameraRegistrationModel();

			Assert.AreEqual(hasCamera, model.ShouldRegister(hasCamera));
		}

		[TestCase(false, true,  true,  false, false)]
		[TestCase(true,  false, true,  false, false)]
		[TestCase(true,  true,  true,  true,  true)]
		[TestCase(true,  true,  false, true,  false)]
		public void CanvasCameraProviderModel_DecideAssignment_ReturnsAssignmentPlan(
			bool hasRegisteredCamera,
			bool hasCanvas,
			bool registeredCameraIsMainCamera,
			bool expectedShouldAssign,
			bool expectedUseRegisteredCamera)
		{
			var model = new CanvasCameraProviderModel();

			var decision = model.DecideAssignment(
				hasRegisteredCamera,
				hasCanvas,
				registeredCameraIsMainCamera
			);

			Assert.AreEqual(expectedShouldAssign,        decision.ShouldAssign);
			Assert.AreEqual(expectedUseRegisteredCamera, decision.UseRegisteredCamera);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CanvasCameraProviderModel_ShouldApplyResolvedCamera_ReturnsOnlyWhenCameraExists(bool hasCamera)
		{
			var model = new CanvasCameraProviderModel();

			Assert.AreEqual(hasCamera, model.ShouldApplyResolvedCamera(hasCamera));
		}

		[Test]
		public void DisableAll_DeactivatesConfiguredGameObjects()
		{
			var startDisabled = CreateStartDisabled();

			startDisabled.DisableAll();

			Assert.IsFalse(_target.activeSelf);
		}

		[Test]
		public void EnableAll_ActivatesConfiguredGameObjects()
		{
			var startDisabled = CreateStartDisabled();
			_target.SetActive(false);

			startDisabled.EnableAll();

			Assert.IsTrue(_target.activeSelf);
		}

		[Test]
		public void GameObjects_ReturnsConfiguredCollection()
		{
			var startDisabled = CreateStartDisabled();

			CollectionAssert.Contains(startDisabled.GameObjects, _target);
		}

		[Test]
		public void InstantiationManagerModel_DecideEnqueue_ReturnsCancelOrDisposedBeforeEnqueue()
		{
			var model = new InstantiationManagerModel<object>();

			Assert.AreEqual(InstantiationManagerEnqueueDecision.Cancel, model.DecideEnqueue(true));

			model.MarkDisposed();

			Assert.AreEqual(InstantiationManagerEnqueueDecision.FailDisposed, model.DecideEnqueue(false));
			Assert.AreEqual(InstantiationManagerEnqueueDecision.FailDisposed, model.DecideEnqueue(true));
		}

		[Test]
		public void InstantiationManagerModel_DecideFinalQueueAction_PrioritizesCancellationThenDisposal()
		{
			var model = new InstantiationManagerModel<object>();

			Assert.AreEqual(InstantiationManagerFinalQueueAction.None,   model.DecideFinalQueueAction(false));
			Assert.AreEqual(InstantiationManagerFinalQueueAction.Cancel, model.DecideFinalQueueAction(true));

			model.MarkDisposed();

			Assert.AreEqual(InstantiationManagerFinalQueueAction.FailDisposed, model.DecideFinalQueueAction(false));
			Assert.AreEqual(InstantiationManagerFinalQueueAction.Cancel,       model.DecideFinalQueueAction(true));
		}

		[TestCase(0, 0, 1f)]
		[TestCase(1, 4, 0.25f)]
		public void InstantiationManagerModel_GetBatchProgress_ReturnsProcessedRatio(
			int   processedCount,
			int   batchCount,
			float expected)
		{
			var model = new InstantiationManagerModel<object>();

			Assert.AreEqual(expected, model.GetBatchProgress(processedCount, batchCount));
		}

		[Test]
		public void InstantiationManagerModel_ShouldContinueProcessing_RequiresQueueAndActiveState()
		{
			var model = new InstantiationManagerModel<object>();

			Assert.IsFalse(model.ShouldContinueProcessing(false));

			model.Enqueue(new(), 0);

			Assert.IsTrue(model.ShouldContinueProcessing(false));
			Assert.IsFalse(model.ShouldContinueProcessing(true));

			model.MarkDisposed();

			Assert.IsFalse(model.ShouldContinueProcessing(false));
		}

		[TestCase(1, 0d,  16f, 5, false, true)]
		[TestCase(0, 0d,  16f, 5, false, false)]
		[TestCase(1, 16d, 16f, 5, false, false)]
		[TestCase(5, 0d,  16f, 5, false, false)]
		[TestCase(1, 0d,  16f, 5, true,  false)]
		public void InstantiationManagerModel_ShouldProcessNext_RequiresQueuedWorkWithinFrameBudget(
			int    processedCount,
			double elapsedMilliseconds,
			float  frameBudgetMilliseconds,
			int    maxInstantiationsPerFrame,
			bool   isCancellationRequested,
			bool   expected)
		{
			var model = new InstantiationManagerModel<object>();

			if (processedCount > 0)
				model.Enqueue(new(), 0);

			Assert.AreEqual(
				expected,
				model.ShouldProcessNext(
					processedCount,
					elapsedMilliseconds,
					frameBudgetMilliseconds,
					maxInstantiationsPerFrame,
					isCancellationRequested
				)
			);
		}

		[Test]
		public void InstantiationManagerModel_TryStartProcessing_StartsOnlyOnceUntilFinished()
		{
			var model = new InstantiationManagerModel<object>();

			Assert.IsTrue(model.TryStartProcessing());
			Assert.IsFalse(model.TryStartProcessing());

			model.FinishProcessing();

			Assert.IsTrue(model.TryStartProcessing());
		}

		[Test]
		public void InstantiationPriorityQueue_Dequeue_ReturnsLowestPriorityFirstAndPreservesPriorityOrder()
		{
			var queue = new InstantiationPriorityQueue<string>();

			queue.Enqueue("priority-10",  10);
			queue.Enqueue("priority-1-a", 1);
			queue.Enqueue("priority-1-b", 1);

			Assert.AreEqual("priority-1-a", queue.Dequeue());
			Assert.AreEqual("priority-1-b", queue.Dequeue());
			Assert.AreEqual("priority-10",  queue.Dequeue());
			Assert.AreEqual(0,              queue.Count);
		}

		[TestCase(true,  true,  false, LevelLoaderButtonLoadAction.LoadSceneCollection)]
		[TestCase(true,  false, true,  LevelLoaderButtonLoadAction.MissingSceneCollection)]
		[TestCase(false, true,  true,  LevelLoaderButtonLoadAction.LoadSceneReference)]
		[TestCase(false, true,  false, LevelLoaderButtonLoadAction.MissingSceneReference)]
		public void LevelLoaderButtonModel_DecideLoadAction_ReturnsConfiguredSourceDecision(
			bool                        useSceneCollection,
			bool                        hasSceneCollection,
			bool                        hasSceneReference,
			LevelLoaderButtonLoadAction expectedAction)
		{
			var model = new LevelLoaderButtonModel();

			var decision = model.DecideLoadAction(
				useSceneCollection,
				hasSceneCollection,
				hasSceneReference
			);

			Assert.AreEqual(expectedAction, decision.Action);
		}

		[TestCase(false, true)]
		[TestCase(true,  false)]
		public void LevelReloaderButtonModel_ShouldReload_OnlyWhenNotCancelled(
			bool isCancellationRequested,
			bool expected)
		{
			var model = new LevelReloaderButtonModel();

			Assert.AreEqual(expected, model.ShouldReload(isCancellationRequested));
		}

		[TestCase(-1f,   0f)]
		[TestCase(0.92f, 0.92f)]
		[TestCase(2f,    1f)]
		public void LoadingProgressServiceModel_GetClampedProgress_DoesNotApplyAutoCompleteThreshold(
			float rawProgress,
			float expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.GetClampedProgress(rawProgress));
		}

		[TestCase(0.5f, 0.75f, 0.75f)]
		[TestCase(0.8f, 0.25f, 0.8f)]
		public void LoadingProgressServiceModel_GetMonotonicProgress_NeverGoesBackward(
			float nextProgress,
			float currentProgress,
			float expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.GetMonotonicProgress(nextProgress, currentProgress));
		}

		[TestCase(0.009f, 0.01f)]
		[TestCase(2f,     2f)]
		public void LoadingProgressServiceModel_GetPositiveWeight_EnforcesMinimumWeight(
			float weight,
			float expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.GetPositiveWeight(weight));
		}

		[TestCase(-1f,   0f)]
		[TestCase(0.5f,  0.5f)]
		[TestCase(0.92f, 1f)]
		[TestCase(2f,    1f)]
		public void LoadingProgressServiceModel_GetTrackerProgress_ClampsAndAutoCompletesNearDone(
			float rawProgress,
			float expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.GetTrackerProgress(rawProgress));
		}

		[Test]
		public void LoadingProgressServiceModel_GetWeightedProgress_ReturnsWeightedAverage()
		{
			var model = new LoadingProgressServiceModel();

			float progress = model.GetWeightedProgress(
				new[]
				{
					new LoadingProgressContribution(0.25f, 1f),
					new LoadingProgressContribution(1f,    3f)
				}
			);

			Assert.AreEqual(0.8125f, progress);
		}

		[TestCase(false, true)]
		[TestCase(true,  false)]
		public void LoadingProgressServiceModel_ShouldAutoBeginOperation_ReturnsOnlyWhenIdle(
			bool isLoading,
			bool expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.ShouldAutoBeginOperation(isLoading));
		}

		[TestCase(0f,    false)]
		[TestCase(0.98f, false)]
		[TestCase(0.99f, true)]
		[TestCase(1f,    true)]
		public void LoadingProgressServiceModel_ShouldConclude_UsesCompletionThreshold(
			float progress,
			bool  expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.ShouldConclude(progress));
		}

		[TestCase(0, true)]
		[TestCase(1, false)]
		public void LoadingProgressServiceModel_ShouldCreateDefaultStage_ReturnsOnlyWhenEmpty(
			int  stageCount,
			bool expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.ShouldCreateDefaultStage(stageCount));
		}

		[TestCase(true,  0, true)]
		[TestCase(true,  1, false)]
		[TestCase(false, 0, false)]
		public void LoadingProgressServiceModel_ShouldForceCompleteForNoTrackers_RequiresLoadingAndNoStages(
			bool isLoading,
			int  stageCount,
			bool expected)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(expected, model.ShouldForceCompleteForNoTrackers(isLoading, stageCount));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void LoadingProgressServiceModel_ShouldUpdate_ReturnsOnlyWhileLoading(bool isLoading)
		{
			var model = new LoadingProgressServiceModel();

			Assert.AreEqual(isLoading, model.ShouldUpdate(isLoading));
		}

		[Test]
		public void LoadingScreenModel_GetInitialPresentation_ReturnsZeroProgress()
		{
			var model = new LoadingScreenModel();

			var presentation = model.GetInitialPresentation();

			Assert.AreEqual(0f,    presentation.FillAmount);
			Assert.AreEqual("0 %", presentation.PercentText);
		}

		[TestCase(0.25f, 0.5f, 0.5f,  "50 %")]
		[TestCase(0.75f, 0.5f, 0.75f, "75 %")]
		[TestCase(1.25f, 0.5f, 1f,    "100 %")]
		public void LoadingScreenModel_GetProgressPresentation_ClampsAndKeepsProgressMonotonic(
			float  requestedProgress,
			float  currentFillAmount,
			float  expectedFillAmount,
			string expectedPercentText)
		{
			var model = new LoadingScreenModel();

			var presentation = model.GetProgressPresentation(requestedProgress, currentFillAmount);

			Assert.AreEqual(expectedFillAmount,  presentation.FillAmount);
			Assert.AreEqual(expectedPercentText, presentation.PercentText);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void MainCameraModel_FadeImageDecisions_ReturnOnlyWhenFadeImageExists(bool hasFadeImage)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(hasFadeImage, model.ShouldActivateFadeImage(hasFadeImage));
			Assert.AreEqual(hasFadeImage, model.ShouldApplyFadeColor(hasFadeImage));
			Assert.AreEqual(hasFadeImage, model.ShouldFadeImage(hasFadeImage));
			Assert.AreEqual(hasFadeImage, model.ShouldResetFadeAlpha(hasFadeImage));
		}

		[Test]
		public void MainCameraModel_GetFadeInAlpha_ReturnsOpaqueAlpha()
		{
			var model = new MainCameraModel();

			Assert.AreEqual(1f, model.GetFadeInAlpha());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void MainCameraModel_ShouldActivateCanvas_ReturnsOnlyWhenCanvasGameObjectExists(bool hasCanvasGameObject)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(hasCanvasGameObject, model.ShouldActivateCanvas(hasCanvasGameObject));
		}

		[TestCase(true,  true,  true)]
		[TestCase(true,  false, false)]
		[TestCase(false, true,  false)]
		public void MainCameraModel_ShouldFadeOut_RequiresCanvasAndGameObject(
			bool hasCanvas,
			bool hasCanvasGameObject,
			bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldFadeOut(hasCanvas, hasCanvasGameObject));
		}

		[TestCase(false, false, false, false)]
		[TestCase(true,  false, true,  false)]
		[TestCase(true,  true,  true,  true)]
		public void PointerEventReceiverModel_DecideReceiverHit_ReturnsFirstReceiverOwnership(
			bool hasReceiver,
			bool isSelf,
			bool expectedHasReceiver,
			bool expectedIsPrimaryReceiver)
		{
			var model = new PointerEventReceiverModel();

			var decision = model.DecideReceiverHit(hasReceiver, isSelf);

			Assert.AreEqual(expectedHasReceiver,       decision.HasReceiver);
			Assert.AreEqual(expectedIsPrimaryReceiver, decision.IsPrimaryReceiver);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PointerEventReceiverModel_ShouldInspectCollider_ReturnsOnlyWhenColliderExists(bool hasCollider)
		{
			var model = new PointerEventReceiverModel();

			Assert.AreEqual(hasCollider, model.ShouldInspectCollider(hasCollider));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PointerEventReceiverModel_ShouldLog_ReturnsConfiguredValue(bool enableLogs)
		{
			var model = new PointerEventReceiverModel();

			Assert.AreEqual(enableLogs, model.ShouldLog(enableLogs));
		}

		[TestCase(false, true)]
		[TestCase(true,  false)]
		public void PointerEventReceiverModel_ShouldRegister_ReturnsOnlyWhenNotRegistered(
			bool isRegistered,
			bool expected)
		{
			var model = new PointerEventReceiverModel();

			Assert.AreEqual(expected, model.ShouldRegister(isRegistered));
		}

		[TestCase(false, false)]
		[TestCase(true,  true)]
		public void PointerEventReceiverModel_ShouldUnregister_ReturnsOnlyWhenRegistered(
			bool isRegistered,
			bool expected)
		{
			var model = new PointerEventReceiverModel();

			Assert.AreEqual(expected, model.ShouldUnregister(isRegistered));
		}

		[Test]
		public void PointerManagerModel_AddUniqueReceivers_AddsEachReceiverOnce()
		{
			var model    = new PointerManagerModel<object, object, object>(2);
			var receiver = new object();

			Assert.IsTrue(model.AddUniquePressReceiver(receiver));
			Assert.IsFalse(model.AddUniquePressReceiver(receiver));
			Assert.IsTrue(model.AddUniqueOverReceiver(receiver));
			Assert.IsFalse(model.AddUniqueOverReceiver(receiver));
			Assert.IsTrue(model.AddUniqueDragReceiver(receiver));
			Assert.IsFalse(model.AddUniqueDragReceiver(receiver));

			Assert.AreEqual(1, model.PressReceivers.Count);
			Assert.AreEqual(1, model.OverReceivers.Count);
			Assert.AreEqual(1, model.DragReceivers.Count);
		}

		[Test]
		public void PointerManagerModel_ClickDecisions_RequirePriorPress()
		{
			var model = new PointerManagerModel<object, object, object>(2);

			Assert.IsFalse(model.ShouldDispatchClick());
			Assert.IsFalse(model.ShouldNotifyClickMissed(0));

			model.NotifyPointerPressed();

			Assert.IsTrue(model.ShouldDispatchClick());
			Assert.IsTrue(model.ShouldNotifyClickMissed(0));
			Assert.IsFalse(model.ShouldNotifyClickMissed(1));

			model.NotifyPointerReleased();

			Assert.IsFalse(model.ShouldDispatchClick());
		}

		[Test]
		public void PointerManagerModel_RemoveReceivers_RemovesRegisteredReceivers()
		{
			var model    = new PointerManagerModel<object, object, object>(2);
			var receiver = new object();

			model.AddUniquePressReceiver(receiver);
			model.AddUniqueOverReceiver(receiver);
			model.AddUniqueDragReceiver(receiver);

			Assert.AreEqual(1, model.RemovePressReceiver(receiver));
			Assert.AreEqual(1, model.RemoveOverReceiver(receiver));
			Assert.AreEqual(1, model.RemoveDragReceiver(receiver));
			Assert.AreEqual(0, model.PressReceivers.Count);
			Assert.AreEqual(0, model.OverReceivers.Count);
			Assert.AreEqual(0, model.DragReceivers.Count);
		}

		[TestCase(true,  1f, 1, true)]
		[TestCase(false, 1f, 1, false)]
		[TestCase(true,  0f, 1, false)]
		[TestCase(true,  1f, 0, false)]
		public void PointerManagerModel_ShouldDispatchDrag_RequiresPressMovementAndDragReceiver(
			bool  isPressed,
			float deltaMagnitude,
			int   dragReceiverCount,
			bool  expected)
		{
			var model = new PointerManagerModel<object, object, object>(2);

			Assert.AreEqual(expected, model.ShouldDispatchDrag(isPressed, deltaMagnitude, dragReceiverCount));
		}

		[TestCase(0, false)]
		[TestCase(2, true)]
		public void PointerManagerModel_ShouldDispatchStay_RequiresCurrentHit(int currentHitCount, bool expected)
		{
			var model = new PointerManagerModel<object, object, object>(2);

			Assert.AreEqual(expected, model.ShouldDispatchStay(currentHitCount));
		}

		[TestCase(0, false)]
		[TestCase(1, true)]
		public void PointerManagerModel_ShouldProcessOverEvents_RequiresOverReceiver(int receiverCount, bool expected)
		{
			var model = new PointerManagerModel<object, object, object>(2);

			Assert.AreEqual(expected, model.ShouldProcessOverEvents(receiverCount));
		}

		[Test]
		public void ReflectionProbeManagerModel_AddUnique_AddsValidProbeOnce()
		{
			var model = new ReflectionProbeManagerModel<object>(2);
			var probe = new object();

			Assert.IsTrue(model.AddUnique(probe,  item => item != null));
			Assert.IsFalse(model.AddUnique(probe, item => item != null));

			Assert.AreEqual(1, model.ReflectionProbes.Count);
			Assert.AreSame(probe, model.ReflectionProbes[0]);
		}

		[Test]
		public void ReflectionProbeManagerModel_AddUnique_IgnoresInvalidProbe()
		{
			var model = new ReflectionProbeManagerModel<object>(2);

			Assert.IsFalse(model.AddUnique(null, item => item != null));
			Assert.AreEqual(0, model.ReflectionProbes.Count);
		}

		[Test]
		public void ReflectionProbeManagerModel_GetValidItemsAndRemoveInvalid_RemovesInvalidEntries()
		{
			var model   = new ReflectionProbeManagerModel<object>(3);
			var validA  = new object();
			var invalid = new object();
			var validB  = new object();

			model.AddUnique(validA,  item => item != null);
			model.AddUnique(invalid, item => item != null);
			model.AddUnique(validB,  item => item != null);

			var validItems = new List<object>(
				model.GetValidItemsAndRemoveInvalid(item => !ReferenceEquals(item, invalid))
			);

			CollectionAssert.AreEqual(new[] { validB, validA }, validItems);
			CollectionAssert.AreEqual(new[] { validA, validB }, model.ReflectionProbes);
		}

		[Test]
		public void ReflectionProbeManagerModel_Remove_ReturnsWhetherProbeWasRegistered()
		{
			var model = new ReflectionProbeManagerModel<object>(2);
			var probe = new object();

			Assert.IsFalse(model.Remove(probe));

			model.AddUnique(probe, item => item != null);

			Assert.IsTrue(model.Remove(probe));
			Assert.AreEqual(0, model.ReflectionProbes.Count);
		}

		[TestCase(true,  ReflectionProbeProviderEnableAction.Register)]
		[TestCase(false, ReflectionProbeProviderEnableAction.ReportMissingProbe)]
		public void ReflectionProbeProviderModel_DecideEnable_ReturnsRegisterOrWarningAction(
			bool                                hasReflectionProbe,
			ReflectionProbeProviderEnableAction expectedAction)
		{
			var model = new ReflectionProbeProviderModel();

			Assert.AreEqual(expectedAction, model.DecideEnable(hasReflectionProbe));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReflectionProbeProviderModel_ShouldUnregister_ReturnsOnlyWhenProbeExists(bool hasReflectionProbe)
		{
			var model = new ReflectionProbeProviderModel();

			Assert.AreEqual(hasReflectionProbe, model.ShouldUnregister(hasReflectionProbe));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SceneBGMConfigModel_ManualActions_RunOnlyWhileApplicationIsPlaying(bool isPlaying)
		{
			var model = new SceneBGMConfigModel();

			Assert.AreEqual(isPlaying, model.ShouldRunManualPlay(isPlaying));
			Assert.AreEqual(isPlaying, model.ShouldRunManualStop(isPlaying));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SceneBGMConfigModel_ShouldPlayOnStart_ReturnsConfiguredValue(bool playOnStart)
		{
			var model = new SceneBGMConfigModel();

			Assert.AreEqual(playOnStart, model.ShouldPlayOnStart(playOnStart));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StartDisabledModel_GetTargetActiveState_ReturnsRequestedState(bool enabled)
		{
			var model = new StartDisabledModel();

			Assert.AreEqual(enabled, model.GetTargetActiveState(enabled));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StartDisabledModel_ShouldApplyTarget_ReturnsOnlyWhenTargetExists(bool hasTarget)
		{
			var model = new StartDisabledModel();

			Assert.AreEqual(hasTarget, model.ShouldApplyTarget(hasTarget));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StateMachineOwnerModel_HasRequiredStateMachine_ReturnsStateMachinePresence(
			bool hasStateMachine)
		{
			var model = new StateMachineOwnerModel();

			Assert.AreEqual(hasStateMachine, model.HasRequiredStateMachine(hasStateMachine));
		}

		[TestCase(true,  false)]
		[TestCase(false, true)]
		public void StateMachineOwnerModel_ShouldDisableOwner_ReturnsOnlyWhenMissingRequiredStateMachine(
			bool hasRequiredStateMachine,
			bool expected)
		{
			var model = new StateMachineOwnerModel();

			Assert.AreEqual(expected, model.ShouldDisableOwner(hasRequiredStateMachine));
		}

		[TestCase(false, true)]
		[TestCase(true,  false)]
		public void StateMachineOwnerModel_ShouldRegisterTickable_ReturnsOnlyWhenNotAlreadyRegistered(
			bool isTickRegistered,
			bool expected)
		{
			var model = new StateMachineOwnerModel();

			Assert.AreEqual(expected, model.ShouldRegisterTickable(isTickRegistered));
		}

		[TestCase(false, false)]
		[TestCase(true,  true)]
		public void StateMachineOwnerModel_ShouldUnregisterTickable_ReturnsOnlyWhenRegistered(
			bool isTickRegistered,
			bool expected)
		{
			var model = new StateMachineOwnerModel();

			Assert.AreEqual(expected, model.ShouldUnregisterTickable(isTickRegistered));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StateMachineOwnerModel_StateMachineActions_RunOnlyWhenStateMachineExists(
			bool hasStateMachine)
		{
			var model = new StateMachineOwnerModel();

			Assert.AreEqual(hasStateMachine, model.ShouldRunStateMachineAction(hasStateMachine));
			Assert.AreEqual(hasStateMachine, model.ShouldFindState(hasStateMachine));
			Assert.AreEqual(hasStateMachine, model.ShouldReplaceExistingStateMachine(hasStateMachine));
		}

		[TestCase(true,  false, true)]
		[TestCase(true,  true,  false)]
		[TestCase(false, false, false)]
		public void UnloadSceneOnLoadingCompleteModel_ShouldStartUnload_RequiresActiveCancellationSource(
			bool hasCancellationSource,
			bool isCancellationRequested,
			bool expected)
		{
			var model = new UnloadSceneOnLoadingCompleteModel();

			Assert.AreEqual(expected, model.ShouldStartUnload(hasCancellationSource, isCancellationRequested));
		}

		private StartDisabled CreateStartDisabled()
		{
			_host   = new("StartDisabledTestsHost");
			_target = new("StartDisabledTestsTarget");
			var startDisabled = _host.AddComponent<StartDisabled>();

			typeof(StartDisabled).GetField(
				"gameObjects",
				BindingFlags.Instance | BindingFlags.NonPublic
			).SetValue(startDisabled, new List<GameObject> { _target, null });

			return startDisabled;
		}

		private sealed class TestLoadItem
		{
			public TestLoadItem(int priority)
			{
				Priority = priority;
			}

			public int Priority { get; }
		}
	}
}
