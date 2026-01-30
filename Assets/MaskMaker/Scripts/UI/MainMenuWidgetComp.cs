using System;
using UnityEngine;

public class MainMenuWidgetComp : MonoBehaviour
{
    [SerializeField] private SettingsMenuWidgetComp _settingsMenuWidget;
    [SerializeField] private CanvasGroup _buttonsCanvasGroup;

    public event Action Opened;
    public event Action GameContinued;
    public event Action GameStarted;

    private CanvasGroup _rootCanvasGroup;
    private bool _hasGameStarted;

    private void Awake()
    {
        transform.GetChild(0).gameObject.TryGetComponent(out _rootCanvasGroup);
    }

    private void Start()
    {
        _settingsMenuWidget.Closed += OnSettingsMenuClosed;
    }

    private void Destroy()
    {
        _settingsMenuWidget.Closed -= OnSettingsMenuClosed;
    }

    public void Open()
    {
        _rootCanvasGroup.EnableWidgetTween();
        Opened?.Invoke();
    }

    public void ContinueGame()
    {
        _rootCanvasGroup.DisableWidgetTween();

        if (!_hasGameStarted)
        {
            GameStarted?.Invoke();
            _hasGameStarted = true;
        }
        GameContinued?.Invoke();
    }

    public void OpenSettings()
    {
        _buttonsCanvasGroup.DisableWidgetTween();
        _settingsMenuWidget.Open();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnSettingsMenuClosed()
    {
        _buttonsCanvasGroup.EnableWidgetTween();
    }
}
