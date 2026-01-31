using UnityEngine;

public class InteractionBaseComp : MonoBehaviour
{
    [Header("Template Settings")]
    [SerializeField] private bool _shouldUseGlobalSettings = true;
    [SerializeField] private InteractionSettingsAsset _settingsTemplateOverride;

    protected InteractionSettingsAsset _cachedTemplate;
    protected bool _isUsingTemplate;

    private void Start()
    {
        _cachedTemplate =
            InteractionSettingsAsset.GetGlobalOrOverrideSettingsTemplate(
                _shouldUseGlobalSettings,
                _settingsTemplateOverride);
        _isUsingTemplate = _cachedTemplate != null;
    }
}
