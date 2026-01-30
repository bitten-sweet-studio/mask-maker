using UnityEngine;

[DisallowMultipleComponent]
public class ScalableComp : MonoBehaviour
{
    [SerializeField] private float scaleSpeed = 0.5f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 5f;

    private Vector3 targetScale;
    private Vector3 scaleVelocity;

    private void Start()
    {
        targetScale = transform.localScale;
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
            float scaleFactor = 1f + (scroll * scaleSpeed * Time.deltaTime);
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
                smoothTime
            );
        }
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        scale.z = Mathf.Clamp(scale.z, minScale, maxScale);
        return scale;
    }
}
