#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace aurw
{
    public class AURW_FreeShaderGUI : ShaderGUI
    {
        private int _selectedTab = 0;
        private readonly string[] _tabs = { "Surface", "Normals", "Foam" };

        private Color tabBgColor = new Color(0.141f, 0.145f, 0.149f);
        private Color tabActiveColor = new Color(0.3f, 0.52f, 0.82f, 0.3f);
        private Color sectionBgColor = new Color(0.18f, 0.18f, 0.19f, 0.95f);
        private Color lineColor = new Color(0.27f, 0.28f, 0.3f, 1f);
        private Color labelTextColor = new Color(0.82f, 0.84f, 0.87f);
        private Color headerColor = new Color(0.87f, 0.89f, 0.93f);

        public override void OnGUI(MaterialEditor editor, MaterialProperty[] props)
        {
            Material mat = editor.target as Material;

            DrawHeader();
            DrawTabSelector();

            EditorGUILayout.Space(10);
            DrawLine();

            EditorGUILayout.Space(8);
            DrawCard(() =>
            {
                switch (_selectedTab)
                {
                    case 0: DrawSurfaceTab(editor, props, mat); break;
                    case 1: DrawNormalsTab(editor, props, mat); break;
                    case 2: DrawFoamTab(editor, props); break;
                }
            });

            EditorGUILayout.Space(14);
            DrawLine();

            EditorGUILayout.Space(8);
            DrawCard(() =>
            {
                DrawSectionTitle("Animation Settings");
                editor.ShaderProperty(Find("_Tilling", props), "Tiling");
                editor.ShaderProperty(Find("_Speed", props), "Speed");
                editor.ShaderProperty(Find("_Speed_Factor", props), "Speed Factor");

                var direction = Find("_Direction", props);
                Vector2 dir = new Vector2(direction.vectorValue.x, direction.vectorValue.y);
                dir = EditorGUILayout.Vector2Field("Direction (XY)", dir);
                direction.vectorValue = new Vector4(dir.x, dir.y, 0, 0);

                editor.ShaderProperty(Find("_Second_Direction_Factor", props), "Secondary Direction Factor");
            });
        }

        private void DrawHeader()
        {
            GUILayout.Space(8);
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = headerColor }
            };
            EditorGUILayout.LabelField("AURW Water Shader", style);
            GUILayout.Space(6);
        }

        private void DrawTabSelector()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < _tabs.Length; i++)
            {
                bool isSelected = (_selectedTab == i);

                GUIStyle tabStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    fixedHeight = 26,
                    margin = new RectOffset(2, 2, 2, 2),
                    padding = new RectOffset(12, 12, 4, 4),
                    normal = { textColor = isSelected ? Color.white : labelTextColor }
                };

                Rect rect = GUILayoutUtility.GetRect(new GUIContent(_tabs[i]), tabStyle, GUILayout.ExpandWidth(true));

                EditorGUI.DrawRect(rect, isSelected ? tabActiveColor : tabBgColor);

                if (rect.Contains(Event.current.mousePosition) && !isSelected)
                {
                    EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.05f));
                    EditorApplication.QueuePlayerLoopUpdate();
                }

                if (GUI.Button(rect, _tabs[i], tabStyle))
                    _selectedTab = i;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSurfaceTab(MaterialEditor editor, MaterialProperty[] props, Material mat)
        {
            DrawSection("Colors", () =>
            {
                editor.ShaderProperty(Find("_Water_Near_Color", props), "Near Color");
                editor.ShaderProperty(Find("_Water_color", props), "Base Color");
                editor.ShaderProperty(Find("_Depth_Color", props), "Depth Color");
            });

            DrawSection("Surface Properties", () =>
            {
                editor.ShaderProperty(Find("_Smoothness", props), "Smoothness");
                editor.ShaderProperty(Find("_Specular", props), "Specular");
            });

            DrawSection("Refraction", () =>
            {
                editor.ShaderProperty(Find("_REFRACTION", props), "Enabled");
                if (mat.IsKeywordEnabled("_REFRACTION"))
                {
                    editor.ShaderProperty(Find("_IOR", props), "IOR");
                    editor.ShaderProperty(Find("_Refraction_Strength", props), "Refraction Strength");
                }
            });

            DrawSection("Stains", () =>
            {
                editor.ShaderProperty(Find("_Stain_scale", props), "Scale");
                editor.ShaderProperty(Find("_Stain_color", props), "Color");
            });
        }

        private void DrawNormalsTab(MaterialEditor editor, MaterialProperty[] props, Material mat)
        {
            editor.ShaderProperty(Find("_NORMAL", props), "Enabled");
            if (!mat.IsKeywordEnabled("_NORMAL")) return;

            DrawSection("Normal Maps", () =>
            {
                editor.TextureProperty(Find("_Main_Normal", props), "Main Normal");
                editor.TextureProperty(Find("_Second_Normal", props), "Secondary Normal");
                editor.TextureProperty(Find("_Big_Normal", props), "Big Normal");
            });

            DrawSection("Controls", () =>
            {
                editor.ShaderProperty(Find("_Normal_Strength", props), "Strength");
                editor.ShaderProperty(Find("_Big_Normal_Fix", props), "Big Scale");
            });
        }

        private void DrawFoamTab(MaterialEditor editor, MaterialProperty[] props)
        {
            DrawSection("Foam Settings", () =>
            {
                editor.ShaderProperty(Find("_Depth_Distance", props), "Depth Distance");
                editor.ShaderProperty(Find("_Foam_Distance", props), "Distance (%)");
                editor.ShaderProperty(Find("_Foam_Color", props), "Color (HDR)");
                editor.ShaderProperty(Find("_Foam_Volume", props), "Volume");
            });

            DrawSection("Advanced Foam", () =>
            {
                editor.ShaderProperty(Find("_Foam_Scale", props), "Scale");
                editor.ShaderProperty(Find("_Foam_Speed_Fix", props), "Speed");
                editor.ShaderProperty(Find("_Foam_Tilling_Fix", props), "Tiling Fix");
                editor.ShaderProperty(Find("_Foam_Seed", props), "Seed");
            });
        }

        private void DrawSection(string title, System.Action body)
        {
            GUILayout.Space(6);
            EditorGUILayout.BeginVertical();
            DrawSectionTitle(title);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), lineColor);
            DrawCard(body);
            EditorGUILayout.EndVertical();
        }

        private void DrawSectionTitle(string title)
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                normal = { textColor = labelTextColor }
            };
            EditorGUILayout.LabelField(title, titleStyle);
        }

        private void DrawLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, lineColor);
        }

        private GUIStyle _cardStyle;
        private void InitCardStyle()
        {
            if (_cardStyle == null)
            {
                _cardStyle = new GUIStyle(EditorStyles.helpBox);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, sectionBgColor);
                tex.Apply();
                _cardStyle.normal.background = tex;
                _cardStyle.padding = new RectOffset(10, 10, 8, 8);
            }
        }
        private void DrawCard(System.Action body)
        {
            InitCardStyle();
            EditorGUILayout.BeginVertical(_cardStyle);
            body?.Invoke();
            EditorGUILayout.EndVertical();
        }

        private MaterialProperty Find(string name, MaterialProperty[] props)
        {
            return FindProperty(name, props);
        }
    }
}
#endif
