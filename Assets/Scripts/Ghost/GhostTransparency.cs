using UnityEngine;
using UnityEngine.Rendering;

public class GhostTransparency : MonoBehaviour
{
    [Range(0f, 1f)]
    public float transparency = 0.2f;

    private Renderer[] renderers;

    private void Awake()
    {
        // Get all renderers on the ghost
        renderers = GetComponentsInChildren<Renderer>();

        // Set material transparency at the start
        SetURPTransparency(transparency);
    }

    private void SetURPTransparency(float alpha)
    {
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                // Check if the shader is compatible with URP
                if (!mat.shader.name.Contains("Universal Render Pipeline"))
                {
                    // Switch to a URP Lit shader in Transparent mode
                    mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                    mat.SetFloat("_Surface", 1); // Set to Transparent mode
                    mat.SetFloat("_Blend", (float)BlendMode.OneMinusSrcAlpha); // Use alpha blending
                }

                // Configure material for transparency
                mat.SetFloat("_Surface", 1); // Set to Transparent
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0); // Disable depth writing for proper transparency sorting
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;

                // Set color with transparency
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;
            }
        }
    }
}
