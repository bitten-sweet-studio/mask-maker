using UnityEngine;
using DG.Tweening;

public class MaskComp : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Tween _floatTween;
    private DraggableComp _draggable;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _draggable);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out MaskEditingAreaComp maskEditingArea))
        {
            return;
        }

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _draggable.StopDragging();

        transform.DOMove(other.bounds.center, 0.5f).OnComplete(() =>
        {
            _floatTween = transform
            .DOMoveY(0.5f, 2f)
            .SetEase(Ease.InOutSine)
            .SetRelative(true)
            .SetLoops(-1, LoopType.Yoyo);
        });
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out MaskEditingAreaComp maskEditingArea))
        {
            return;
        }

        _floatTween.Kill();
    }
}
