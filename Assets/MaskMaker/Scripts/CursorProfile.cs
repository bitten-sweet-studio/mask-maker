using UnityEngine;

[CreateAssetMenu(menuName = "Input/Cursor Profile", fileName = "CursorProfile")]
public class CursorProfile : ScriptableObject
{
    [Header("ID para chamar por UnityEvent")]
    public string id = "Default";

    [Header("Sprite/Texture do cursor")]
    public Texture2D texture;

    [Header("Ponto quente (hotspot) em pixels")]
    public Vector2 hotspot;

    [Header("Modo de renderização do cursor")]
    public CursorMode cursorMode = CursorMode.Auto;

    [Header("Escala de hotspot: (0,0)=canto sup-esq, (1,1)=canto inf-dir")]
    public bool hotspotNormalized = false;

    public Vector2 GetHotspot()
    {
        if (!texture) return Vector2.zero;

        if (!hotspotNormalized) return hotspot;

        // hotspot como UV (0..1) -> pixels
        return new Vector2(hotspot.x * texture.width, hotspot.y * texture.height);
    }
}