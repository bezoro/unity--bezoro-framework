using Bezoro.Unity.Core.ScriptableObjects;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	public sealed class ScriptableSingletonProbe : ScriptableSingleton<ScriptableSingletonProbe>
	{
		public void InitializeForTest() =>
			Awake();
	}
}
