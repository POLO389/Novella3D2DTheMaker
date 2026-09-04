using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PostSovietEffect : MonoBehaviour
{
    [SerializeField] private Shader postSovietShader;
    
    private Material postProcessMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (postSovietShader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (postProcessMaterial == null)
        {
            postProcessMaterial = new Material(postSovietShader);
            postProcessMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        Graphics.Blit(source, destination, postProcessMaterial);
    }

    private void OnDisable()
    {
        if (postProcessMaterial != null)
        {
            DestroyImmediate(postProcessMaterial);
        }
    }
}