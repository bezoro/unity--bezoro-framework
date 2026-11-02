using System;
using System.Collections.Generic;

namespace Bezoro.Unity.Core.ScriptableObjects
{
	public sealed class SceneCollectionLoadPlan
	{
		private SceneCollectionLoadPlan(
			IReadOnlyList<string> providerScenePaths,
			IReadOnlyList<string> remainingScenePaths,
			string activeScenePath)
		{
			ProviderScenePaths  = providerScenePaths;
			RemainingScenePaths = remainingScenePaths;
			ActiveScenePath      = activeScenePath;
		}

		public IReadOnlyList<string> ProviderScenePaths { get; }
		public IReadOnlyList<string> RemainingScenePaths { get; }
		public string ActiveScenePath { get; }

		public static SceneCollectionLoadPlan Create(IReadOnlyList<SceneCollectionEntry> entries)
		{
			if (entries == null)
				throw new ArgumentNullException(nameof(entries));

			var providers = new List<string>(entries.Count);
			var remaining = new List<string>(entries.Count);
			var activeScenePath = string.Empty;

			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				if (entry?.Scene == null || !entry.Scene.HasValue)
					continue;

				if (string.IsNullOrEmpty(activeScenePath))
					activeScenePath = entry.Scene.Path;

				if (entry.Dependencies.Count > 0)
					providers.Add(entry.Scene.Path);
				else
					remaining.Add(entry.Scene.Path);
			}

			return new(providers, remaining, activeScenePath);
		}
	}
}
