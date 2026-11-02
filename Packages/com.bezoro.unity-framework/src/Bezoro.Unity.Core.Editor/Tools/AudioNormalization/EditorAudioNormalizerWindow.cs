using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Tools.AudioNormalization
{
	public sealed class EditorAudioNormalizerWindow : EditorWindow
	{
		private const string MENU_PATH = "Bezoro/Tools/Audio Normalizer";
		private const string BACKUP_FOLDER_NAME = "AudioNormalizerBackups";
		private const string DEFAULT_FOLDER_PATH =
			"Assets/_Project/Engine/EditorAudioFeedback/Resources/VO";
		private const float  DEFAULT_TARGET_ACTIVE_RMS_DB = -22.7f;
		private const float  DEFAULT_MAX_PEAK_DB = -1f;
		private const float  DEFAULT_SILENCE_THRESHOLD_DB = -60f;

		private readonly List<AudioClipAnalysisRow> _rows = new();

		private DefaultAsset _folderAsset = null!;
		private string       _folderPath = DEFAULT_FOLDER_PATH;
		private bool         _includeSubfolders;
		private bool         _applyCopiesToEditorAudioFeedbackSettings = true;
		private bool         _createBackupsWhenReplacing = true;
		private float        _targetActiveRmsDb = DEFAULT_TARGET_ACTIVE_RMS_DB;
		private float        _maxPeakDb = DEFAULT_MAX_PEAK_DB;
		private float        _silenceThresholdDb = DEFAULT_SILENCE_THRESHOLD_DB;
		private Vector2      _scrollPosition;
		private string       _statusMessage = string.Empty;
		private MessageType  _statusMessageType = MessageType.Info;

		[MenuItem(
			MENU_PATH
		)]
		public static void ShowWindow()
		{
			var window = GetWindow<EditorAudioNormalizerWindow>(
				"Audio Normalizer"
			);
			window.minSize = new Vector2(
				760f,
				420f
			);
			window.Show();
		}

		private void OnEnable()
		{
			if (!AssetDatabase.IsValidFolder(
				    _folderPath
			    ))
				return;

			_folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(
				_folderPath
			);
		}

		private void OnGUI()
		{
			DrawControls();
			DrawStatus();
			DrawAnalysisRows();
		}

		private void DrawControls()
		{
			EditorGUILayout.LabelField(
				"Audio Normalizer",
				EditorStyles.boldLabel
			);

			EditorGUI.BeginChangeCheck();
			var folderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
				"Folder",
				_folderAsset,
				typeof(DefaultAsset),
				false
			);
			if (EditorGUI.EndChangeCheck())
				SetFolderAsset(
					folderAsset
				);

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.SelectableLabel(
					_folderPath,
					EditorStyles.textField,
					GUILayout.Height(
						EditorGUIUtility.singleLineHeight
					)
				);

				if (GUILayout.Button(
					    "Reset",
					    GUILayout.Width(
						    80f
					    )
				    ))
					SetFolderPath(
						DEFAULT_FOLDER_PATH
					);
			}

			_targetActiveRmsDb = EditorGUILayout.Slider(
				"Target Active RMS (dBFS)",
				_targetActiveRmsDb,
				-36f,
				-6f
			);
			_maxPeakDb = EditorGUILayout.Slider(
				"Max Peak (dBFS)",
				_maxPeakDb,
				-12f,
				0f
			);
			_silenceThresholdDb = EditorGUILayout.Slider(
				"Silence Threshold (dBFS)",
				_silenceThresholdDb,
				-90f,
				-20f
			);
			_includeSubfolders = EditorGUILayout.Toggle(
				"Include Subfolders",
				_includeSubfolders
			);
			_applyCopiesToEditorAudioFeedbackSettings = EditorGUILayout.Toggle(
				"Apply Copies To Editor Audio Feedback",
				_applyCopiesToEditorAudioFeedbackSettings
			);
			_createBackupsWhenReplacing = EditorGUILayout.Toggle(
				"Create Backups When Replacing",
				_createBackupsWhenReplacing
			);

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button(
					    "Analyze Folder"
				    ))
					AnalyzeFolder();

				EditorGUI.BeginDisabledGroup(
					_rows.Count == 0
				);

				if (GUILayout.Button(
					    "Write Normalized WAV Copies"
				    ))
					WriteNormalizedCopies();

				if (GUILayout.Button(
					    "Replace Originals With Normalized WAVs"
				    ))
					ReplaceOriginalsWithNormalizedWavs();

				EditorGUI.EndDisabledGroup();

				if (GUILayout.Button(
					    "Delete Backups"
				    ))
					DeleteBackups();
			}
		}

		private void DrawStatus()
		{
			if (string.IsNullOrWhiteSpace(
				    _statusMessage
			    ))
				return;

			EditorGUILayout.HelpBox(
				_statusMessage,
				_statusMessageType
			);
		}

		private void DrawAnalysisRows()
		{
			if (_rows.Count == 0) return;

			EditorGUILayout.Space();
			using (new EditorGUILayout.HorizontalScope())
			{
				DrawHeader(
					"Clip",
					220f
				);
				DrawHeader(
					"Peak",
					70f
				);
				DrawHeader(
					"RMS",
					70f
				);
				DrawHeader(
					"Active RMS",
					90f
				);
				DrawHeader(
					"Gain",
					70f
				);
				DrawHeader(
					"Result",
					70f
				);
				DrawHeader(
					"Limited",
					60f
				);
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(
				_scrollPosition
			);

			foreach (var row in _rows)
				DrawAnalysisRow(
					row
				);

			EditorGUILayout.EndScrollView();
		}

		private static void DrawHeader(string label, float width)
		{
			EditorGUILayout.LabelField(
				label,
				EditorStyles.boldLabel,
				GUILayout.Width(
					width
				)
			);
		}

		private static void DrawAnalysisRow(AudioClipAnalysisRow row)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.ObjectField(
					row.Clip,
					typeof(AudioClip),
					false,
					GUILayout.Width(
						220f
					)
				);

				if (!string.IsNullOrWhiteSpace(
					    row.Error
				    ))
				{
					EditorGUILayout.LabelField(
						row.Error
					);
					return;
				}

				DrawDbLabel(
					row.Stats.PeakDb,
					70f
				);
				DrawDbLabel(
					row.Stats.RmsDb,
					70f
				);
				DrawDbLabel(
					row.Stats.ActiveRmsDb,
					90f
				);
				DrawDbLabel(
					row.Plan.GainDb,
					70f
				);
				DrawDbLabel(
					row.Plan.ResultingActiveRmsDb,
					70f
				);
				EditorGUILayout.LabelField(
					row.Plan.IsPeakLimited
						? "Yes"
						: "No",
					GUILayout.Width(
						60f
					)
				);
			}
		}

		private static void DrawDbLabel(float value, float width)
		{
			EditorGUILayout.LabelField(
				float.IsNegativeInfinity(
					value
				)
					? "-Inf"
					: value.ToString(
						"0.00"
					),
				GUILayout.Width(
					width
				)
			);
		}

		private void AnalyzeFolder()
		{
			_rows.Clear();

			if (!AssetDatabase.IsValidFolder(
				    _folderPath
			    ))
			{
				SetStatus(
					$"Audio folder does not exist: {_folderPath}",
					MessageType.Error
				);
				return;
			}

			foreach (var assetPath in EditorAudioNormalizer.FindAudioClipAssetPaths(
				         _folderPath,
				         _includeSubfolders
			         ))
			{
				var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
					assetPath
				);
				if (!EditorAudioNormalizer.TryAnalyzeClip(
					    clip,
					    _silenceThresholdDb,
					    out var stats,
					    out var error
				    ))
				{
					_rows.Add(
						new AudioClipAnalysisRow(
							clip,
							assetPath,
							default,
							default,
							error
						)
					);
					continue;
				}

				_rows.Add(
					new AudioClipAnalysisRow(
						clip,
						assetPath,
						stats,
						EditorAudioNormalizer.CreateNormalizationPlan(
							stats,
							_targetActiveRmsDb,
							_maxPeakDb
						),
						string.Empty
					)
				);
			}

			SetStatus(
				$"Analyzed {_rows.Count} audio clips.",
				MessageType.Info
			);
		}

		private void WriteNormalizedCopies()
		{
			var processableRows = GetProcessableRows();
			if (processableRows.Count == 0)
			{
				SetStatus(
					"No analyzed audio clips are ready to normalize.",
					MessageType.Warning
				);
				return;
			}

			var outputFolderPath = EditorAudioNormalizer.GetNormalizedCopiesFolderPath(
				_folderPath
			);
			Directory.CreateDirectory(
				EditorAudioNormalizer.AssetPathToFullPath(
					outputFolderPath
				)
			);

			var replacementAssetPaths = new Dictionary<string, string>(
				StringComparer.OrdinalIgnoreCase
			);
			var errors = new List<string>();

			foreach (var row in processableRows)
			{
				if (!EditorAudioNormalizer.TryCreateNormalizedWavData(
					    row.Clip,
					    _targetActiveRmsDb,
					    _maxPeakDb,
					    _silenceThresholdDb,
					    out var wavData,
					    out _,
					    out _,
					    out var error
				    ))
				{
					errors.Add(
						$"{row.AssetPath}: {error}"
					);
					continue;
				}

				var outputAssetPath = GetOutputCopyAssetPath(
					row.AssetPath,
					outputFolderPath
				);
				File.WriteAllBytes(
					EditorAudioNormalizer.AssetPathToFullPath(
						outputAssetPath
					),
					wavData
				);
				replacementAssetPaths[row.AssetPath] = outputAssetPath;
			}

			AssetDatabase.Refresh();
			foreach (var outputAssetPath in replacementAssetPaths.Values)
				AssetDatabase.ImportAsset(
					outputAssetPath,
					ImportAssetOptions.ForceUpdate
				);

			var changedSettingsCount = _applyCopiesToEditorAudioFeedbackSettings
				? EditorAudioNormalizer.ApplyClipReplacementMapToEditorAudioFeedbackSettings(
					replacementAssetPaths
				)
				: 0;

			SetStatus(
				errors.Count == 0
					? $"Wrote {replacementAssetPaths.Count} normalized WAV copies to {outputFolderPath}. Updated {changedSettingsCount} editor audio feedback references."
					: $"Wrote {replacementAssetPaths.Count} normalized WAV copies with {errors.Count} errors:\n{string.Join("\n", errors)}",
				errors.Count == 0
					? MessageType.Info
					: MessageType.Warning
			);
		}

		private void ReplaceOriginalsWithNormalizedWavs()
		{
			var processableRows = GetProcessableRows();
			if (processableRows.Count == 0)
			{
				SetStatus(
					"No analyzed audio clips are ready to replace.",
					MessageType.Warning
				);
				return;
			}

			var backupRootFullPath = GetBackupRunDirectoryFullPath();
			var backupMessage = _createBackupsWhenReplacing
				? $"Source files will be backed up to:\n{backupRootFullPath}"
				: "No backup will be created.";

			if (!EditorUtility.DisplayDialog(
				    "Replace Originals With Normalized WAVs",
				    $"This will replace {processableRows.Count} source files.\n\n{backupMessage}\n\nMP3 assets will become WAV assets while keeping their Unity GUIDs.",
				    "Replace",
				    "Cancel"
			    ))
				return;

			if (!TryCreateTemporaryNormalizedWavs(
				    processableRows,
				    out var generatedFiles,
				    out var generationError
			    ))
			{
				DeleteTemporaryGeneratedFiles(
					generatedFiles
				);
				SetStatus(
					generationError,
					MessageType.Error
				);
				return;
			}

			var conflict = GetReplacementConflict(
				generatedFiles
			);
			if (!string.IsNullOrWhiteSpace(
				    conflict
			    ))
			{
				DeleteTemporaryGeneratedFiles(
					generatedFiles
				);
				SetStatus(
					conflict,
					MessageType.Error
				);
				return;
			}

			try
			{
				if (_createBackupsWhenReplacing)
					Directory.CreateDirectory(
						backupRootFullPath
					);

				AssetDatabase.StartAssetEditing();
				try
				{
					foreach (var generatedFile in generatedFiles)
						ReplaceOriginalWithGeneratedWav(
							generatedFile,
							backupRootFullPath,
							_createBackupsWhenReplacing
						);
				}
				finally
				{
					AssetDatabase.StopAssetEditing();
				}

				AssetDatabase.Refresh();
				foreach (var generatedFile in generatedFiles)
					AssetDatabase.ImportAsset(
						generatedFile.ReplacementAssetPath,
						ImportAssetOptions.ForceUpdate
					);

				SetStatus(
					_createBackupsWhenReplacing
						? $"Replaced {generatedFiles.Count} original audio assets with normalized WAVs. Backup: {backupRootFullPath}"
						: $"Replaced {generatedFiles.Count} original audio assets with normalized WAVs. No backup was created.",
					MessageType.Info
				);
				AnalyzeFolder();
			}
			catch (Exception ex)
			{
				SetStatus(
					_createBackupsWhenReplacing
						? $"Failed to replace original audio assets: {ex.Message}. Backup: {backupRootFullPath}"
						: $"Failed to replace original audio assets: {ex.Message}. No backup was created.",
					MessageType.Error
				);
			}
			finally
			{
				DeleteTemporaryGeneratedFiles(
					generatedFiles
				);
			}
		}

		private bool TryCreateTemporaryNormalizedWavs(
			IReadOnlyList<AudioClipAnalysisRow> rows,
			out List<GeneratedNormalizedWav>     generatedFiles,
			out string                           error
		)
		{
			generatedFiles = new List<GeneratedNormalizedWav>();
			error          = string.Empty;

			var temporaryFolderPath = Path.Combine(
				Path.GetTempPath(),
				nameof(Bezoro),
				nameof(EditorAudioNormalizer),
				DateTime.Now.ToString(
					"yyyyMMdd_HHmmss_fff"
				)
			);
			Directory.CreateDirectory(
				temporaryFolderPath
			);

			foreach (var row in rows)
			{
				if (!EditorAudioNormalizer.TryCreateNormalizedWavData(
					    row.Clip,
					    _targetActiveRmsDb,
					    _maxPeakDb,
					    _silenceThresholdDb,
					    out var wavData,
					    out _,
					    out _,
					    out var rowError
				    ))
				{
					error = $"{row.AssetPath}: {rowError}";
					return false;
				}

				var replacementAssetPath = EditorAudioNormalizer.GetReplacementAssetPath(
					row.AssetPath
				);
				var temporaryWavPath = Path.Combine(
					temporaryFolderPath,
					$"{Guid.NewGuid():N}.wav"
				);
				File.WriteAllBytes(
					temporaryWavPath,
					wavData
				);
				generatedFiles.Add(
					new GeneratedNormalizedWav(
						row.AssetPath,
						replacementAssetPath,
						temporaryWavPath
					)
				);
			}

			return true;
		}

		private static string GetReplacementConflict(IReadOnlyList<GeneratedNormalizedWav> generatedFiles)
		{
			foreach (var generatedFile in generatedFiles)
			{
				if (PathsEqual(
					    generatedFile.SourceAssetPath,
					    generatedFile.ReplacementAssetPath
				    ))
					continue;

				if (AssetDatabase.LoadAssetAtPath<Object>(
					    generatedFile.ReplacementAssetPath
				    ) != null)
					return $"Cannot replace {generatedFile.SourceAssetPath}; target already exists: {generatedFile.ReplacementAssetPath}";

				var replacementFullPath = EditorAudioNormalizer.AssetPathToFullPath(
					generatedFile.ReplacementAssetPath
				);
				if (File.Exists(
					    replacementFullPath
				    ) || File.Exists(
					    $"{replacementFullPath}.meta"
				    ))
					return $"Cannot replace {generatedFile.SourceAssetPath}; target file already exists: {generatedFile.ReplacementAssetPath}";
			}

			return string.Empty;
		}

		private static void ReplaceOriginalWithGeneratedWav(
			GeneratedNormalizedWav generatedFile,
			string                 backupRootFullPath,
			bool                   createBackup
		)
		{
			if (createBackup)
				BackupAssetFile(
					generatedFile.SourceAssetPath,
					backupRootFullPath
				);

			if (!PathsEqual(
				    generatedFile.SourceAssetPath,
				    generatedFile.ReplacementAssetPath
			    ))
			{
				var moveError = AssetDatabase.MoveAsset(
					generatedFile.SourceAssetPath,
					generatedFile.ReplacementAssetPath
				);
				if (!string.IsNullOrEmpty(
					    moveError
				    ))
					throw new IOException(
						moveError
					);
			}

			File.Copy(
				generatedFile.TemporaryWavPath,
				EditorAudioNormalizer.AssetPathToFullPath(
					generatedFile.ReplacementAssetPath
				),
				true
			);
		}

		private void DeleteBackups()
		{
			var backupRootFullPath = GetBackupRootDirectoryFullPath();
			if (!Directory.Exists(
				    backupRootFullPath
			    ))
			{
				SetStatus(
					$"No audio normalizer backups found at: {backupRootFullPath}",
					MessageType.Info
				);
				return;
			}

			if (!EditorUtility.DisplayDialog(
				    "Delete Audio Normalizer Backups",
				    $"Delete all audio normalizer backups from:\n{backupRootFullPath}",
				    "Delete",
				    "Cancel"
			    ))
				return;

			try
			{
				Directory.Delete(
					backupRootFullPath,
					true
				);
				SetStatus(
					$"Deleted audio normalizer backups: {backupRootFullPath}",
					MessageType.Info
				);
			}
			catch (Exception ex)
			{
				SetStatus(
					$"Failed to delete audio normalizer backups: {ex.Message}",
					MessageType.Error
				);
			}
		}

		private static void BackupAssetFile(string assetPath, string backupRootFullPath)
		{
			var sourceFullPath = EditorAudioNormalizer.AssetPathToFullPath(
				assetPath
			);
			BackupFile(
				sourceFullPath,
				Path.Combine(
					backupRootFullPath,
					EditorAudioNormalizer.NormalizeAssetPath(
							assetPath
						)
						.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
				)
			);
			BackupFile(
				$"{sourceFullPath}.meta",
				Path.Combine(
					backupRootFullPath,
					$"{EditorAudioNormalizer.NormalizeAssetPath(assetPath)}.meta".Replace(
						'/',
						Path.DirectorySeparatorChar
					)
				)
			);
		}

		private static void BackupFile(string sourceFullPath, string backupFullPath)
		{
			if (!File.Exists(
				    sourceFullPath
			    ))
				return;

			Directory.CreateDirectory(
				Path.GetDirectoryName(
					backupFullPath
				)!
			);
			File.Copy(
				sourceFullPath,
				backupFullPath,
				true
			);
		}

		private static void DeleteTemporaryGeneratedFiles(
			IReadOnlyList<GeneratedNormalizedWav> generatedFiles
		)
		{
			foreach (var generatedFile in generatedFiles)
			{
				try
				{
					if (File.Exists(
						    generatedFile.TemporaryWavPath
					    ))
						File.Delete(
							generatedFile.TemporaryWavPath
						);
				}
				catch (IOException)
				{
					/* The temp file can be removed by the OS or the next tool run. */
				}
				catch (UnauthorizedAccessException)
				{
					/* The temp file can be removed by the OS or the next tool run. */
				}
			}
		}

		private IReadOnlyList<AudioClipAnalysisRow> GetProcessableRows() =>
			_rows
				.Where(row => row.Clip != null && string.IsNullOrWhiteSpace(
					row.Error
				))
				.ToArray();

		private static string GetOutputCopyAssetPath(string sourceAssetPath, string outputFolderPath) =>
			EditorAudioNormalizer.NormalizeAssetPath(
				$"{outputFolderPath}/{Path.GetFileNameWithoutExtension(sourceAssetPath)}.wav"
			);

		private static string GetBackupRootDirectoryFullPath() =>
			Path.Combine(
				Directory.GetParent(
						UnityEngine.Application.dataPath
					)
					!.FullName,
				BACKUP_FOLDER_NAME
			);

		private static string GetBackupRunDirectoryFullPath() =>
			Path.Combine(
				GetBackupRootDirectoryFullPath(),
				DateTime.Now.ToString(
					"yyyyMMdd_HHmmss"
				)
			);

		private void SetFolderAsset(DefaultAsset folderAsset)
		{
			_folderAsset = folderAsset;
			if (folderAsset == null) return;

			var assetPath = AssetDatabase.GetAssetPath(
				folderAsset
			);
			if (!AssetDatabase.IsValidFolder(
				    assetPath
			    ))
			{
				SetStatus(
					"Selected asset is not a folder.",
					MessageType.Warning
				);
				return;
			}

			SetFolderPath(
				assetPath
			);
		}

		private void SetFolderPath(string folderPath)
		{
			_folderPath = EditorAudioNormalizer.NormalizeAssetPath(
				folderPath
			);
			_folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(
				_folderPath
			);
			_rows.Clear();
		}

		private void SetStatus(string message, MessageType messageType)
		{
			_statusMessage     = message;
			_statusMessageType = messageType;
			Repaint();
		}

		private static bool PathsEqual(string left, string right) =>
			string.Equals(
				EditorAudioNormalizer.NormalizeAssetPath(
					left
				),
				EditorAudioNormalizer.NormalizeAssetPath(
					right
				),
				StringComparison.OrdinalIgnoreCase
			);

		private sealed class AudioClipAnalysisRow
		{
			public AudioClipAnalysisRow(
				AudioClip                 clip,
				string                    assetPath,
				AudioNormalizationStats  stats,
				AudioNormalizationPlan   plan,
				string                    error
			)
			{
				Clip      = clip;
				AssetPath = assetPath;
				Stats     = stats;
				Plan      = plan;
				Error     = error;
			}

			public AudioClip                 Clip      { get; }
			public string                    AssetPath { get; }
			public AudioNormalizationStats  Stats     { get; }
			public AudioNormalizationPlan   Plan      { get; }
			public string                    Error     { get; }
		}

		private readonly struct GeneratedNormalizedWav
		{
			public GeneratedNormalizedWav(
				string sourceAssetPath,
				string replacementAssetPath,
				string temporaryWavPath
			)
			{
				SourceAssetPath      = sourceAssetPath;
				ReplacementAssetPath = replacementAssetPath;
				TemporaryWavPath     = temporaryWavPath;
			}

			public string SourceAssetPath      { get; }
			public string ReplacementAssetPath { get; }
			public string TemporaryWavPath     { get; }
		}
	}
}
