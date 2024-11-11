namespace Bezoro.Framework.Unity.Bootstrapping
{
	public enum InitializationPhase
	{
		SubsystemRegistration,
		AfterAssembliesLoaded,
		BeforeSplashScreen,
		BeforeSceneLoad,
		AfterSceneLoad
	}
}
