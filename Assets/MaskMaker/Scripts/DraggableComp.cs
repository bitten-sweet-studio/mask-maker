using UnityEngine;

public class DraggableComp : MonoBehaviour
{
    [SerializeField] private float hoverHeight = 0.1f;
    [SerializeField] private float dragForce = 100f;
    [SerializeField] private float dragDamping = 10f;
    [SerializeField] private LayerMask _nonInteractibleLayer;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Rigidbody rb;
    private Collider _collider;

    private void Start()
    {
        TryGetComponent(out rb);
        TryGetComponent(out _collider);
    }

    private void FixedUpdate()
    {
        if (!isDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _nonInteractibleLayer))
        {
            Vector3 targetPos = hit.point + (hit.normal * hoverHeight) + dragOffset;
            Vector3 forceDirection = targetPos - rb.position;
            rb.AddForce(forceDirection * dragForce - rb.linearVelocity * dragDamping);
        }
        else
        {
            Vector3 targetPos = mouseWorldPos + dragOffset;
            Vector3 forceDirection = targetPos - rb.position;
            rb.AddForce(forceDirection * dragForce - rb.linearVelocity * dragDamping);
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
