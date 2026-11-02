using System.Reflection;
using Bezoro.Unity.Core.Application;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies public lifecycle flags exposed for Unity initialization stages.
	/// </summary>
	[TestFixture]
	public sealed class ApplicationLifecycleTests
	{
		[TearDown]
		public void TearDown()
		{
			InvokeLifecycleCallback("OnSubsystemRegistration");
		}

		[TestCase("OnAfterAssembliesLoaded", ApplicationLifecycle.InitializationStage.AfterAssembliesLoaded)]
		[TestCase("OnAfterSceneLoad",        ApplicationLifecycle.InitializationStage.AfterSceneLoad)]
		[TestCase("OnBeforeSceneLoad",       ApplicationLifecycle.InitializationStage.BeforeSceneLoad)]
		[TestCase("OnBeforeSplashScreen",    ApplicationLifecycle.InitializationStage.BeforeSplashScreen)]
		[TestCase("OnInitializeOnLoad",      ApplicationLifecycle.InitializationStage.InitializeOnLoad)]
		[TestCase("OnSubsystemRegistration", ApplicationLifecycle.InitializationStage.SubsystemRegistration)]
		public void LifecycleCallback_SetsCurrentStageAndMatchingFlag(
			string                                   callbackName,
			ApplicationLifecycle.InitializationStage expectedStage
		)
		{
			InvokeLifecycleCallback(callbackName);

			Assert.AreEqual(expectedStage, ApplicationLifecycle.CurrentStage);
			Assert.AreEqual(
				expectedStage == ApplicationLifecycle.InitializationStage.AfterAssembliesLoaded,
				ApplicationLifecycle.IsAfterAssembliesLoaded
			);

			Assert.AreEqual(
				expectedStage == ApplicationLifecycle.InitializationStage.AfterSceneLoad,
				ApplicationLifecycle.IsAfterSceneLoad
			);

			Assert.AreEqual(
				expectedStage == ApplicationLifecycle.InitializationStage.BeforeSceneLoad,
				ApplicationLifecycle.IsBeforeSceneLoad
			);

			Assert.AreEqual(
				expectedStage == ApplicationLifecycle.InitializationStage.BeforeSplashScreen,
				ApplicationLifecycle.IsBeforeSplashScreen
			);

			Assert.AreEqual(
				expectedStage == ApplicationLifecycle.InitializationStage.InitializeOnLoad,
				ApplicationLifecycle.IsInitializeOnLoad
			);

			Assert.AreEqual(
				expectedStage == ApplicationLifecycle.InitializationStage.SubsystemRegistration,
				ApplicationLifecycle.IsSubsystemRegistration
			);
		}

		private static void InvokeLifecycleCallback(string callbackName)
		{
			var method = typeof(ApplicationLifecycle).GetMethod(
				callbackName,
				BindingFlags.Static | BindingFlags.NonPublic
			);

			Assert.IsNotNull(method, $"Expected lifecycle callback `{callbackName}` to exist.");
			method!.Invoke(null, null);
		}
	}
}
