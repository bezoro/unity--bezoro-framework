using Bezoro.Core.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

#if UNITY_PIPELINE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Bezoro.Unity.Core.Editor.Tools;

/// <summary>
///     Copies Unity, platform, render pipeline, and package details to the clipboard.
/// </summary>
public static class SystemInfoMenu
{
	[MenuItem(
		Constants.BEZORO + "/Tools/Copy System Information"
	)]
	public static void CopySystemInformation()
	{
		string systemInfo = GetSystemInformation();
		EditorGUIUtility.systemCopyBuffer = systemInfo;
		Debug.Log(
			"System information copied to clipboard:\n" + systemInfo
		);
	}

	private static string GetOSInfo()
	{
		string os       = SystemInfo.operatingSystem;
		var    platform = UnityEngine.Application.platform.ToString();

		// Enhanced platform detection
		string platformDetails = platform switch
		{
			"WindowsEditor" => "Windows Editor",
			"WindowsPlayer" => "Windows Standalone",
			"OSXEditor"     => "macOS Editor",
			"OSXPlayer"     => "macOS Standalone",
			"LinuxEditor"   => "Linux Editor",
			"LinuxPlayer"   => "Linux Standalone",
			_               => platform
		};

		return $"{os} ({platformDetails})";
	}

	private static string GetRenderPipelineInfo()
	{
		var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;

		if (renderPipelineAsset == null) return "Built-in Render Pipeline";

		string pipelineTypeName = renderPipelineAsset.GetType().Name;

		// Handle URP
		if (renderPipelineAsset is UniversalRenderPipelineAsset)
		{
			var packageInfo =
				PackageInfo.FindForAssetPath(
					"Packages/com.unity.render-pipelines.universal/package.json"
				);

			if (packageInfo != null) return $"Universal Render Pipeline (URP) - Version {packageInfo.version}";

			return "Universal Render Pipeline (URP) - Version Unknown";
		}

#if UNITY_PIPELINE_HDRP
		// Handle HDRP
		if (renderPipelineAsset is HDRenderPipelineAsset)
		{
			var packageInfo =
				PackageInfo.FindForAssetPath(
					"Packages/com.unity.render-pipelines.high-definition/package.json"
				);

			if (packageInfo != null)
			{
				return $"High Definition Render Pipeline (HDRP) - Version {packageInfo.version}";
			}

			return "High Definition Render Pipeline (HDRP) - Version Unknown";
		}
#else
		// Handle HDRP without the package imported (check by type name)
		if (pipelineTypeName.Contains(
				"HDRenderPipelineAsset"
			) ||
			pipelineTypeName.Contains(
				"HighDefinition"
			))
		{
			var packageInfo =
				PackageInfo.FindForAssetPath(
					"Packages/com.unity.render-pipelines.high-definition/package.json"
				);

			if (packageInfo != null) return $"High Definition Render Pipeline (HDRP) - Version {packageInfo.version}";

			return "High Definition Render Pipeline (HDRP) - Version Unknown";
		}
#endif

		// For other custom render pipelines
		return $"Custom Render Pipeline ({pipelineTypeName})";
	}

	private static string GetSystemInformation()
	{
		string unityVersion       = UnityEngine.Application.unityVersion;
		string renderPipelineInfo = GetRenderPipelineInfo();
		string osInfo             = GetOSInfo();

		return $"Unity Version: {unityVersion}\n" +
			   $"Render Pipeline: {renderPipelineInfo}\n" +
			   $"Operating System: {osInfo}";
	}
}
