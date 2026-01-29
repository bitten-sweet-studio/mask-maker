using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneViewManagerComp : MonoBehaviour
{
    [SerializeField] CinemachineCamera _clientViewCinemachine;
    [SerializeField] CinemachineCamera _workstationViewCinemachine;

    private InputAction _gotoClientViewInputAction;
    private InputAction _gotoWorkstationViewInputAction;
    private CinemachineBrain _cinemachineBrain;

    private void Start()
    {
        Camera.main.TryGetComponent(out _cinemachineBrain);

        _gotoClientViewInputAction = InputSystem.actions.FindAction("GoToClientView");
        _gotoWorkstationViewInputAction = InputSystem.actions.FindAction("GoToWorkstationView");

        _gotoClientViewInputAction.performed += HandleGoToClientView;
        _gotoWorkstationViewInputAction.performed += HandleGoToWorkstationView;
    }

    private void Destroy()
    {
        _gotoClientViewInputAction.performed -= HandleGoToClientView;
        _gotoWorkstationViewInputAction.performed -= HandleGoToWorkstationView;
    }

    private void HandleGoToClientView(InputAction.CallbackContext context)
    {
        GoToClientView();
    }

    private void HandleGoToWorkstationView(InputAction.CallbackContext context)
    {
        GoToWorkstationView();
    }

    public void GoToClientView()
    {
        if (_cinemachineBrain.IsBlending) return;

        Debug.Log("Client View");
        _clientViewCinemachine.Priority = 5;
        _workstationViewCinemachine.Priority = 0;
    }

    public void GoToWorkstationView()
    {
        if (_cinemachineBrain.IsBlending) return;

        Debug.Log("Workstation View");
        _clientViewCinemachine.Priority = 0;
        _workstationViewCinemachine.Priority = 5;
    }
}
