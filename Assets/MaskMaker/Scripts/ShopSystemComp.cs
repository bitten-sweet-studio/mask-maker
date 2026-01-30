using System;
using DG.Tweening;
using UnityEngine;

public class ShopSystemComp : MonoBehaviour
{
    [SerializeField] private MainMenuWidgetComp _mainMenuWidget;

    [Header("Door")]
    [SerializeField] private GameObject _door;
    [SerializeField] private Vector3 _doorOpenTweenToRotation;
    [SerializeField] private float _doorOpenTweenDuration;

    public event Action ShopOpened;
    public event Action GameStarted;

    private void Awake()
    {
        _mainMenuWidget.GameStarted += OnGameStarted;
    }

    private void Destroy()
    {
        _mainMenuWidget.GameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        GameStarted?.Invoke();
        OpenDoor();
    }

    private void OpenDoor()
    {
        _door.transform
            .DOLocalRotate(_doorOpenTweenToRotation, _doorOpenTweenDuration)
            .OnComplete(() =>
            {
                ShopOpened?.Invoke();
            });
    }
}
