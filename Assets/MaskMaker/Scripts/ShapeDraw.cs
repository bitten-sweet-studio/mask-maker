using UnityEngine;

public class ShapeDraw : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera sceneCamera;

    [Header("Textures")]
    [SerializeField] private Texture2D baseMaskTexture;
    [SerializeField] private Texture2D brushTexture;

    [Header("Shader Settings")]
    [SerializeField] private string maskTexturePropertyName = "_MaskTex";

    [Header("Brush Settings")]
    [SerializeField] private int brushRadiusInPixels = 24;
    [SerializeField, Range(0f, 1f)] private float brushStrength = 1f;

    [Header("Raycast")]
    [SerializeField] private LayerMask paintableLayers = ~0;

    [Header("Paint Mode (read-only in Inspector)")]
    [SerializeField] private Color currentPaintColor = Color.white;

    private Texture2D runtimeMaskTexture;
    private MaterialPropertyBlock materialPropertyBlock;

    private Renderer cachedHitRenderer;
    private Collider cachedHitCollider;

    private Vector3 previousMousePosition;
    private bool hasPreviousMousePosition;

    private void Awake()
    {
        if (sceneCamera == null)
            sceneCamera = Camera.main;

        materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        CreateRuntimeMaskTexture();
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            hasPreviousMousePosition = false;
            return;
        }

        Vector3 currentMousePosition = Input.mousePosition;

        if (hasPreviousMousePosition && currentMousePosition == previousMousePosition)
            return;

        previousMousePosition = currentMousePosition;
        hasPreviousMousePosition = true;

        if (!TryGetRaycastHit(currentMousePosition, out RaycastHit raycastHit))
            return;

        CacheRendererFromHit(raycastHit);

        if (cachedHitRenderer == null)
            return;

        PaintAtHitPoint(raycastHit);
        ApplyMaskTextureToCachedRenderer();
    }

    // --- Public API (Buttons) ---

    public void SetDrawing()
    {
        currentPaintColor = Color.white;
    }

    public void SetErasing()
    {
        currentPaintColor = Color.black;
    }

    // --- Internal ---

    private void CreateRuntimeMaskTexture()
    {
        runtimeMaskTexture = new Texture2D(
            baseMaskTexture.width,
            baseMaskTexture.height,
            TextureFormat.RGBA32,
            false
        );

        runtimeMaskTexture.SetPixels(baseMaskTexture.GetPixels());
        runtimeMaskTexture.Apply();
    }

    private bool TryGetRaycastHit(Vector3 mouseScreenPosition, out RaycastHit raycastHit)
    {
        Ray ray = sceneCamera.ScreenPointToRay(mouseScreenPosition);
        return Physics.Raycast(ray, out raycastHit, Mathf.Infinity, paintableLayers, QueryTriggerInteraction.Ignore);
    }

    private void CacheRendererFromHit(RaycastHit raycastHit)
    {
        if (raycastHit.collider == cachedHitCollider && cachedHitRenderer != null)
            return;

        cachedHitCollider = raycastHit.collider;
        cachedHitRenderer = cachedHitCollider.GetComponent<Renderer>();
    }

    private void PaintAtHitPoint(RaycastHit raycastHit)
    {
        Vector2 hitTextureCoordinates = raycastHit.textureCoord;

        int hitPixelX = Mathf.FloorToInt(hitTextureCoordinates.x * runtimeMaskTexture.width);
        int hitPixelY = Mathf.FloorToInt(hitTextureCoordinates.y * runtimeMaskTexture.height);

        int paintStartPixelX = hitPixelX - brushRadiusInPixels;
        int paintStartPixelY = hitPixelY - brushRadiusInPixels;

        int brushDiameterInPixels = brushRadiusInPixels * 2;

        for (int brushPixelOffsetX = 0; brushPixelOffsetX < brushDiameterInPixels; brushPixelOffsetX++)
        {
            for (int brushPixelOffsetY = 0; brushPixelOffsetY < brushDiameterInPixels; brushPixelOffsetY++)
            {
                int targetPixelX = paintStartPixelX + brushPixelOffsetX;
                int targetPixelY = paintStartPixelY + brushPixelOffsetY;

                if (!IsInsideTextureBounds(runtimeMaskTexture, targetPixelX, targetPixelY))
                    continue;

                float brushSampleU = brushPixelOffsetX / (float)(brushDiameterInPixels - 1);
                float brushSampleV = brushPixelOffsetY / (float)(brushDiameterInPixels - 1);

                float brushAlpha = brushTexture.GetPixelBilinear(brushSampleU, brushSampleV).a;
                float finalPaintAlpha = brushAlpha * brushStrength;

                if (finalPaintAlpha <= 0.001f)
                    continue;

                Color currentMaskPixelColor = runtimeMaskTexture.GetPixel(targetPixelX, targetPixelY);
                Color blendedColor = Color.Lerp(currentMaskPixelColor, currentPaintColor, finalPaintAlpha);

                runtimeMaskTexture.SetPixel(targetPixelX, targetPixelY, blendedColor);
            }
        }

        runtimeMaskTexture.Apply();
    }

    private void ApplyMaskTextureToCachedRenderer()
    {
        cachedHitRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetTexture(maskTexturePropertyName, runtimeMaskTexture);
        cachedHitRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private bool IsInsideTextureBounds(Texture2D texture, int pixelX, int pixelY)
    {
        return pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height;
    }
}
