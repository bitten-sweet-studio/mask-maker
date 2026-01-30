using UnityEngine;
using SaintsField;

public class MaskComp : MonoBehaviour
{
    [Header("Debug")]
    [ReadOnly, SerializeField] private Interactable _interactable;

    private void Awake()
    {
        TryGetComponent(out _interactable);
    }
}
