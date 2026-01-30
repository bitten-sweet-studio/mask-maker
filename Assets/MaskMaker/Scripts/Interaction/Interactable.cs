using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SaintsField;

[DisallowMultipleComponent]
public class Interactable : MonoBehaviour
{
    [SerializeField]
    private Material _overlayMaterial;

    [Header("")]
    [Header("Events")]
    public UnityEvent onClick;

    [Header("Debug")]
    [ReadOnly, SerializeField] private Renderer[] renderers;

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

            _originalMats[r] = r.sharedMaterials; // sharedMaterials pra evitar instanciar sem necessidade
        }
    }

    private void OnMouseEnter()
    {
        ApplyOverlay(_overlayMaterial);
    }

    private void OnMouseUpAsButton()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }
    }

    private void OnMouseExit()
    {
        RestoreOriginal();
    }

    private void Click()
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
