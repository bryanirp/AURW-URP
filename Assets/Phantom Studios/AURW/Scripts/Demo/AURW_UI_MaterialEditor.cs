using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aurw
{
    public class AURW_UI_MaterialEditor : MonoBehaviour
    {
        #region Public Prop
        public Material material;
        public KeyCode enableUIKey = KeyCode.U;
        public bool enableUI = false;
        [Header("UI")]
        [Range(10, 100)] public int windowWidth = 30;
        [Range(10, 100)] public int windowHeight = 60;
        public GameObject father;
        public AURW_FreeCameraController freeCameraController;
        [Header("Tabs")]
        public GameObject surfaceTab;
        public GameObject normalTab;
        public GameObject foamTab;
        [Header("")]
        public TMP_InputField nearColorHexCode;
        public RawImage nearColorImage;
        public TMP_InputField waterBaseHexCode;
        public RawImage waterBaseImage;
        public TMP_InputField depthColorHexCode;
        public RawImage depthColorImage;
        public Slider smoothnesSlider;
        public TMP_Text smoothnessValue;
        public TMP_InputField specularColorHexCode;
        public RawImage specularColorImage;
        public Toggle RefractionToggle;
        public TMP_InputField refractionInput;
        public TMP_InputField stainsColorHexCode;
        public RawImage stainsColorImage;
        [Header("")]
        public Toggle enableNormals;
        public Button mainNormalButton;
        public RawImage mainNormalImage;
        public Button secondNormalButton;
        public RawImage secondNormalImage;
        public Button bigNormalButton;
        public RawImage bigNormalImage;
        public Slider normalStrengthSlider;
        public TMP_Text normalStrengthText;
        [Header("")]
        public TMP_InputField depthDistanceInput;
        public Slider foamDistanceSlider;
        public TMP_Text foamDistanceText;
        public Slider foamVolumeSlider;
        public TMP_Text foamVolumeValue;
        [Header("Normal Maps")]
        public Texture2D[] textures;
        public Texture2D[] textureSprites;
        #endregion
        /// Privates ///
        private int mainNormalStatus = 4;
        private int secondNormalStatus = 4;
        private int bigNormalStatus = 1;

        void Start()
        {
            SetUpPannel();
            ShowUI(enableUI);
            ActivateTab(surfaceTab);
            nearColorHexCode.onEndEdit.AddListener((miau) => UpdateColorFromHex(nearColorHexCode, nearColorImage, "_Water_Near_Color"));
            waterBaseHexCode.onEndEdit.AddListener((miau) => UpdateColorFromHex(waterBaseHexCode, waterBaseImage, "_Water_color"));
            depthColorHexCode.onEndEdit.AddListener((miau) => UpdateColorFromHex(depthColorHexCode, depthColorImage, "_Depth_Color"));
            specularColorHexCode.onEndEdit.AddListener((miau) => UpdateColorFromHex(specularColorHexCode, specularColorImage, "_Specular"));

            UpdateColorFromHex(nearColorHexCode, nearColorImage, "_Water_Near_Color");
            UpdateColorFromHex(waterBaseHexCode, waterBaseImage, "_Water_color");
            UpdateColorFromHex(depthColorHexCode, depthColorImage, "_Depth_Color");
            UpdateColorFromHex(specularColorHexCode, specularColorImage, "_Specular");

        }
        // Update is called once per frame
        void Update()
        {
            UpdateColors();
            Sliders();
            Toggle();
            InputsFields();
            if (Input.GetKeyUp(enableUIKey))
            {
                ShowUI(enableUI);
                enableUI = !enableUI;

            }
        }
        void ShowUI(bool enabled)
        {
            if (father != null)
            {
                freeCameraController.LockMouse(!enabled);
                father.SetActive(enabled);
            }
            else
            {
                Debug.LogWarning("No father UI assigned");
                return;
            }
        }
        void SetUpPannel()
        {

            if (father != null)
            {
                RectTransform rt = father.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float width = (windowWidth / 100f) * Screen.width;
                    float height = (windowHeight / 100f) * Screen.height;

                    rt.sizeDelta = new Vector2(width, height);

                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                }
                else
                {
                    Debug.LogWarning("Father object does not have a RectTransform component");
                }
            }
        }
        public void UpdateColors()
        {
            if (nearColorHexCode.isFocused && Input.GetKeyDown(KeyCode.Return))
                UpdateColorFromHex(nearColorHexCode, nearColorImage, "_Water_Near_Color");

            if (waterBaseHexCode.isFocused && Input.GetKeyDown(KeyCode.Return))
                UpdateColorFromHex(waterBaseHexCode, waterBaseImage, "_Water_color");

            if (depthColorHexCode.isFocused && Input.GetKeyDown(KeyCode.Return))
                UpdateColorFromHex(depthColorHexCode, depthColorImage, "_Depth_Color");

            if (specularColorHexCode.isFocused && Input.GetKeyDown(KeyCode.Return))
                UpdateColorFromHex(specularColorHexCode, specularColorImage, "_Specular");
        }
        private void UpdateColorFromHex(TMP_InputField inputField, RawImage colorImage, string shaderProperty)
        {
            if (inputField.text.Length == 8)
            {
                Color newColor;
                if (ColorUtility.TryParseHtmlString("#" + inputField.text, out newColor))
                {
                    if (colorImage != null)
                        colorImage.color = newColor;

                    if (material != null)
                        material.SetColor(shaderProperty, newColor);
                }
            }
        }

        public void ActivateTab(GameObject tabToActivate)
        {
            surfaceTab.SetActive(false);
            normalTab.SetActive(false);
            foamTab.SetActive(false);

            tabToActivate.SetActive(true);
        }
        public void SetTexture(int textureType)
        {
            switch (textureType)
            {
                case 0: // Main Normal
                    mainNormalStatus = ValidateNumberTexture(mainNormalStatus);
                    ApplyTexture(mainNormalStatus, "_Main_Normal", mainNormalImage);
                    break;

                case 1: // Second Normal
                    secondNormalStatus = ValidateNumberTexture(secondNormalStatus);
                    ApplyTexture(secondNormalStatus, "_Second_Normal", secondNormalImage);
                    break;

                case 2: // Big Normal
                    bigNormalStatus = ValidateNumberTexture(bigNormalStatus);
                    ApplyTexture(bigNormalStatus, "_Big_Normal", bigNormalImage);
                    break;
            }
        }

        private void ApplyTexture(int textureIndex, string shaderProperty, RawImage targetImage)
        {
            if (textures == null || textureSprites == null ||
               textures.Length == 0 || textureSprites.Length == 0)
            {
                Debug.LogError("Texture arrays not properly initialized!");
                return;
            }

            int safeIndex = Mathf.Clamp(textureIndex, 0, Mathf.Min(textures.Length, textureSprites.Length) - 1);

            if (material != null)
            {
                material.SetTexture(shaderProperty, textures[safeIndex]);
            }

            if (targetImage != null)
            {
                targetImage.texture = textureSprites[safeIndex];
            }
        }

        private int ValidateNumberTexture(int currentStatus)
        {
            int newStatus = currentStatus + 1;

            // Usamos el mínimo entre los dos arrays para asegurar compatibilidad
            int maxIndex = Mathf.Min(textures.Length, textureSprites.Length) - 1;

            if (newStatus > maxIndex)
            {
                newStatus = 0;
            }

            return newStatus;
        }
        private void Sliders()
        {
            smoothnessValue.text = (smoothnesSlider.value / 100).ToString();
            material.SetFloat("_Smoothness", smoothnesSlider.value / 100);

            normalStrengthText.text = (normalStrengthSlider.value / 100).ToString();
            material.SetFloat("_Normal_Strength", normalStrengthSlider.value / 100);

            foamDistanceText.text = (foamDistanceSlider.value).ToString();
            material.SetFloat("_Foam_Distance", foamDistanceSlider.value);

            foamVolumeValue.text = (foamVolumeSlider.value / 100).ToString();
            material.SetFloat("_Foam_Volume", foamVolumeSlider.value / 100);
        }
        private void Toggle()
        {
            if (material.IsKeywordEnabled("_NORMAL") != enableNormals.isOn)
            {
                if (enableNormals.isOn)
                    material.EnableKeyword("_NORMAL");
                else
                    material.DisableKeyword("_NORMAL");
            }

            if (material.IsKeywordEnabled("_REFRACTION") != RefractionToggle.isOn)
            {
                if (RefractionToggle.isOn)
                    material.EnableKeyword("_REFRACTION");
                else
                    material.DisableKeyword("_REFRACTION");
            }
        }
        private void InputsFields()
        {
            string iorText = refractionInput.text.Replace(',', '.');
            string depthText = depthDistanceInput.text.Replace(',', '.');

            float iorValue;
            float depthValue;

            if (float.TryParse(iorText, NumberStyles.Float, CultureInfo.InvariantCulture, out iorValue))
            {
                material.SetFloat("_IOR", iorValue);
            }

            if (float.TryParse(depthText, NumberStyles.Float, CultureInfo.InvariantCulture, out depthValue))
            {
                material.SetFloat("_Depth_Distance", depthValue);
            }
        }
    }
}