using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	[TestFixture]
	public sealed class SingletonMonoBehaviourHelpersPlayModeTests
	{
		private GameObject _createdObject;

		[TearDown]
		public void TearDown()
		{
			if (_createdObject != null)
				Object.Destroy(_createdObject);
		}

		[Test]
		public void CreateSingleton_CreatesNamedHiddenComponent()
		{
			var singleton = SingletonMonoBehaviourHelpers.CreateSingleton<SingletonHelperTestComponent>();
			_createdObject = singleton.gameObject;

			Assert.AreEqual("[SingletonHelperTestComponent]", singleton.gameObject.name);
			Assert.AreEqual(HideFlags.NotEditable,            singleton.gameObject.hideFlags);
		}

		private sealed class SingletonHelperTestComponent : MonoBehaviour { }
	}
}
