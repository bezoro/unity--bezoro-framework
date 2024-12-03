namespace Bezoro.Framework.Unity.Bootstrapping
{
	public interface IAfterSceneLoadInitializable
	{
		public void InitializeAfterSceneLoad();
	}

	public interface IBeforeSceneLoadInitializable
	{
		public void InitializeBeforeSceneLoad();
	}

	public interface IBeforeSplashScreenInitializable
	{
		public void InitializeBeforeSplashScreen();
	}

	public interface IAfterAssembliesLoadedInitializable
	{
		public void InitializeAfterAssembliesLoaded();
	}

	public interface ISubsystemRegistrationInitializable
	{
		public void InitializeSubsystemRegistration();
	}
}
