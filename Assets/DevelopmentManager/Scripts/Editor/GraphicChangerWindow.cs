using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace dev.GraphicChanger
{
    public class GraphicChangerWindow : EditorWindow
    {
        private RenderPipelineAsset urpAsset;
        private RenderPipelineAsset hdrpAsset;

        [MenuItem("Tools/Dev/Graphic Changer")]
        public static void ShowWindow()
        {
            GetWindow<GraphicChangerWindow>("Graphic Changer");
        }

        private void OnEnable()
        {
            // Load saved asset paths when opening the window
            LoadSavedAssets();
        }

        private void OnGUI()
        {
            GUILayout.Label("Render Pipeline Quick Changer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Assign your Render Pipeline assets below or use the Auto-Detect button.", MessageType.Info);
            EditorGUILayout.Space();

            // Asset fields
            urpAsset = (RenderPipelineAsset)EditorGUILayout.ObjectField("URP Asset", urpAsset, typeof(RenderPipelineAsset), false);
            hdrpAsset = (RenderPipelineAsset)EditorGUILayout.ObjectField("HDRP Asset", hdrpAsset, typeof(RenderPipelineAsset), false);

            if (GUILayout.Button("Auto-Detect Assets in Project", GUILayout.Height(25)))
            {
                AutoDetectAssets();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Switch to Built-in Render Pipeline (BIRP)
            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.5f);
            if (GUILayout.Button("Switch to Built-In (BIRP)", GUILayout.Height(40)))
            {
                SwitchToBuiltIn();
            }

            // Switch to Universal Render Pipeline (URP)
            GUI.backgroundColor = new Color(0.5f, 0.9f, 0.5f);
            if (GUILayout.Button("Switch to URP", GUILayout.Height(40)))
            {
                SwitchToURP();
            }

            // Switch to High Definition Render Pipeline (HDRP)
            GUI.backgroundColor = new Color(0.5f, 0.7f, 1.0f);
            if (GUILayout.Button("Switch to HDRP", GUILayout.Height(40)))
            {
                SwitchToHDRP();
            }
            GUI.backgroundColor = Color.white;
        }

        private void AutoDetectAssets()
        {
            // Search for URP asset in project
            string[] urpGuids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            if (urpGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(urpGuids[0]);
                urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            }

            // Search for HDRP asset in project
            string[] hdrpGuids = AssetDatabase.FindAssets("t:HDRenderPipelineAsset");
            if (hdrpGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(hdrpGuids[0]);
                hdrpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            }

            SaveAssets();
            Debug.Log("[GraphicChanger] Auto-detection completed successfully.");
        }

        private void SwitchToBuiltIn()
        {
            GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.renderPipeline = null;
            EditorUtility.DisplayDialog("Graphic Changer", "Successfully switched to Built-In Render Pipeline (BIRP)!", "OK");
            Debug.Log("[GraphicChanger] Active Pipeline: Built-In Render Pipeline");
        }

        private void SwitchToURP()
        {
            if (urpAsset == null)
            {
                EditorUtility.DisplayDialog("Error", "URP Asset is missing. Please assign it or use Auto-Detect.", "OK");
                return;
            }

            GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
            EditorUtility.DisplayDialog("Graphic Changer", "Successfully switched to Universal Render Pipeline (URP)!", "OK");
            Debug.Log("[GraphicChanger] Active Pipeline: URP");
        }

        private void SwitchToHDRP()
        {
            if (hdrpAsset == null)
            {
                EditorUtility.DisplayDialog("Error", "HDRP Asset is missing. Please assign it or use Auto-Detect.", "OK");
                return;
            }

            GraphicsSettings.defaultRenderPipeline = hdrpAsset;
            QualitySettings.renderPipeline = hdrpAsset;
            EditorUtility.DisplayDialog("Graphic Changer", "Successfully switched to High Definition Render Pipeline (HDRP)!", "OK");
            Debug.Log("[GraphicChanger] Active Pipeline: HDRP");
        }

        private void SaveAssets()
        {
            if (urpAsset != null)
                EditorPrefs.SetString("GraphicChanger_URP", AssetDatabase.GetAssetPath(urpAsset));
            if (hdrpAsset != null)
                EditorPrefs.SetString("GraphicChanger_HDRP", AssetDatabase.GetAssetPath(hdrpAsset));
        }

        private void LoadSavedAssets()
        {
            if (EditorPrefs.HasKey("GraphicChanger_URP"))
            {
                string path = EditorPrefs.GetString("GraphicChanger_URP");
                urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            }
            if (EditorPrefs.HasKey("GraphicChanger_HDRP"))
            {
                string path = EditorPrefs.GetString("GraphicChanger_HDRP");
                hdrpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            }
        }
    }
}