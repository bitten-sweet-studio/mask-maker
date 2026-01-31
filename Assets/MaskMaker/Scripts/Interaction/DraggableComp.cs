using UnityEngine;

[DisallowMultipleComponent]
public class DraggableComp : InteractionBaseComp
{
    [Header("Custom Settings")]
    [SerializeField] private float hoverHeight = 0.1f;
    [SerializeField] private float dragForce = 100f;
    [SerializeField] private float dragDamping = 10f;
    [SerializeField] private bool _shouldFreezeRotation = true;
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

    private bool ShouldFreezeRotation => _isUsingTemplate
        ? _cachedTemplate.ShouldFreezeRotation
        : _shouldFreezeRotation;

    private LayerMask NonInteractibleLayer => _isUsingTemplate
        ? _cachedTemplate.NonInteractibleLayer
        : _nonInteractibleLayer;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Rigidbody rb;
    private Collider _collider;

    private void Start()
    {
        TryGetComponent(out rb);
        TryGetComponent(out _collider);
    }

    private void OnDisable()
    {
        StopDragging();
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
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, NonInteractibleLayer))
        {
            dragOffset = transform.position - hit.point;
        }
        else
        {
            dragOffset = Vector3.zero;
        }

        if (ShouldFreezeRotation)
        {
            rb.freezeRotation = true;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void StopDragging()
    {
        isDragging = false;

        if (ShouldFreezeRotation)
        {
            rb.freezeRotation = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isDragging) return;

        Vector3 targetPos = GetMouseWorldPosition() + dragOffset;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, NonInteractibleLayer))
        {
            targetPos = hit.point + (hit.normal * HoverHeight) + dragOffset;
        }

        MoveToTarget(targetPos);
    }

    private void MoveToTarget(Vector3 targetPosition)
    {
        if (rb.isKinematic)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            Vector3 force = (targetPosition - rb.position) * DragForce - rb.linearVelocity * DragDamping;
            rb.AddForce(force);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
