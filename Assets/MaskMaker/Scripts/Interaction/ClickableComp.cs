using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SaintsField;

[DisallowMultipleComponent]
public class ClickableComp : InteractionBaseComp
{
    [Header("Custom Settings")]
    [SerializeField] private Material _overlayMaterial;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onClick;

    [Header("Debug")]
    [ReadOnly, SerializeField] private Renderer[] renderers;

    private Material OverlayMaterial => _isUsingTemplate
        ? _cachedTemplate.OverlayMaterial
        : _overlayMaterial;

    private readonly Dictionary<Renderer, Material[]> _originalMats = new();
    private bool _isHighlighted;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
        }

        foreach (var r in renderers)
        {
            if (!r) continue;

            _originalMats[r] = r.sharedMaterials;
        }
    }

    private void OnDisable()
    {
        RestoreOriginal();
    }

    private void OnMouseEnter()
    {
        if (!enabled) return;

        ApplyOverlay(OverlayMaterial);
    }

    private void OnMouseUpAsButton()
    {
        if (Input.GetMouseButtonUp(0)) Click();
    }

    private void OnMouseExit()
    {
        if (!enabled) return;

        RestoreOriginal();
    }

    private void Click()
    {
        onClick?.Invoke();
    }

    private void ApplyOverlay(Material overlayMat)
    {
        if (_isHighlighted || !overlayMat) return;

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

        _isHighlighted = true;
    }

    private void RestoreOriginal()
    {
        if (!_isHighlighted) return;

        foreach (var kv in _originalMats)
        {
            if (!kv.Key) continue;
            kv.Key.sharedMaterials = kv.Value;
        }

        _isHighlighted = false;
    }

}
