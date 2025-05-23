using UnityEditor;
using UnityEngine;

namespace HaruChanZ64.ScriptableBatchTool
{
    public class ScriptableWrapper
    {
        public readonly ScriptableObject Instance;
        public readonly SerializedObject SerializedObject;
        public string AssetName;

        public ScriptableWrapper(ScriptableObject instance, string defaultName)
        {
            Instance = instance;
            SerializedObject = new SerializedObject(instance);
            AssetName = defaultName;
        }
    }
}