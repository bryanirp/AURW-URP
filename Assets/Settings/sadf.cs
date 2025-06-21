using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class asdf : MonoBehaviour
{
    public Material mat;

    void Update()
    {
        if (mat != null)
            mat.SetTexture("_BaseMap", Shader.GetGlobalTexture("_AURW_RefractionScene"));
    }
}