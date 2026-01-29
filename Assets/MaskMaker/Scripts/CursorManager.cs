using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Cursor padrão")]
    [SerializeField] private CursorProfile defaultCursor;

    [Header("Perfis cadastrados (opcional)")]
    [SerializeField] private List<CursorProfile> profiles = new();

    private readonly Dictionary<string, CursorProfile> byId = new();
    private CursorProfile current;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Se quiser que persista entre cenas:
        // DontDestroyOnLoad(gameObject);

        RebuildIndex();
    }

    private void Start()
    {
        if (defaultCursor != null)
            SetCursor(defaultCursor);
        else
            ClearCursor();
    }

    [ContextMenu("Rebuild Index")]
    public void RebuildIndex()
    {
        byId.Clear();
        foreach (var p in profiles)
        {
            if (!p) continue;
            if (string.IsNullOrWhiteSpace(p.id)) continue;

            // Se IDs repetidos, o último ganha (você pode logar warning se preferir)
            byId[p.id] = p;
        }
    }

    // ===== Unity Events (assinaturas amigáveis) =====

    public void SetCursor(CursorProfile profile)
    {
        if (!profile || !profile.texture)
        {
            Debug.LogWarning("[CursorManager] Perfil inválido ou sem texture. Limpando cursor.");
            ClearCursor();
            current = null;
            return;
        }

        Cursor.SetCursor(profile.texture, profile.GetHotspot(), profile.cursorMode);
        current = profile;
    }

    public void SetCursorById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("[CursorManager] ID vazio. Voltando ao default.");
            SetDefault();
            return;
        }

        if (byId.TryGetValue(id, out var p) && p != null)
        {
            SetCursor(p);
            return;
        }

        Debug.LogWarning($"[CursorManager] Cursor ID '{id}' não encontrado. Voltando ao default.");
        SetDefault();
    }

    public void SetDefault()
    {
        if (defaultCursor != null) SetCursor(defaultCursor);
        else ClearCursor();
    }

    public void ClearCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // Extensível: stack (push/pop) pra overlays temporários (hover, drag, etc.)
    private readonly Stack<CursorProfile> stack = new();

    public void PushCursor(CursorProfile profile)
    {
        stack.Push(current);
        SetCursor(profile);
    }

    public void PopCursor()
    {
        if (stack.Count == 0) { SetDefault(); return; }
        var prev = stack.Pop();
        if (prev != null) SetCursor(prev);
        else SetDefault();
    }
}
