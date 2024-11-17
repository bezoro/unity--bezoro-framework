namespace Bezoro.Framework.Unity.Bootstrapping
{
	public interface IAfterSceneLoadInitializable
	{
		void InitializeAfterSceneLoad();
	}

	public interface IBeforeSceneLoadInitializable
	{
		void InitializeBeforeSceneLoad();
	}

	public interface IBeforeSplashScreenInitializable
	{
		void InitializeBeforeSplashScreen();
	}

	public interface IAfterAssembliesLoadedInitializable
	{
		void InitializeAfterAssembliesLoaded();
	}

	public interface ISubsystemRegistrationInitializable
	{
		void InitializeSubsystemRegistration();
	}
}
