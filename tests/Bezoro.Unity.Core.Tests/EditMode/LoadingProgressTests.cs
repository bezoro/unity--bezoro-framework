using System;
using Bezoro.Unity.Core.Loading;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies loading progress value clamping and weighted aggregation.
	/// </summary>
	[TestFixture]
	public sealed class LoadingProgressTests
	{
		private LoadingProgressService _service;
		private GameObject             _serviceObject;

		[SetUp]
		public void SetUp()
		{
			_serviceObject = new("LoadingProgressTestsService");
			_service       = _serviceObject.AddComponent<LoadingProgressService>();
			_service.BeginOperation("EditMode Test Loading");
		}

		[TearDown]
		public void TearDown()
		{
			if (_serviceObject != null)
				Object.DestroyImmediate(_serviceObject);
		}

		[Test]
		public void LoadingProgressServiceModel_GetMonotonicProgress_NeverMovesBackwards()
		{
			var model = new LoadingProgressServiceModel();

			float progress = model.GetMonotonicProgress(0.25f, 0.5f);

			Assert.AreEqual(0.5f, progress);
		}

		[TestCase(0.91f, 0.91f)]
		[TestCase(0.92f, 1f)]
		[TestCase(-1f,   0f)]
		[TestCase(2f,    1f)]
		public void LoadingProgressServiceModel_GetTrackerProgress_ClampsAndAutoCompletesNearDone(
			float rawProgress,
			float expectedProgress)
		{
			var model = new LoadingProgressServiceModel();

			float progress = model.GetTrackerProgress(rawProgress);

			Assert.AreEqual(expectedProgress, progress);
		}

		[Test]
		public void LoadingProgressServiceModel_GetWeightedProgress_UsesContributionWeights()
		{
			var model = new LoadingProgressServiceModel();
			var contributions = new[]
			{
				new LoadingProgressContribution(0.25f, 1f),
				new LoadingProgressContribution(0.75f, 3f)
			};

			float progress = model.GetWeightedProgress(contributions);

			Assert.AreEqual(0.625f, progress);
		}

		[Test]
		public void LoadingProgressServiceModel_GetWeightedProgress_WhenNoContributions_ReturnsZero()
		{
			var model = new LoadingProgressServiceModel();

			float progress = model.GetWeightedProgress(Array.Empty<LoadingProgressContribution>());

			Assert.AreEqual(0f, progress);
		}

		[TestCase(0.989f, false)]
		[TestCase(0.99f,  true)]
		[TestCase(1f,     true)]
		public void LoadingProgressServiceModel_ShouldConclude_UsesCompletionThreshold(
			float progress,
			bool  expected)
		{
			var model = new LoadingProgressServiceModel();

			bool shouldConclude = model.ShouldConclude(progress);

			Assert.AreEqual(expected, shouldConclude);
		}

		[TestCase(true,  0, true)]
		[TestCase(true,  1, false)]
		[TestCase(false, 0, false)]
		public void LoadingProgressServiceModel_ShouldForceCompleteForNoTrackers_ReturnsOnlyForActiveEmptyOperation(
			bool isLoading,
			int  stageCount,
			bool expected)
		{
			var model = new LoadingProgressServiceModel();

			bool shouldForceComplete = model.ShouldForceCompleteForNoTrackers(isLoading, stageCount);

			Assert.AreEqual(expected, shouldForceComplete);
		}

		[Test]
		public void LoadingStage_ProgressUsesTrackerWeights()
		{
			var stage  = _service.BeginStage("Stage");
			var first  = stage.CreateTracker("First");
			var second = stage.CreateTracker("Second", 3f);

			first.Progress  = 0.25f;
			second.Progress = 0.75f;

			Assert.AreEqual(0.625f, stage.Progress);
		}

		[Test]
		public void ProgressTracker_ClampsProgressAndWeight()
		{
			var stage   = _service.BeginStage("Stage");
			var tracker = stage.CreateTracker("Tracker", -1f);

			tracker.Progress = 2f;

			Assert.AreEqual(0.01f, tracker.Weight);
			Assert.AreEqual(1f,    tracker.Progress);
		}

		[Test]
		public void ProgressTracker_WhenBoundToVariable_UsesClampedGetterValue()
		{
			var value   = 2f;
			var tracker = _service.CreateTracker("Bound");

			tracker.BindToVariable(() => value);

			Assert.AreEqual(1f, tracker.Progress);

			value = 0.5f;

			Assert.AreEqual(0.5f, tracker.Progress);
		}
	}
}
