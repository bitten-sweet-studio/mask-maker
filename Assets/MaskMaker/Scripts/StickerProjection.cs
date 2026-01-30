using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class StickerProjection : MonoBehaviour
{
    [Header("Contact Filter")]
    public LayerMask validLayers = ~0;     // só projeta se encostar nessa(s) layer(s)
    public bool useTrigger = false;        // se true, use OnTriggerEnter; se false, OnCollisionEnter

    [Header("Projection")]
    public Vector3 rayDirectionLocal = Vector3.down;
    public float rayDistance = 5f;
    public float surfaceOffset = 0.001f;

    [Header("Mesh Update")]
    public bool recalcNormals = true;
    public bool recalcBounds = true;

    [Header("One Shot")]
    public bool projectOnlyOnce = true;

    MeshFilter mf;
    Mesh meshInstance;
    Vector3[] verts;

    bool projected = false;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();

        // Instancia a mesh pra não alterar o asset compartilhado
        if (mf.sharedMesh != null)
        {
            meshInstance = Instantiate(mf.sharedMesh);
            meshInstance.name = mf.sharedMesh.name + " (Projected Instance)";
            mf.sharedMesh = meshInstance;
            verts = meshInstance.vertices;
        }
    }

    // ----------- CONTACT -----------
    void OnCollisionEnter(Collision col)
    {
        if (useTrigger) return;
        TryProjectFromCollider(col.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        TryProjectFromCollider(other);
    }

    void TryProjectFromCollider(Collider target)
    {
        if (projectOnlyOnce && projected) return;

        // Checa layer
        int targetLayerMaskBit = 1 << target.gameObject.layer;
        if ((validLayers.value & targetLayerMaskBit) == 0) return;

        // Projeta usando ESSE collider como alvo
        ProjectVerticesTo(target);

        projected = true;
    }

    // ----------- PROJECTION -----------
    void ProjectVerticesTo(Collider targetCollider)
    {
        if (meshInstance == null || verts == null) return;

        Transform t = transform;
        Vector3 rayDirWorld = t.TransformDirection(rayDirectionLocal.normalized);

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vWorld = t.TransformPoint(verts[i]);

            // Começa um pouco "atrás" do raio pra evitar nascer dentro do collider
            Vector3 origin = vWorld - rayDirWorld * 0.01f;
            Ray ray = new Ray(origin, rayDirWorld);

            // IMPORTANTE: isso garante que só acerta o collider que encostou
            if (targetCollider.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                Vector3 projectedWorld = hit.point + hit.normal * surfaceOffset;
                verts[i] = t.InverseTransformPoint(projectedWorld);
            }
        }

        meshInstance.vertices = verts;

        if (recalcNormals) meshInstance.RecalculateNormals();
        if (recalcBounds) meshInstance.RecalculateBounds();
    }

    [ContextMenu("Force Project (needs a target collider in contact)")]
    void ForceProjectInfo()
    {
        Debug.Log("Use o contato (Collision/Trigger) para definir o collider alvo.");
    }
}
