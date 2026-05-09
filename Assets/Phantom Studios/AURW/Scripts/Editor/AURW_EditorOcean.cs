using UnityEngine;
using UnityEditor;

namespace aurw
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AURW_Ocean))]
    public class AURW_Editor : Editor
    {
        private MaterialEditor _materialEditor;
        private void OnDisable()
        {
            if (_materialEditor != null)
            {
                DestroyImmediate(_materialEditor);
            }
        }
        public override void OnInspectorGUI()
        {
            AURW_Ocean pc = (AURW_Ocean)target;
            base.OnInspectorGUI();
            EditorGUILayout.Space();
        }
    }
}