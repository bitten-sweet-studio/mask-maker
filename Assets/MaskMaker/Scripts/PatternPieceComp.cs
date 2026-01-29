using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PatternPieceComp : MonoBehaviour
{
    private static bool _isDraggingAnyPatternPiece;

    private Vector3 _offset;
    private Collider _ownCollider;
    private RaycastHit _lastHit;
    private bool _wasLastHitSuccessful;

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
        _wasLastHitSuccessful = Physics.Raycast(ray, out _lastHit);

        if (_wasLastHitSuccessful)
        {
            transform.position = _lastHit.point + _lastHit.normal * 0.25f;
            transform.rotation = Quaternion.Euler(_lastHit.normal);
        }
    }

    private void OnMouseUp()
    {
        if (_ownCollider)
        {
            _ownCollider.enabled = true;
        }

        if (_wasLastHitSuccessful)
        {
            SnapToMask(_lastHit);
            // ConformToMask(
            //         _lastHit.transform.GetComponent<MeshFilter>().mesh,
            //         _lastHit.transform,
            //         _lastHit.point,
            //         _lastHit.normal);
        }

        _isDraggingAnyPatternPiece = false;
    }

    public void SnapToMask(RaycastHit hit)
    {
        GameObject mask = hit.collider.gameObject;
        Mesh maskMesh = mask.GetComponent<MeshFilter>().mesh;

        MeshFilter patternMF = GetComponent<MeshFilter>();
        Mesh patternMesh = patternMF.mesh;
        Vector3[] vertices = patternMesh.vertices;

        // For each pattern vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = transform.TransformPoint(vertices[i]);

            // Find closest point on mask triangles
            Vector3 closestSurfacePoint = FindClosestPointOnMeshTriangles(
                worldVertex,
                maskMesh,
                mask.transform
            );

            // Get normal at closest point and add offset
            Vector3 surfaceNormal = GetNormalAtPoint(closestSurfacePoint, mask, hit.normal);
            closestSurfacePoint += surfaceNormal * 0.001f;

            vertices[i] = transform.InverseTransformPoint(closestSurfacePoint);
        }

        // Update pattern mesh
        patternMesh.vertices = vertices;
        patternMesh.RecalculateNormals();

        // Position at hit point
        transform.position = hit.point;
        transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);

        // Attach to mask
        transform.SetParent(mask.transform);
        Destroy(GetComponent<Collider>());
    }

    Vector3 FindClosestPointOnMeshTriangles(Vector3 worldPoint, Mesh mesh, Transform meshTransform)
    {
        Vector3 closestPoint = worldPoint;
        float closestDistance = float.MaxValue;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Convert point to local space of mask
        Vector3 localPoint = meshTransform.InverseTransformPoint(worldPoint);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            Vector3 triangleClosest = ClosestPointOnTriangle(localPoint, v1, v2, v3);
            float distance = Vector3.Distance(localPoint, triangleClosest);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = meshTransform.TransformPoint(triangleClosest);
            }
        }

        return closestPoint;
    }

    Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Check if point is inside triangle
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = p - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) return a;

        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b;

        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c;

        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab;
        }

        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac;
        }

        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b);
        }

        float denom = 1f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }

    Vector3 GetNormalAtPoint(Vector3 point, GameObject mask, Vector3 fallbackNormal)
    {
        // Simple raycast to get normal
        Ray ray = new Ray(point + Vector3.up * 0.01f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 0.02f) && hit.collider.gameObject == mask)
        {
            return hit.normal;
        }

        return fallbackNormal;
    }
}
