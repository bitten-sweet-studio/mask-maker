using UnityEngine;

[DisallowMultipleComponent]
public class ScalableComp : InteractionBaseComp
{
    [Header("Custom Settings")]
    [SerializeField] private float scaleSpeed = 3f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private bool _enableDiscoMode = false;

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

    private bool EnableDiscoMode => _isUsingTemplate
        ? _cachedTemplate.EnableDiscoMode
        : _enableDiscoMode;

    private Vector3 targetScale;
    private Vector3 scaleVelocity;

    private void Start()
    {
        targetScale = transform.localScale;
    }

    private void OnMouseOver()
    {
        if (!enabled) return;

        if (!EnableDiscoMode) HandleInput();
    }

    private void Update()
    {
        if (EnableDiscoMode) HandleInput();
    }

    private void HandleInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        HandleScaling(scroll);
    }

    private void HandleScaling(float scroll)
    {
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float scaleFactor = 1f + (scroll * ScaleSpeed * Time.deltaTime);
            targetScale *= scaleFactor;
            targetScale = ClampScale(targetScale);
        }

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
