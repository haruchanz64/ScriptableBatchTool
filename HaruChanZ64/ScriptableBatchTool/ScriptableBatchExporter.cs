using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HaruChanZ64.ScriptableBatchTool
{
    public static class ScriptableBatchExporter
    {
        public static void ExportAssets(List<ScriptableWrapper> items, string targetFolder)
        {
            foreach (var item in items)
            {
                var safeName = string.IsNullOrWhiteSpace(item.AssetName) ? "Unnamed" : item.AssetName;
                var fullPath = Path.Combine(targetFolder, safeName + ".asset");
                fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

                AssetDatabase.CreateAsset(item.Instance, fullPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All ScriptableObjects exported!");
        }
    }
}