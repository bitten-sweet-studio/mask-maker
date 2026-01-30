using UnityEngine;

[DisallowMultipleComponent]
public class PaperSurface : MonoBehaviour
{
    [Header("Mask Setup")]
    [SerializeField] private Texture2D baseMaskTexture;
    [SerializeField] private string maskTexturePropertyName = "_MaskTex";

    private Texture2D runtimeMaskTexture;
    private Renderer cachedRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    public Texture2D RuntimeMaskTexture => runtimeMaskTexture;
    
    [Header("Fabric Interaction")]
    [SerializeField] private string useCutoutPropertyName = "_UseCutout";
    private bool isOnFabric = false;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer == null)
            cachedRenderer = GetComponentInChildren<Renderer>();

        // 🚨 ESSENCIAL: cria instância única do material
        cachedRenderer.material = new Material(cachedRenderer.sharedMaterial);

        materialPropertyBlock = new MaterialPropertyBlock();

        CreateRuntimeMaskIfNeeded();
        ApplyRuntimeMaskToRenderer();
    }

    public void CreateRuntimeMaskIfNeeded()
    {
        if (runtimeMaskTexture != null) return;

        if (baseMaskTexture == null)
        {
            Debug.LogError($"[{name}] baseMaskTexture não está setada no PaperSurface.", this);
            return;
        }

        runtimeMaskTexture = new Texture2D(baseMaskTexture.width, baseMaskTexture.height, TextureFormat.RGBA32, false);
        runtimeMaskTexture.SetPixels(baseMaskTexture.GetPixels());
        runtimeMaskTexture.Apply();
    }

    public void ApplyRuntimeMaskToRenderer()
    {
        if (cachedRenderer == null)
        {
            Debug.LogError($"[{name}] Não achei Renderer no objeto nem nos filhos.", this);
            return;
        }

        if (runtimeMaskTexture == null)
            return;

        cachedRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetTexture(maskTexturePropertyName, runtimeMaskTexture);
        cachedRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    public void SetOnFabric(bool value)
    {
        isOnFabric = value;

        cachedRenderer.material.SetFloat(useCutoutPropertyName, isOnFabric ? 1f : 0f);
    }

}