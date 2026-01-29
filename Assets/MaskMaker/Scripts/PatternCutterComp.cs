using System.Collections.Generic;
using UnityEngine;

public class PatternCutterComp : MonoBehaviour
{
    [SerializeField] private MeshFilter patternPlane;
    [SerializeField] private Material patternMaterial;
    [SerializeField] private LineRenderer cutLine;

    private List<Vector3> _worldCutPoints = new List<Vector3>();
    private bool _isCutting = false;

    private void Update()
    {
        if (PatternPieceComp.CheckIsDraggingAnyPatternPiece()) return;

        if (Input.GetMouseButtonDown(0)) StartCut();
        if (_isCutting && Input.GetMouseButton(0)) ContinueCut();
        if (Input.GetMouseButtonUp(0)) CompleteCut();
    }

    private void StartCut()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == patternPlane.gameObject)
        {
            _worldCutPoints.Clear();
            _worldCutPoints.Add(hit.point);

            cutLine.positionCount = 1;
            cutLine.SetPosition(0, hit.point);
            _isCutting = true;
        }
    }

    private void ContinueCut()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) &&
            hit.collider.gameObject == patternPlane.gameObject)
        {
            if (_worldCutPoints.Count > 0)
            {
                float distance = Vector3.Distance(hit.point, _worldCutPoints[_worldCutPoints.Count - 1]);
                if (distance < 0.05f) return;
            }

            _worldCutPoints.Add(hit.point);
            cutLine.positionCount = _worldCutPoints.Count;
            cutLine.SetPosition(_worldCutPoints.Count - 1, hit.point);
        }
    }

    private void CompleteCut()
    {
        if (_worldCutPoints.Count < 3)
        {
            ResetCut();
            return;
        }

        if (Vector3.Distance(_worldCutPoints[0], _worldCutPoints[_worldCutPoints.Count - 1]) > 0.1f)
        {
            _worldCutPoints.Add(_worldCutPoints[0]);
        }

        ExtractPatternPiece();
        ResetCut();
    }

    private int GetPatternPieceMeshVertexCount()
    {
        int extraSlotForCenterVertex = 1;
        return _worldCutPoints.Count + extraSlotForCenterVertex;
    }

    private void ExtractPatternPiece()
    {
        Vector3[] localVertices = new Vector3[GetPatternPieceMeshVertexCount()];
        int lastVertexIndex = localVertices.Length - 1;

        for (int i = 0; i < _worldCutPoints.Count; i++)
        {
            Vector3 localVertex = patternPlane.transform.InverseTransformPoint(_worldCutPoints[i]);
            localVertices[i] = localVertex;
        }

        Vector3 localMeshCenter = Vector3.zero;
        foreach (Vector3 point in localVertices)
        {
            localMeshCenter += point;
        }
        localMeshCenter /= localVertices.Length;

        localVertices[lastVertexIndex] = localMeshCenter;

        for (int i = 0; i < localVertices.Length; i++)
        {
            localVertices[i] = localVertices[i] - localMeshCenter;
        }

        int[] triangles = new int[(localVertices.Length - 2) * 3];
        int triangleIndex = 0;
        for (int i = 0; i < localVertices.Length - 2; i++)
        {
            triangles[triangleIndex++] = lastVertexIndex;
            triangles[triangleIndex++] = i;
            triangles[triangleIndex++] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = localVertices;
        mesh.triangles = triangles;

        Vector2[] uvs = new Vector2[localVertices.Length];
        for (int i = 0; i < localVertices.Length; i++)
        {
            uvs[i] = new Vector2(localVertices[i].x + 0.5f, localVertices[i].y + 0.5f);
        }
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Vector3 worldCenter = patternPlane.transform.TransformPoint(localMeshCenter);
        GameObject piece = new GameObject("PatternPiece");
        piece.transform.position = worldCenter + patternPlane.transform.up * 0.25f;
        piece.transform.rotation = patternPlane.transform.rotation;
        piece.transform.localScale = patternPlane.transform.localScale;

        MeshFilter meshFilter = piece.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = piece.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(patternMaterial);

        piece.AddComponent<BoxCollider>();
        piece.AddComponent<PatternPieceComp>();
    }

    private void ResetCut()
    {
        _worldCutPoints.Clear();
        cutLine.positionCount = 0;
        _isCutting = false;
    }
}
