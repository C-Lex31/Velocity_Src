using UnityEngine;

[ExecuteInEditMode]
public class ScreenWarpEffect : MonoBehaviour
{
    public Material warpMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (warpMaterial != null)
        {
            Graphics.Blit(source, destination, warpMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
