using UnityEngine;

public class ShapeDraw : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera sceneCamera;

    [Header("Brush")]
    [SerializeField] private Texture2D brushTexture;
    [SerializeField] private int brushRadiusInPixels = 24;
    [SerializeField, Range(0f, 1f)] private float brushStrength = 1f;

    [Header("Raycast")]
    [SerializeField] private LayerMask paintableLayers = ~0;

    [Header("Paint Mode")]
    [SerializeField] private Color currentPaintColor = Color.white;

    [Header("Tool State")]
    [SerializeField] private bool isDrawingEnabled = true;

    private PaperSurface activePaperSurface;

    private Vector3 previousMousePosition;
    private bool hasPreviousMousePosition;

    private void Awake()
    {
        if (sceneCamera == null)
            sceneCamera = Camera.main;
    }

    private void Update()
    {
        if (!isDrawingEnabled)
            return;
        
        if (!Input.GetMouseButton(0))
        {
            hasPreviousMousePosition = false;
            return;
        }
        
        if(Input.GetKey(KeyCode.Escape)) StopDrawing();

        Vector3 currentMousePosition = Input.mousePosition;

        if (hasPreviousMousePosition && currentMousePosition == previousMousePosition)
            return;

        previousMousePosition = currentMousePosition;
        hasPreviousMousePosition = true;

        if (!TryGetRaycastHit(currentMousePosition, out RaycastHit raycastHit))
            return;

        // ✅ Aqui é a parte que normalmente quebra (collider em child etc)
        PaperSurface hitPaperSurface =
            raycastHit.collider.GetComponent<PaperSurface>() ??
            raycastHit.collider.GetComponentInParent<PaperSurface>() ??
            raycastHit.collider.GetComponentInChildren<PaperSurface>();

        if (hitPaperSurface == null)
        {
            Debug.LogWarning($"Raycast acertou '{raycastHit.collider.name}', mas não achei PaperSurface nele/pai/filhos.", raycastHit.collider);
            return;
        }

        if (hitPaperSurface != activePaperSurface)
        {
            activePaperSurface = hitPaperSurface;
            activePaperSurface.CreateRuntimeMaskIfNeeded();
            activePaperSurface.ApplyRuntimeMaskToRenderer();
            Debug.Log($"Paper ativo: {activePaperSurface.name}", activePaperSurface);
        }

        PaintAtHitPoint(activePaperSurface.RuntimeMaskTexture, raycastHit.textureCoord);
        activePaperSurface.ApplyRuntimeMaskToRenderer();
    }

    // Buttons
    public void SetDrawing() => currentPaintColor = Color.white;
    public void SetErasing() => currentPaintColor = Color.black;

    private bool TryGetRaycastHit(Vector3 mouseScreenPosition, out RaycastHit raycastHit)
    {
        Ray ray = sceneCamera.ScreenPointToRay(mouseScreenPosition);
        bool hit = Physics.Raycast(ray, out raycastHit, Mathf.Infinity, paintableLayers, QueryTriggerInteraction.Ignore);

        // Debug útil pra confirmar que está acertando o papel certo
        if (!hit) return false;

        return true;
    }

    private void PaintAtHitPoint(Texture2D runtimeMaskTexture, Vector2 hitTextureCoordinates)
    {
        if (runtimeMaskTexture == null) return;
        if (brushTexture == null) return;

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

                if (targetPixelX < 0 || targetPixelX >= runtimeMaskTexture.width ||
                    targetPixelY < 0 || targetPixelY >= runtimeMaskTexture.height)
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
    public void StartDrawing()
    {
        isDrawingEnabled = true;
    }

    public void StopDrawing()
    {
        isDrawingEnabled = false;
        hasPreviousMousePosition = false; // evita linha fantasma ao voltar
    }

    public void ToggleDrawing()
    {
        isDrawingEnabled = !isDrawingEnabled;

        if (!isDrawingEnabled)
            hasPreviousMousePosition = false;
    }
}
