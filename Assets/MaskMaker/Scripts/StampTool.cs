using UnityEngine;

[DisallowMultipleComponent]
public class StampTool : MonoBehaviour
{
    [Header("Fabric Capture")]
    [SerializeField] private Camera fabricCaptureCamera;

    [Header("Stamp Rules")]
    [Tooltip("Se true, só carimba quando a ferramenta NÃO estiver sendo segurada (drop).")]
    [SerializeField] private bool stampOnlyWhenNotHeld = true;

    [SerializeField] private bool isHeld = false;
    [SerializeField] private LayerMask fabricLayerMask;


    // evita carimbar repetidamente no mesmo papel enquanto estiver encostado
    private PaperSurface lastStampedPaperSurface;

    private void Awake()
    {
        if (fabricCaptureCamera == null)
            Debug.LogError("StampTool: fabricCaptureCamera não atribuída.", this);
        fabricCaptureCamera.enabled = false;
        fabricCaptureCamera.eventMask = 0;
    }

    /// <summary>
    /// Chame isso do seu sistema de pegar/soltar.
    /// isHeld=true quando estiver na mão; isHeld=false quando dropou.
    /// </summary>
    public void SetHeld(bool value)
    {
        isHeld = value;

        // Quando soltar, permitimos carimbar novamente (inclusive no mesmo papel)
        if (!isHeld)
            lastStampedPaperSurface = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryStampFromCollider(other);
    }
    private void OnTriggerStay(Collider other)
    {
        // Se você quiser que só carimbe ao encostar, pode remover OnTriggerStay.
        // Eu deixo pra garantir que, se dropou com overlap já ativo, ele ainda carimba.
        TryStampFromCollider(other);
    }

    private void OnTriggerExit(Collider other)
    {
        PaperSurface paper =
            other.GetComponent<PaperSurface>() ??
            other.GetComponentInParent<PaperSurface>();

        if (paper != null && paper == lastStampedPaperSurface)
            lastStampedPaperSurface = null;
    }

    private void TryStampFromCollider(Collider other)
    {
        if (stampOnlyWhenNotHeld && isHeld)
            return;

        PaperSurface paper =
            other.GetComponent<PaperSurface>() ??
            other.GetComponentInParent<PaperSurface>();

        if (paper == null)
            return;

        // Evita carimbar infinitas vezes no mesmo papel enquanto estiver encostado
        if (paper == lastStampedPaperSurface)
            return;

        // Só carimba se o papel estiver em cima do tecido (Cutout)
        if (paper.CurrentPaperState != PaperState.Cutout)
            return;

        // ✅ CARIMBO: captura o tecido e muda estado
        paper.CaptureFabricTexture(fabricCaptureCamera, fabricLayerMask);
        paper.SetPaperState(PaperState.Stamped);

        lastStampedPaperSurface = paper;

        Debug.Log($"StampTool: Carimbou '{paper.name}'.", paper);
    }
}
