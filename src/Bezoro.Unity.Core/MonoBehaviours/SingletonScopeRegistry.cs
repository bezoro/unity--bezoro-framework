using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	internal static class SingletonScopeRegistry
	{
		private static readonly Dictionary<Type, SingletonScope> Scopes = new();
		private static readonly object                          Lock   = new();

		internal static RegistrationResult TryRegister(
			Type           identityType,
			SingletonScope scope)
		{
			if (identityType == null)
				throw new ArgumentNullException(nameof(identityType));

			if (scope == null)
				throw new ArgumentNullException(nameof(scope));

			lock (Lock)
			{
				if (Scopes.TryGetValue(identityType, out var existing))
				{
					if (existing == null)
					{
						Scopes.Remove(identityType);
					}
					else if (ReferenceEquals(existing, scope))
					{
						return RegistrationResult.Accepted;
					}
					else
					{
						return RegistrationResult.RejectedDuplicate;
					}
				}

				Scopes[identityType] = scope;
				return RegistrationResult.Accepted;
			}
		}

		internal static void Unregister(Type identityType, SingletonScope scope)
		{
			if (identityType == null)
				throw new ArgumentNullException(nameof(identityType));

			lock (Lock)
			{
				if (!Scopes.TryGetValue(identityType, out var existing))
					return;

				if (existing == null || ReferenceEquals(existing, scope))
					Scopes.Remove(identityType);
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
				Scopes.Clear();
			}
		}

		internal enum RegistrationResult
		{
			Accepted,
			RejectedDuplicate
		}
	}
}
