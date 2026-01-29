using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Interactable : MonoBehaviour
{
    [Header("Renderers (opcional)")]
    [Tooltip("Se vazio, pega todos os Renderers nos filhos.")]
    [SerializeField] private Renderer[] renderers;

    [Header("Eventos")]
    public UnityEvent onClick;

    // Cache das listas originais pra restaurar sem erro.
    private readonly Dictionary<Renderer, Material[]> _originalMats = new();
    private bool _isHighlighted;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(includeInactive: false);

        // Cache do estado original
        foreach (var r in renderers)
        {
            if (!r) continue;
            _originalMats[r] = r.sharedMaterials; // sharedMaterials pra evitar instanciar sem necessidade
        }
    }

    public void SetHighlighted(bool value, Material overlayMat)
    {
        if (_isHighlighted == value) return;
        _isHighlighted = value;

        if (value) ApplyOverlay(overlayMat);
        else RestoreOriginal();
    }

    public void Click()
    {
        onClick?.Invoke();
    }

    private void ApplyOverlay(Material overlayMat)
    {
        if (!overlayMat) return;

        foreach (var r in renderers)
        {
            if (!r) continue;

            var original = _originalMats[r];
            // Evita duplicar overlay se SetHighlighted for chamado repetidamente
            int len = original.Length;

            var newMats = new Material[len + 1];
            for (int i = 0; i < len; i++) newMats[i] = original[i];
            newMats[len] = overlayMat;

            r.sharedMaterials = newMats;
        }
    }

    private void RestoreOriginal()
    {
        foreach (var kv in _originalMats)
        {
            if (!kv.Key) continue;
            kv.Key.sharedMaterials = kv.Value;
        }
    }

    private void OnDisable()
    {
        // Segurança: se desativar em hover, garante que não fica "preso"
        if (_isHighlighted) RestoreOriginal();
        _isHighlighted = false;
    }
}
