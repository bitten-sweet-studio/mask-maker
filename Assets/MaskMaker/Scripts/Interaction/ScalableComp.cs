using UnityEngine;

[DisallowMultipleComponent]
public class ScalableComp : MonoBehaviour
{
    [Header("Template Settings")]
    [SerializeField] private bool _shouldUseGlobalSettings = true;
    [SerializeField] private InteractionSettingsAsset _settingsTemplateOverride;

    [Header("Custom Settings")]
    [SerializeField] private float scaleSpeed = 3f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;

    private float ScaleSpeed => _isUsingTemplate
        ? _cachedTemplate.ScaleSpeed
        : scaleSpeed;

    private float SmoothTime => _isUsingTemplate
        ? _cachedTemplate.ScaleSmoothTime
        : smoothTime;

    private float MinScale => _isUsingTemplate
        ? _cachedTemplate.MinScale
        : minScale;

    private float MaxScale => _isUsingTemplate
        ? _cachedTemplate.MaxScale
        : maxScale;

    private Vector3 targetScale;
    private Vector3 scaleVelocity;
    private InteractionSettingsAsset _cachedTemplate;
    private bool _isUsingTemplate;

    private void Start()
    {
        targetScale = transform.localScale;
        _cachedTemplate =
            InteractionSettingsAsset.GetGlobalOrOverrideSettingsTemplate(
                _shouldUseGlobalSettings,
                _settingsTemplateOverride);
        _isUsingTemplate = _cachedTemplate != null;
    }

    private void Update()
    {
        HandleScaling();
        ApplySmoothScale();
    }

    private void HandleScaling()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            float scaleFactor = 1f + (scroll * ScaleSpeed * Time.deltaTime);
            targetScale *= scaleFactor;
            targetScale = ClampScale(targetScale);
        }
    }

    private void ApplySmoothScale()
    {
        if (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.SmoothDamp(
                transform.localScale,
                targetScale,
                ref scaleVelocity,
                SmoothTime
            );
        }
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, MinScale, MaxScale);
        scale.y = Mathf.Clamp(scale.y, MinScale, MaxScale);
        scale.z = Mathf.Clamp(scale.z, MinScale, MaxScale);
        return scale;
    }
}
