using UnityEngine;

[DisallowMultipleComponent]
public class DraggableComp : MonoBehaviour
{
    [Header("Template Settings")]
    [SerializeField] private bool _shouldUseGlobalSettings = true;
    [SerializeField] private InteractionSettingsAsset _settingsTemplateOverride;

    [Header("Custom Settings")]
    [SerializeField] private float hoverHeight = 0.1f;
    [SerializeField] private float dragForce = 100f;
    [SerializeField] private float dragDamping = 10f;
    [SerializeField] private LayerMask _nonInteractibleLayer;

    private float HoverHeight => _isUsingTemplate
        ? _cachedTemplate.HoverHeight
        : hoverHeight;

    private float DragForce => _isUsingTemplate
        ? _cachedTemplate.DragForce
        : dragForce;

    private float DragDamping => _isUsingTemplate
        ? _cachedTemplate.DragDamping
        : dragDamping;

    private LayerMask NonInteractibleLayer => _isUsingTemplate
        ? _cachedTemplate.NonInteractibleLayer
        : _nonInteractibleLayer;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Rigidbody rb;
    private Collider _collider;

    private InteractionSettingsAsset _cachedTemplate;
    private bool _isUsingTemplate;

    private void Start()
    {
        TryGetComponent(out rb);
        TryGetComponent(out _collider);

        _cachedTemplate =
            InteractionSettingsAsset.GetGlobalOrOverrideSettingsTemplate(
                _shouldUseGlobalSettings,
                _settingsTemplateOverride);
        _isUsingTemplate = _cachedTemplate != null;
    }

    private void FixedUpdate()
    {
        if (!isDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, NonInteractibleLayer))
        {
            Vector3 targetPos = hit.point + (hit.normal * HoverHeight) + dragOffset;
            Vector3 forceDirection = targetPos - rb.position;
            rb.AddForce(forceDirection * DragForce - rb.linearVelocity * DragDamping);
        }
        else
        {
            Vector3 targetPos = mouseWorldPos + dragOffset;
            Vector3 forceDirection = targetPos - rb.position;
            rb.AddForce(forceDirection * DragForce - rb.linearVelocity * DragDamping);
        }
    }

    private void OnMouseDown()
    {
        StartDragging();
    }

    private void OnMouseUp()
    {
        StopDragging();
    }

    private void StartDragging()
    {
        isDragging = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _nonInteractibleLayer))
        {
            dragOffset = transform.position - hit.point;
        }
        else
        {
            dragOffset = Vector3.zero;
        }
    }

    private void StopDragging()
    {
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
