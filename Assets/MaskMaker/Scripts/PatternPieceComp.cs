using UnityEngine;
using UnityEngine.Assertions;

public class PatternPieceComp : MonoBehaviour
{
    private static bool _isDraggingAnyPatternPiece;

    private Vector3 _offset;
    private Collider _ownCollider;

    private void Awake()
    {
        TryGetComponent(out _ownCollider);
        Assert.IsNotNull(_ownCollider);
    }

    public static bool CheckIsDraggingAnyPatternPiece()
    {
        return _isDraggingAnyPatternPiece;
    }

    private void OnMouseDown()
    {
        _isDraggingAnyPatternPiece = true;
        if (_ownCollider)
        {
            _ownCollider.enabled = false;
        }
    }

    private void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            transform.position = hit.point + hit.normal * 0.25f;
            transform.rotation = Quaternion.Euler(hit.normal);
        }
    }

    private void OnMouseUp()
    {
        if (_ownCollider)
        {
            _ownCollider.enabled = true;
        }
        _isDraggingAnyPatternPiece = false;
    }
}
