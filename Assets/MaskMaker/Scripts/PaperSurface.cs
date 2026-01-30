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
    
    [Header("Fabric Capture")]
    [SerializeField] private string fabricTexturePropertyName = "_FabricTex";
    [SerializeField] private int fabricTextureResolution = 1024;

    private RenderTexture fabricRenderTexture;

    [Header("Paper State")]
    [SerializeField] private PaperState currentPaperState = PaperState.Normal;
    [SerializeField] private string normalKeyword  = "NORMAL";
    [SerializeField] private string cutoutKeyword  = "CUTOUT";
    [SerializeField] private string stampedKeyword = "STAMPED";
    public PaperState CurrentPaperState => currentPaperState;
    public void SetPaperState(PaperState newState) => SetVisualState(newState);


    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer == null)
            cachedRenderer = GetComponentInChildren<Renderer>();

        // material único por papel
        cachedRenderer.material = new Material(cachedRenderer.sharedMaterial);

        materialPropertyBlock = new MaterialPropertyBlock();

        CreateRuntimeMaskIfNeeded();
        ApplyRuntimeMaskToRenderer();

        // 🔥 ESSENCIAL: aplica estado inicial
        SetVisualState(currentPaperState);
    }
    public void CaptureFabricTexture(Camera captureCamera, LayerMask fabricLayerMask)
{
    if (captureCamera == null || cachedRenderer == null)
        return;

    EnsureFabricRenderTexture();

    // Normal do papel (mude pra transform.forward se o seu plano usa outro eixo)
    Vector3 paperNormal = transform.forward;
    
    Bounds paperBoundsWorld = cachedRenderer.bounds;

    // Tamanho do papel no plano (pega o maior extent "lateral")
    float halfSize = Mathf.Max(paperBoundsWorld.extents.x, paperBoundsWorld.extents.y);

    // Configura câmera ortográfica 1:1 (porque RT é quadrada)
    captureCamera.orthographic = true;
    captureCamera.aspect = 1f;
    captureCamera.orthographicSize = halfSize;

    // Posiciona a câmera um pouco acima do papel olhando "para baixo" no normal
    float distance = 1.0f; // pode ser pequeno, só precisa estar acima
    captureCamera.transform.position = paperBoundsWorld.center + paperNormal * distance;
    captureCamera.transform.rotation = Quaternion.LookRotation(-paperNormal, transform.forward);

    // Só renderiza o tecido
    captureCamera.cullingMask = fabricLayerMask;

    // Renderiza para a RT
    captureCamera.targetTexture = fabricRenderTexture;
    captureCamera.Render();
    captureCamera.targetTexture = null;

    // Aplica no renderer do papel via PropertyBlock (consistente com sua máscara)
    cachedRenderer.GetPropertyBlock(materialPropertyBlock);
    materialPropertyBlock.SetTexture(fabricTexturePropertyName, fabricRenderTexture);
    cachedRenderer.SetPropertyBlock(materialPropertyBlock);
}

private void EnsureFabricRenderTexture()
{
    if (fabricRenderTexture != null) return;

    fabricRenderTexture = new RenderTexture(
        fabricTextureResolution,
        fabricTextureResolution,
        0,
        RenderTextureFormat.ARGB32
    );

    fabricRenderTexture.wrapMode = TextureWrapMode.Clamp;
    fabricRenderTexture.filterMode = FilterMode.Bilinear;
    fabricRenderTexture.useMipMap = false;
    fabricRenderTexture.Create();
}

    public void CreateRuntimeMaskIfNeeded()
    {
        if (runtimeMaskTexture != null) return;

        if (baseMaskTexture == null)
        {
            Debug.LogError($"[{name}] baseMaskTexture não está setada no PaperSurface.", this);
            return;
        }

        runtimeMaskTexture = new Texture2D(
            baseMaskTexture.width,
            baseMaskTexture.height,
            TextureFormat.RGBA32,
            false
        );

        runtimeMaskTexture.SetPixels(baseMaskTexture.GetPixels());
        runtimeMaskTexture.Apply();
    }

    public void ApplyRuntimeMaskToRenderer()
    {
        if (cachedRenderer == null || runtimeMaskTexture == null)
            return;

        cachedRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetTexture(maskTexturePropertyName, runtimeMaskTexture);
        cachedRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    public void SetOnFabric(bool value)
    {
        SetVisualState(value ? PaperState.Cutout : PaperState.Normal);
    }

    private void SetVisualState(PaperState newState)
    {
        if (currentPaperState == newState)
            return;

        currentPaperState = newState;

        Material materialInstance = cachedRenderer.material;

        materialInstance.DisableKeyword(normalKeyword);
        materialInstance.DisableKeyword(cutoutKeyword);
        materialInstance.DisableKeyword(stampedKeyword);

        switch (currentPaperState)
        {
            case PaperState.Normal:
                materialInstance.EnableKeyword(normalKeyword);
                break;

            case PaperState.Cutout:
                materialInstance.EnableKeyword(cutoutKeyword);
                break;

            case PaperState.Stamped:
                materialInstance.EnableKeyword(stampedKeyword);
                break;
        }
    }
}

public enum PaperState
{
    Normal,
    Cutout,
    Stamped
}
