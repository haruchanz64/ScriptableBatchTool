using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaruChanZ64.ScriptableBatchTool.Editor
{
    public class ScriptableBatchToolWindow : EditorWindow
    {
        private Type _targetType;
        private Object _scriptReference;
        private Object _previousScriptReference;
        private string _targetFolder = "Assets";
        private readonly List<ScriptableWrapper> _items = new();

        private Vector2 _scroll;

        [MenuItem("Tools/ScriptableObject Batch Creator")]
        private static void Init()
        {
            var window = GetWindow<ScriptableBatchToolWindow>("ScriptableObject Batch Creator");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            DrawScriptSelector();
            DrawTargetFolder();
            DrawItemControls();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("3. Item List", EditorStyles.miniBoldLabel);
            DrawItemList();

            EditorGUILayout.Space();
            DrawExportButton();

            EditorGUILayout.EndVertical();
        }

        private void DrawScriptSelector()
        {
            EditorGUILayout.LabelField("1. Select a ScriptableObject class", EditorStyles.miniBoldLabel);
            
            var newScriptReference = EditorGUILayout.ObjectField("Script", _scriptReference, typeof(MonoScript), false);

            if (newScriptReference != _scriptReference)
            {
                // User changed script selection - validate it
                if (newScriptReference is MonoScript newMonoScript)
                {
                    var newType = newMonoScript.GetClass();

                    if (newType != null && newType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        // Check if switching type while having existing entries
                        if (_targetType != null && newType != _targetType && _items.Count > 0)
                        {
                            var confirmed = EditorUtility.DisplayDialog(
                                "Change ScriptableObject Type?",
                                "Changing the ScriptableObject class will remove all current entries.\n\nContinue?",
                                "Yes, Change Type",
                                "Cancel"
                            );

                            if (!confirmed)
                            {
                                return;
                            }

                            _items.Clear();
                        }

                        // Accept the new valid type
                        _scriptReference = newScriptReference;
                        _previousScriptReference = _scriptReference;
                        _targetType = newType;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Selection", "Selected script must extend ScriptableObject.", "OK");
                        // Revert to previous
                        _scriptReference = _previousScriptReference;
                    }
                }
                else if (!newScriptReference)
                {
                    // Reset all values
                    if (_items.Count > 0)
                    {
                        var confirmed = EditorUtility.DisplayDialog(
                            "Clear Script Selection?",
                            "Clearing the Script selection will remove all current entries.\n\nContinue?",
                            "Yes, Clear",
                            "Cancel"
                        );

                        if (!confirmed)
                        {
                            // Revert
                            return;
                        }
                    }

                    _scriptReference = null;
                    _previousScriptReference = null;
                    _targetType = null;
                    _items.Clear();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Selection", "Selected script must be a MonoScript derived from ScriptableObject.", "OK");
                    _scriptReference = _previousScriptReference;
                }
            }

            if (!_scriptReference)
            {
                EditorGUILayout.HelpBox("No ScriptableObject assigned yet!", MessageType.None);
            }
            else if (_targetType != null)
            {
                EditorGUILayout.HelpBox("Target Type Set: " + _targetType.Name, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Selected script is not a ScriptableObject!", MessageType.Error);
            }


            EditorGUILayout.Space();
        }
        
        private void DrawTargetFolder()
        {
            EditorGUILayout.LabelField("2. Choose Export Folder", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Select Output Folder"))
            {
                var selected = EditorUtility.OpenFolderPanel("Choose Output Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                {
                    _targetFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogError("Selected folder must be inside the Assets folder.");
                }
            }

            EditorGUILayout.LabelField("Target Folder: ", _targetFolder);
            EditorGUILayout.Space();
        }

        private void DrawItemControls()
        {
            if (_targetType == null) return;

            EditorGUILayout.LabelField("Add ScriptableObject Entries", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Add New Entry", GUILayout.Height(25)))
            {
                var instance = ScriptableObject.CreateInstance(_targetType);
                _items.Add(new ScriptableWrapper(instance, $"{_targetType.Name}_{_items.Count + 1}"));
            }

            EditorGUILayout.Space();
        }

        private void DrawItemList()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{_targetType.Name} #{i + 1}", EditorStyles.boldLabel);

                item.AssetName = EditorGUILayout.TextField("Asset Name", item.AssetName);

                var serialized = item.SerializedObject;
                serialized.Update();

                var prop = serialized.GetIterator();
                prop.NextVisible(true); // Skip script field

                while (prop.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(prop, true);
                }

                serialized.ApplyModifiedProperties();

                EditorGUILayout.Space();
                if (GUILayout.Button("Remove"))
                {
                    _items.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawExportButton()
        {
            if (_items.Count <= 0) return;
            EditorGUILayout.LabelField($"4. Export to {_targetFolder}", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Export All", GUILayout.Height(30)))
            {
                ScriptableBatchExporter.ExportAssets(_items, _targetFolder);
            }
        }
    }
}
