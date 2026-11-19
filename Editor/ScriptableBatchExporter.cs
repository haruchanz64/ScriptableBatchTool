using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaruChanZ64.ScriptableBatchTool.Editor
{
    public static class ScriptableBatchExporter
    {
        public static int ExportAssets(IReadOnlyList<ScriptableWrapper> items, string targetFolder)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogWarning("No ScriptableObject entries to export.");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(targetFolder) || !AssetDatabase.IsValidFolder(targetFolder))
            {
                Debug.LogError($"Invalid target folder: '{targetFolder}'. Please select an existing folder inside Assets.");
                return 0;
            }

            var normalizedFolder = targetFolder.Replace('\\', '/').TrimEnd('/');
            var exportedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var item in items)
                {
                    if (item == null || item.Instance == null)
                    {
                        continue;
                    }

                    item.SerializedObject?.ApplyModifiedPropertiesWithoutUndo();

                    var safeName = SanitizeFileName(item.AssetName);
                    var fullPath = $"{normalizedFolder}/{safeName}.asset";
                    fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

                    var assetToCreate = EditorUtility.IsPersistent(item.Instance)
                        ? Object.Instantiate(item.Instance)
                        : item.Instance;

                    AssetDatabase.CreateAsset(assetToCreate, fullPath);
                    exportedCount++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            if (exportedCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Exported {exportedCount} ScriptableObject asset(s) to '{normalizedFolder}'.");
            }

            return exportedCount;
        }

        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Unnamed";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(name.Length);

            foreach (var ch in name.Trim())
            {
                if (ch == '/' || ch == '\\' || Array.IndexOf(invalidChars, ch) >= 0)
                {
                    builder.Append('_');
                }
                else
                {
                    builder.Append(ch);
                }
            }

            var sanitized = builder.ToString().Trim('.', ' ');
            return string.IsNullOrWhiteSpace(sanitized) ? "Unnamed" : sanitized;
        }
    }
}
