using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	internal static class GlobalReferenceRegistry
	{
		private static readonly Dictionary<Type, Component> Instances = new();
		private static readonly object                      Lock      = new();

		public static T GetRequired<T>() where T : Component
		{
			if (TryGet<T>(out var instance))
				return instance;

			throw new InvalidOperationException(
				$"{typeof(T).Name} is not registered as a global reference."
			);
		}

		public static bool TryGet<T>([NotNullWhen(true)] out T? instance)
			where T : Component
		{
			lock (Lock)
			{
				var type = typeof(T);

				if (!Instances.TryGetValue(type, out var component))
				{
					instance = null;
					return false;
				}

				if (component == null)
				{
					Instances.Remove(type);
					instance = null;
					return false;
				}

				instance = component as T;
				return instance != null;
			}
		}

		internal static bool TryGet(Type type, [NotNullWhen(true)] out Component? instance)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			lock (Lock)
			{
				if (!Instances.TryGetValue(type, out var component) || component == null)
				{
					Instances.Remove(type);
					instance = null;
					return false;
				}

				instance = component;
				return true;
			}
		}

		internal static bool TryRegister(Type type, Component instance)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (instance == null)
				throw new ArgumentNullException(nameof(instance));

			if (!typeof(Component).IsAssignableFrom(type))
				throw new ArgumentException(
					$"{type.Name} must inherit from {nameof(Component)}.",
					nameof(type)
				);

			lock (Lock)
			{
				if (Instances.TryGetValue(type, out var existing))
				{
					if (existing == null)
					{
						Instances.Remove(type);
					}
					else
					{
						return ReferenceEquals(existing, instance);
					}
				}

				Instances[type] = instance;
				return true;
			}
		}

		internal static void Unregister(Type type, Component instance)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			lock (Lock)
			{
				if (!Instances.TryGetValue(type, out var existing))
					return;

				if (existing == null || ReferenceEquals(existing, instance))
					Instances.Remove(type);
			}
		}

		internal static void ResetForTests() =>
			Clear();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetOnSubsystemRegistration() =>
			Clear();

		private static void Clear()
		{
			lock (Lock)
			{
				Instances.Clear();
			}
		}
	}
}
