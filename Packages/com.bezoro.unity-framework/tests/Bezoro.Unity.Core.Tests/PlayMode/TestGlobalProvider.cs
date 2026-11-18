using System;
using System.Linq;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	/// <summary>Gives a PlayMode fixture its own provider without changing the existing provider's lifecycle.</summary>
	internal sealed class TestGlobalProvider<T> : IDisposable where T : MonoBehaviour
	{
		private readonly T? _previous;
		private readonly SingletonScope? _previousScope;
		private GameObject? _gameObject;
		private T? _provider;
		private SingletonScope? _scope;
		private bool _disposed;

		internal T Instance => _provider!;

		internal TestGlobalProvider(string name, Scene scene = default)
		{
			GlobalReferenceRegistry.TryGet<T>(out _previous);
			if (_previous != null)
			{
				_previousScope = _previous.GetComponentsInParent<SingletonScope>(true)
					.SingleOrDefault(scope => scope.IsAccepted && ReferenceEquals(scope.Identity, _previous));
				GlobalReferenceRegistry.Unregister(typeof(T), _previous);
				if (_previousScope != null)
					SingletonScopeRegistry.Unregister(typeof(T), _previousScope);
			}

			try
			{
				_gameObject = new GameObject(name);
				_gameObject.SetActive(false);
				if (scene.IsValid()) SceneManager.MoveGameObjectToScene(_gameObject, scene);
				_provider = _gameObject.AddComponent<T>();
				_scope = _gameObject.GetComponent<SingletonScope>();
				_scope.Configure(_provider, SingletonLifetime.Scene);
				Assert.IsTrue(_scope.IsAccepted, $"The fixture must own the {typeof(T).Name} singleton scope.");
				var reference = _gameObject.GetComponent<GlobalReference>();
				reference.ConfigureTarget(_provider);
				_gameObject.SetActive(true);
				Assert.IsTrue(GlobalReferenceAccess<T>.IsCurrent(_provider), $"The fixture must own the {typeof(T).Name} global reference.");
			}
			catch (Exception setupError)
			{
				try { Dispose(); }
				catch (Exception cleanupError) { throw new AggregateException(setupError, cleanupError); }
				throw;
			}
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			try
			{
				if (_gameObject != null) Object.DestroyImmediate(_gameObject);
			}
			finally
			{
				// Awake or OnDestroy may not run if setup fails while the fixture object is inactive.
				if (_provider is not null) GlobalReferenceRegistry.Unregister(typeof(T), _provider);
				if (_scope is not null) SingletonScopeRegistry.Unregister(typeof(T), _scope);
				if (_previousScope != null)
				{
					var result = SingletonScopeRegistry.TryRegister(typeof(T), _previousScope);
					Assert.That(result, Is.EqualTo(SingletonScopeRegistry.RegistrationResult.Accepted));
				}

				if (_previous != null && !GlobalReferenceRegistry.TryRegister(typeof(T), _previous))
				{
					if (_previousScope != null) SingletonScopeRegistry.Unregister(typeof(T), _previousScope);
					Assert.Fail($"Could not restore the original {typeof(T).Name} global reference.");
				}
			}
		}
	}
}
