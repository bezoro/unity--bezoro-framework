using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bezoro.Unity.Core.Application;

[DefaultExecutionOrder(int.MinValue)]
/// <summary>
/// Records the latest Unity runtime initialization stage reached by the application.
/// </summary>
public static class ApplicationLifecycle
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded() =>
		CurrentStage = InitializationStage.AfterAssembliesLoaded;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void OnAfterSceneLoad() =>
		CurrentStage = InitializationStage.AfterSceneLoad;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad() =>
		CurrentStage = InitializationStage.BeforeSceneLoad;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void OnBeforeSplashScreen() =>
		CurrentStage = InitializationStage.BeforeSplashScreen;

#if UNITY_EDITOR
	[InitializeOnLoadMethod]
	private static void OnInitializeOnLoad() =>
		CurrentStage = InitializationStage.InitializeOnLoad;
#endif

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnSubsystemRegistration() =>
		CurrentStage = InitializationStage.SubsystemRegistration;

	public static bool IsAfterAssembliesLoaded => CurrentStage == InitializationStage.AfterAssembliesLoaded;
	public static bool IsAfterSceneLoad        => CurrentStage == InitializationStage.AfterSceneLoad;
	public static bool IsBeforeSceneLoad       => CurrentStage == InitializationStage.BeforeSceneLoad;
	public static bool IsBeforeSplashScreen    => CurrentStage == InitializationStage.BeforeSplashScreen;
	public static bool IsInitializeOnLoad      => CurrentStage == InitializationStage.InitializeOnLoad;
	public static bool IsSubsystemRegistration => CurrentStage == InitializationStage.SubsystemRegistration;

	public static InitializationStage CurrentStage { get; private set; }

	/// <summary>
	///     Lists the supported Initialization Stage values.
	/// </summary>
	public enum InitializationStage
	{
		None,
		SubsystemRegistration,
		AfterAssembliesLoaded,
		BeforeSplashScreen,
		BeforeSceneLoad,
		AfterSceneLoad,
		InitializeOnLoad
	}
}
