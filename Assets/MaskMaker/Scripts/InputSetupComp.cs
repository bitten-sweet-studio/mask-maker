using UnityEngine;
using UnityEngine.InputSystem;

public class InputSetupComp : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActionAsset;
    [SerializeField] private MainMenuWidgetComp _mainMenuWidget;

    private void Awake()
    {
        _inputActionAsset.Disable();

        _mainMenuWidget.Opened += OnGamePaused;
        _mainMenuWidget.GameContinued += OnGameContinued;
    }

    private void Destroy()
    {
        _mainMenuWidget.Opened -= OnGamePaused;
        _mainMenuWidget.GameContinued -= OnGameContinued;
    }

    private void OnGamePaused()
    {
        _inputActionAsset.Disable();
    }

    private void OnGameContinued()
    {
        _inputActionAsset.Enable();
    }
}
