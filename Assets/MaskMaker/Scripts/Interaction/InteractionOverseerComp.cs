using System;
using SaintsField;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionOverseerComp : MonoBehaviour
{
    [Header("Debug")]
    [ReadOnly, SerializeField] private InteractionBaseComp[] _interactionComps;
    [ReadOnly, SerializeField] private ShapeDraw _shapeDraw;

    private void Awake()
    {
        _interactionComps = gameObject.GetComponents<InteractionBaseComp>();

        _shapeDraw = GameObject.FindAnyObjectByType<ShapeDraw>();
        _shapeDraw.StartedDrawing += OnStartedDrawing;
        _shapeDraw.StoppedDrawing += OnStoppedDrawing;
    }

    private void Destroy()
    {
        _shapeDraw.StartedDrawing -= OnStartedDrawing;
        _shapeDraw.StoppedDrawing -= OnStoppedDrawing;
    }

    private void OnStartedDrawing()
    {
        UpdateInteractionCompsEnabledState(false);
    }

    private void OnStoppedDrawing()
    {
        UpdateInteractionCompsEnabledState(true);
    }

    private void UpdateInteractionCompsEnabledState(bool state)
    {
        foreach (var interactionComp in _interactionComps)
        {
            interactionComp.enabled = state;
        }
    }
}
