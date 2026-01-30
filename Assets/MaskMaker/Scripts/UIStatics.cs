using DG.Tweening;
using UnityEngine;

public static class UIStatics
{
    public static float DefaultTweenDuration = 0.5f;

    public static void EnableWidgetTween(this CanvasGroup widget)
    {
        widget.DOFade(1, DefaultTweenDuration)
            .OnComplete(() =>
                {
                    widget.blocksRaycasts = true;
                });
    }

    public static void DisableWidgetTween(this CanvasGroup widget)
    {
        widget.blocksRaycasts = false;
        widget.DOFade(0, DefaultTweenDuration);
    }

    public static void EnableWidgetInstant(this CanvasGroup widget)
    {
        widget.blocksRaycasts = true;
        widget.alpha = 1;
    }

    public static void DisableWidgetInstant(this CanvasGroup widget)
    {
        widget.blocksRaycasts = false;
        widget.alpha = 0;
    }
}
