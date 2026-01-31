using UnityEngine;

[CreateAssetMenu(
        fileName = "IS_Name",
        menuName = MaskMakerStatics.ScriptableObjectMenuName + "/Interaction Settings")]
public class InteractionSettingsAsset : ScriptableObject
{
    [field: Header("General")]
    [field: SerializeField] public bool EnableDiscoMode { get; private set; } = false;

    [field: Header("Interactable")]
    [field: SerializeField] public Material OverlayMaterial { get; private set; }

    [field: Header("Drag")]
    [field: SerializeField] public float HoverHeight { get; private set; } = 0.6f;
    [field: SerializeField] public float DragForce { get; private set; } = 100f;
    [field: SerializeField] public float DragDamping { get; private set; } = 10f;
    [field: SerializeField] public bool ShouldFreezeRotation { get; private set; } = true;
    [field: SerializeField] public LayerMask NonInteractibleLayer { get; private set; }

    [field: Header("Rotation")]
    [field: SerializeField] public float RotationSpeed { get; private set; } = 8f;
    [field: SerializeField] public float RotationSmoothTime { get; private set; } = 0.15f;
    [field: SerializeField] public bool HideCursorWhenRotating { get; private set; } = true;
    [field: SerializeField] public bool EnableMagicBeyblade { get; private set; } = false;

    [field: Header("Scale")]
    [field: SerializeField] public float ScaleSpeed { get; private set; } = 3f;
    [field: SerializeField] public float ScaleSmoothTime { get; private set; } = 0.1f;
    [field: SerializeField] public float MinScale { get; private set; } = 0.5f;
    [field: SerializeField] public float MaxScale { get; private set; } = 2f;

    public const string DefaultGlobalSettingsPath = "IS_DefaultGlobal";

    public static InteractionSettingsAsset GetDefaultGlobalSettingsTemplate()
    {
        InteractionSettingsAsset asset = Resources.Load<InteractionSettingsAsset>(DefaultGlobalSettingsPath);
        return asset;
    }

    public static InteractionSettingsAsset GetGlobalOrOverrideSettingsTemplate(
            bool shouldUseGlobal, InteractionSettingsAsset overrideSettingsTemplate)
    {
        return shouldUseGlobal ? GetDefaultGlobalSettingsTemplate() : overrideSettingsTemplate;
    }
}
