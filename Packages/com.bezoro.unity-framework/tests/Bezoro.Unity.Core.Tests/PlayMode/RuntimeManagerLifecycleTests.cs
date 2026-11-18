using System;
using System.Collections;
using Bezoro.Unity.Core.Common.Extensions;
using Bezoro.Unity.Core.Loading;
using Bezoro.Unity.Core.MonoBehaviours;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	[TestFixture]
	public sealed class RuntimeManagerLifecycleTests
	{
		private Scene _scene;
		private Scene _ownerScene;
		private AudioClip? _clip;

		[SetUp]
		public void SetUp()
		{
			_scene = SceneManager.CreateScene(nameof(RuntimeManagerLifecycleTests));
		}

		[UnityTearDown]
		public IEnumerator TearDown()
		{
			if (_ownerScene.IsValid() && _ownerScene.isLoaded)
				yield return SceneManager.UnloadSceneAsync(_ownerScene);
			if (_scene.IsValid() && _scene.isLoaded)
				yield return SceneManager.UnloadSceneAsync(_scene);
			if (_clip != null)
				Object.Destroy(_clip);
			yield return null;
		}

		[UnityTest]
		public IEnumerator InstantiateAsync_ReplacingOwnerInAdditiveScene_AcceptsRequests()
		{
			using var progress = new TestGlobalProvider<LoadingProgressService>("Replacement test progress", _scene);
			progress.Instance.BeginOperation("Replacement test");
			_ownerScene = SceneManager.CreateScene("Previous instantiation owner");
			CreateManager<InstantiationManager>(_ownerScene);
			yield return SceneManager.UnloadSceneAsync(_ownerScene);

			var owner = CreateManager<InstantiationManager>(_scene);
			Assert.AreSame(owner, InstantiationManager.Instance);
			var prefab = CreateInactiveObject("Instantiation source", _scene);
			var task = InstantiationManager.InstantiateAsync(prefab, parent: owner.transform);
			yield return task.ToCoroutine(clone => Assert.AreSame(owner.transform, clone.transform.parent));
		}

		[UnityTest]
		public IEnumerator StopBGM_DuringPendingFade_PreventsDelayedClipSwitch()
		{
			using var audio = new TestGlobalProvider<AudioManager>("Interrupted fade audio", _scene);
			var manager = audio.Instance;
			_clip = AudioClip.Create("Interrupted fade", 44100, 1, 44100, false);
			manager.PlayBGM(_clip, fadeInDuration: 0.1f);
			manager.StopBGM();
			yield return new WaitForSeconds(0.2f);

			var sources = manager.GetComponents<AudioSource>();
			Assert.AreEqual(2, sources.Length);
			foreach (var source in sources)
				Assert.IsNull(source.clip);
		}

		[Test]
		public void ProgressProviderScope_PreservesAndRestoresExistingProvider()
		{
			LoadingProgressService.TryGetInstance(out var inherited);
			using (var original = new TestGlobalProvider<LoadingProgressService>("Original progress", _scene))
			{
				var scope = original.Instance.GetComponent<SingletonScope>();
				var reference = original.Instance.GetComponent<GlobalReference>();
				using (var temporary = new TestGlobalProvider<LoadingProgressService>("Temporary progress", _scene))
				{
					Assert.AreSame(temporary.Instance, LoadingProgressService.Instance);
					Assert.IsTrue(original.Instance.isActiveAndEnabled);
					Assert.IsTrue(scope.IsAccepted);
					Assert.IsTrue(reference.IsRegistered);
				}
				Assert.AreSame(original.Instance, LoadingProgressService.Instance);
				Assert.AreSame(original.Instance, scope.Identity);
				Assert.AreSame(original.Instance, reference.Target);
			}
			LoadingProgressService.TryGetInstance(out var restored);
			Assert.AreSame(inherited, restored);
		}

		[Test]
		public void CameraRegistered_AtCapacity_FiresOnlyForStoredCameras()
		{
			var manager = CreateManager<CameraManager>(_scene);
			int notifications = 0;
			void Count(Camera camera) => notifications++;
			CameraManager.CameraRegistered += Count;
			try
			{
				for (int i = 0; i < manager.Cameras.Length; i++)
				{
					var go = CreateInactiveObject("Secondary camera", _scene);
					CameraManager.Register(go.AddComponent<Camera>());
				}

				Assert.AreEqual(manager.Cameras.Length - 1, notifications);
			}
			finally
			{
				CameraManager.CameraRegistered -= Count;
			}
		}

		[Test]
		public void GetRandomPosition_WithoutNavigableAreas_ReturnsAgentPosition()
		{
			var go = CreateInactiveObject("NavMesh sample", _scene);
			go.transform.position = new Vector3(234f, 113f, 56f);
			var agent = go.AddComponent<NavMeshAgent>();
			agent.areaMask = 0;
			var randomState = UnityEngine.Random.state;
			try
			{
				Assert.AreEqual(go.transform.position, agent.GetRandomPosition());
				Assert.Throws<ArgumentOutOfRangeException>(() => agent.GetRandomPosition(float.PositiveInfinity));
			}
			finally
			{
				UnityEngine.Random.state = randomState;
			}
		}

		private static T CreateManager<T>(Scene scene) where T : MonoBehaviour
		{
			var go = CreateInactiveObject(typeof(T).Name, scene);
			var manager = go.AddComponent<T>();
			go.GetComponent<SingletonScope>().Configure(manager, SingletonLifetime.Scene);
			go.GetComponent<GlobalReference>().ConfigureTarget(manager);
			go.SetActive(true);
			return manager;
		}

		private static GameObject CreateInactiveObject(string name, Scene scene)
		{
			var go = new GameObject(name);
			go.SetActive(false);
			SceneManager.MoveGameObjectToScene(go, scene);
			return go;
		}
	}
}
