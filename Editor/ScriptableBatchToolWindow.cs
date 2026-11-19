using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HaruChanZ64.ScriptableBatchTool.Editor
{
    public class ScriptableBatchToolWindow : EditorWindow
    {
        [SerializeField] private MonoScript _scriptReference;
        [SerializeField] private string _targetFolder = "Assets";

        private Type _targetType;
        private readonly List<ScriptableWrapper> _items = new();
        private int _nextItemIndex = 1;

        private Vector2 _scroll;

        [MenuItem("Tools/ScriptableObject Batch Creator")]
        private static void Init()
        {
            var window = GetWindow<ScriptableBatchToolWindow>("ScriptableObject Batch Creator");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            if (_scriptReference != null)
            {
                if (!TryGetValidScriptableObjectType(_scriptReference, out _targetType, out _))
                {
                    _scriptReference = null;
                    _targetType = null;
                }
            }

            if (string.IsNullOrWhiteSpace(_targetFolder))
            {
                _targetFolder = "Assets";
            }
        }

        private void OnDestroy()
        {
            ClearItems();
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

            var newScriptReference = EditorGUILayout.ObjectField(
                "Script",
                _scriptReference,
                typeof(MonoScript),
                false
            ) as MonoScript;

            if (newScriptReference != _scriptReference)
            {
                if (newScriptReference != null)
                {
                    if (TryGetValidScriptableObjectType(newScriptReference, out var newType, out var errorMessage))
                    {
                        var shouldApply = true;

                        if (_targetType != null && newType != _targetType && _items.Count > 0)
                        {
                            shouldApply = EditorUtility.DisplayDialog(
                                "Change ScriptableObject Type?",
                                "Changing the ScriptableObject class will remove all current entries.\n\nContinue?",
                                "Yes, Change Type",
                                "Cancel"
                            );

                            if (shouldApply)
                            {
                                ClearItems();
                            }
                        }

                        if (shouldApply)
                        {
                            _scriptReference = newScriptReference;
                            _targetType = newType;
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Selection", errorMessage, "OK");
                    }
                }
                else
                {
                    var shouldClear = true;

                    if (_items.Count > 0)
                    {
                        shouldClear = EditorUtility.DisplayDialog(
                            "Clear Script Selection?",
                            "Clearing the Script selection will remove all current entries.\n\nContinue?",
                            "Yes, Clear",
                            "Cancel"
                        );
                    }

                    if (shouldClear)
                    {
                        _scriptReference = null;
                        _targetType = null;
                        ClearItems();
                    }
                }
            }

            if (_scriptReference == null)
            {
                EditorGUILayout.HelpBox("No ScriptableObject assigned yet!", MessageType.None);
            }
            else if (_targetType != null)
            {
                EditorGUILayout.HelpBox("Target Type Set: " + _targetType.Name, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Selected script is not a valid ScriptableObject!", MessageType.Error);
            }

            EditorGUILayout.Space();
        }

        private void DrawTargetFolder()
        {
            EditorGUILayout.LabelField("2. Choose Export Folder", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Select Output Folder"))
            {
                var selected = EditorUtility.OpenFolderPanel("Choose Output Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (TryConvertToAssetPath(selected, out var assetRelativePath))
                    {
                        _targetFolder = assetRelativePath;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(
                            "Invalid Folder",
                            "Selected folder must be inside the project's Assets folder.",
                            "OK"
                        );
                    }
                }
            }

            EditorGUILayout.LabelField("Target Folder: ", _targetFolder);
            EditorGUILayout.Space();
        }

        private void DrawItemControls()
        {
            if (_targetType == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Add ScriptableObject Entries", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Add New Entry", GUILayout.Height(25)))
            {
                var instance = ScriptableObject.CreateInstance(_targetType);
                if (instance == null)
                {
                    Debug.LogError($"Failed to create an instance of '{_targetType.FullName}'.");
                    return;
                }

                _items.Add(new ScriptableWrapper(instance, $"{_targetType.Name}_{_nextItemIndex++}"));
            }

            EditorGUILayout.Space();
        }

        private void DrawItemList()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            var removeIndex = -1;

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == null || item.Instance == null || item.SerializedObject == null)
                {
                    if (removeIndex < 0)
                    {
                        removeIndex = i;
                    }
                    continue;
                }

                EditorGUILayout.BeginVertical("box");

                var typeName = _targetType != null ? _targetType.Name : item.Instance.GetType().Name;
                EditorGUILayout.LabelField($"{typeName} #{i + 1}", EditorStyles.boldLabel);

                item.AssetName = EditorGUILayout.TextField("Asset Name", item.AssetName);

                var serialized = item.SerializedObject;
                serialized.Update();

                var prop = serialized.GetIterator();
                var enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (prop.name == "m_Script")
                    {
                        continue;
                    }

                    EditorGUILayout.PropertyField(prop, true);
                }

                serialized.ApplyModifiedProperties();

                EditorGUILayout.Space();
                if (GUILayout.Button("Remove"))
                {
                    removeIndex = i;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            if (removeIndex >= 0 && removeIndex < _items.Count)
            {
                _items[removeIndex]?.Dispose();
                _items.RemoveAt(removeIndex);
            }
        }

        private void DrawExportButton()
        {
            if (_items.Count <= 0)
            {
                return;
            }

            EditorGUILayout.LabelField($"4. Export to {_targetFolder}", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Export All", GUILayout.Height(30)))
            {
                if (!AssetDatabase.IsValidFolder(_targetFolder))
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Output Folder",
                        $"The folder '{_targetFolder}' does not exist. Please select a valid folder inside Assets.",
                        "OK"
                    );
                    return;
                }

                var exportedCount = ScriptableBatchExporter.ExportAssets(_items, _targetFolder);
                if (exportedCount > 0)
                {
                    ClearItems(destroyUnpersistedInstances: false);
                }
            }
        }

        private void ClearItems(bool destroyUnpersistedInstances = true)
        {
            foreach (var item in _items)
            {
                item?.Dispose(destroyUnpersistedInstances);
            }

            _items.Clear();
            _nextItemIndex = 1;
        }

        private static bool TryGetValidScriptableObjectType(MonoScript script, out Type validType, out string errorMessage)
        {
            validType = null;
            errorMessage = null;

            if (script == null)
            {
                errorMessage = "Selected script is null.";
                return false;
            }

            var candidateType = script.GetClass();
            if (candidateType == null || !candidateType.IsSubclassOf(typeof(ScriptableObject)))
            {
                errorMessage = "Selected script must extend ScriptableObject.";
                return false;
            }

            if (candidateType.IsAbstract)
            {
                errorMessage = "Selected ScriptableObject class is abstract and cannot be instantiated.";
                return false;
            }

            if (candidateType.IsGenericTypeDefinition || candidateType.ContainsGenericParameters)
            {
                errorMessage = "Selected ScriptableObject class is generic and cannot be instantiated.";
                return false;
            }

            if (typeof(EditorWindow).IsAssignableFrom(candidateType) || typeof(UnityEditor.Editor).IsAssignableFrom(candidateType))
            {
                errorMessage = "Editor and EditorWindow scripts cannot be exported as ScriptableObject assets.";
                return false;
            }

            validType = candidateType;
            return true;
        }

        private static bool TryConvertToAssetPath(string absolutePath, out string assetPath)
        {
            assetPath = null;

            var normalizedDataPath = Path.GetFullPath(Application.dataPath).Replace('\\', '/').TrimEnd('/');
            var normalizedSelected = Path.GetFullPath(absolutePath).Replace('\\', '/').TrimEnd('/');

            if (string.Equals(normalizedSelected, normalizedDataPath, StringComparison.OrdinalIgnoreCase))
            {
                assetPath = "Assets";
                return true;
            }

            if (normalizedSelected.StartsWith(normalizedDataPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                assetPath = "Assets" + normalizedSelected.Substring(normalizedDataPath.Length);
                return true;
            }

            return false;
        }
    }
}
