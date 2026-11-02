using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Common.Helpers
{
	/// <summary>
	///     Queries editor Addressables settings for entries, groups, and addresses.
	/// </summary>
	public static class AddressableEditorChecker
	{
		/// <summary>
		///     Gets the AddressableAssetEntry for a specific address if it exists
		/// </summary>
		/// <param name="addressableName">The address to find</param>
		/// <returns>The AddressableAssetEntry if found, null otherwise</returns>
		public static AddressableAssetEntry? GetAddressableEntry(string addressableName)
		{
			if (string.IsNullOrEmpty(
					addressableName
				)) return null;

			var settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings == null) return null;

			// Check each group for the entry with the matching address
			foreach (var group in settings.groups)
			{
				if (group == null) continue;

				var entry = group.entries.FirstOrDefault(e => e.address == addressableName
				);

				if (entry != null) return entry;
			}

			return null;
		}

		/// <summary>
		///     Checks if an addressable exists with the specified address in the editor
		/// </summary>
		/// <param name="addressableName">The address to check for existence</param>
		/// <returns>True if the addressable exists in the editor, false otherwise</returns>
		public static bool AddressableExistsInEditor(string addressableName)
		{
			if (string.IsNullOrEmpty(
					addressableName
				)) return false;

			// Get the AddressableAssetSettings
			var settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings == null)
			{
				Debug.LogWarning(
					"Addressable Asset Settings not found. Make sure Addressables is properly set up in the project."
				);

				return false;
			}

			// Iterate through all groups and entries to find if the addressable name exists
			foreach (var group in settings.groups)
			{
				if (group == null) continue;

				foreach (var entry in group.entries)
				{
					if (entry.address == addressableName)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Gets the group name that contains the addressable with the specified address
		/// </summary>
		/// <param name="addressableName">The address to find</param>
		/// <returns>The group name if found, null otherwise</returns>
		public static string? GetAddressableGroupName(string addressableName)
		{
			if (string.IsNullOrEmpty(
					addressableName
				)) return null;

			var settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings == null) return null;

			// Check each group for the entry with the matching address
			foreach (var group in settings.groups)
			{
				if (group == null) continue;

				var entry = group.entries.FirstOrDefault(e => e.address == addressableName
				);

				if (entry != null) return group.Name;
			}

			return null;
		}
	}
}
