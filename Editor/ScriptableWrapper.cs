using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaruChanZ64.ScriptableBatchTool.Editor
{
    public class ScriptableWrapper : IDisposable
    {
        public ScriptableObject Instance { get; private set; }
        public SerializedObject SerializedObject { get; private set; }
        public string AssetName;

        private bool _disposed;

        public ScriptableWrapper(ScriptableObject instance, string defaultName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Instance = instance;
            SerializedObject = new SerializedObject(instance);
            AssetName = defaultName;
        }

        public void Dispose()
        {
            Dispose(destroyUnpersistedInstance: true);
        }

        public void Dispose(bool destroyUnpersistedInstance)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            SerializedObject?.Dispose();
            SerializedObject = null;

            if (destroyUnpersistedInstance && Instance != null && !EditorUtility.IsPersistent(Instance))
            {
                Object.DestroyImmediate(Instance);
            }

            Instance = null;
        }
    }
}
