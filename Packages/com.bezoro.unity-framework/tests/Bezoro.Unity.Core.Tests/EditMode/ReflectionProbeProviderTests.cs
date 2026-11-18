using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.ReflectionProbes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class ReflectionProbeProviderTests
	{
		[SetUp]
		public void SetUp()
		{
			ResetReflectionProbeManager();
		}

		[TearDown]
		public void TearDown()
		{
			ResetReflectionProbeManager();
		}

		[Test]
		public void OnEnableAndOnDisable_RegisterAndUnregisterReflectionProbe()
		{
			var manager     = CreateReflectionProbeManager();
			var probeObject = new GameObject("Reflection Probe Provider");
			probeObject.SetActive(false);

			try
			{
				var probe    = probeObject.AddComponent<ReflectionProbe>();
				var provider = probeObject.AddComponent<ReflectionProbeProvider>();

				InvokeProviderLifecycle(provider, "OnEnable");

				CollectionAssert.Contains(GetRegisteredProbes(manager), probe);

				InvokeProviderLifecycle(provider, "OnDisable");

				CollectionAssert.DoesNotContain(GetRegisteredProbes(manager), probe);
			}
			finally
			{
				Object.DestroyImmediate(probeObject);
			}
		}

		[Test]
		public void LateManager_RegistersAlreadyEnabledProvider()
		{
			var probeObject = new GameObject("Late manager provider");
			probeObject.SetActive(false);
			var probe = probeObject.AddComponent<ReflectionProbe>();
			var provider = probeObject.AddComponent<ReflectionProbeProvider>();
			try
			{
				provider.OnEnable();
				var manager = CreateReflectionProbeManager();
				CollectionAssert.Contains(GetRegisteredProbes(manager), probe);
			}
			finally
			{
				provider.OnDisable();
				Object.DestroyImmediate(probeObject);
			}
		}

		private static ReflectionProbeManager CreateReflectionProbeManager()
		{
			var manager = new GameObject("Reflection Probe Manager").AddComponent<ReflectionProbeManager>();
			manager.GetComponent<SingletonScope>().Configure(manager, SingletonLifetime.Scene);
			manager.GetComponent<GlobalReference>().ConfigureTarget(manager);
			InvokeManagerEnable(manager);
			return manager;
		}

		private static IReadOnlyList<ReflectionProbe> GetRegisteredProbes(ReflectionProbeManager manager)
		{
			var model = (ReflectionProbeManagerModel<ReflectionProbe>)typeof(ReflectionProbeManager)
																	  .GetField(
																		  "_model",
																		  BindingFlags.NonPublic | BindingFlags.Instance
																	  )!
																	  .GetValue(manager);

			return model.ReflectionProbes;
		}

		private static void InvokeManagerEnable(ReflectionProbeManager manager)
		{
			typeof(ReflectionProbeManager)
				.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(manager, null);
		}

		private static void InvokeProviderLifecycle(ReflectionProbeProvider provider, string methodName)
		{
			typeof(ReflectionProbeProvider)
				.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!
				.Invoke(provider, null);
		}

		private static void ResetReflectionProbeManager()
		{
			foreach (var manager in Object.FindObjectsByType<ReflectionProbeManager>(FindObjectsInactive.Include))
			{
				if (manager != null)
					Object.DestroyImmediate(manager.gameObject);
			}

			foreach (var provider in Object.FindObjectsByType<ReflectionProbeProvider>(FindObjectsInactive.Include))
			{
				if (provider != null)
					Object.DestroyImmediate(provider.gameObject);
			}

			SingletonScopeRegistry.ResetForTests();
			GlobalReferenceRegistry.ResetForTests();
		}
	}
}
