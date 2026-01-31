using UnityEngine;

[DisallowMultipleComponent]
public class RotatableComp : InteractionBaseComp
{
    [Header("Custom Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool hideCursorWhenRotating = false;
    [SerializeField] private bool _enableMagicBeyblade = false;
    [SerializeField] private bool _enableDiscoMode = false;

    private float RotationSpeed => _isUsingTemplate
        ? _cachedTemplate.RotationSpeed
        : rotationSpeed;

    private float SmoothTime => _isUsingTemplate
        ? _cachedTemplate.RotationSmoothTime
        : smoothTime;

    private bool HideCursorWhenRotating => _isUsingTemplate
        ? _cachedTemplate.HideCursorWhenRotating
        : hideCursorWhenRotating;

    private bool EnableMagicBeyblade => _isUsingTemplate
        ? _cachedTemplate.EnableMagicBeyblade
        : _enableMagicBeyblade;

    private bool EnableDiscoMode => _isUsingTemplate
        ? _cachedTemplate.EnableDiscoMode
        : _enableDiscoMode;

    private bool isRotating = false;
    private Vector2 currentRotationVelocity;
    private Vector2 targetRotation;
    private Vector2 smoothRotationVelocity;

    private void OnDisable()
    {
        StopRotating();
    }

    private void OnMouseOver()
    {
        if (EnableDiscoMode) return;

        HandleStartRotatingInput();
    }

    private void Update()
    {
        if (EnableDiscoMode) { HandleAllRotationInput(); }
        else { HandleStopRotatingInput(); }

        if (isRotating) { UpdateTargetRotation(); }

        if (EnableMagicBeyblade || isRotating) { ApplySmoothRotation(); }
    }

    private void HandleAllRotationInput()
    {
        HandleStartRotatingInput();
        HandleStopRotatingInput();
    }

    private void HandleStartRotatingInput()
    {
        if (Input.GetMouseButtonDown(1)) StartRotating();
    }

    private void HandleStopRotatingInput()
    {
        if (Input.GetMouseButtonUp(1)) StopRotating();
    }

    private void StartRotating()
    {
        isRotating = true;
        currentRotationVelocity = Vector2.zero;

        if (HideCursorWhenRotating)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void StopRotating()
    {
        isRotating = false;

        if (HideCursorWhenRotating)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void UpdateTargetRotation()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Apply rotation speed with time delta
        currentRotationVelocity = mouseDelta * RotationSpeed;

        // Accumulate target rotation
        targetRotation += currentRotationVelocity * Time.deltaTime;
    }

    private void ApplySmoothRotation()
    {
        Vector2 smoothRotation = Vector2.SmoothDamp(
            Vector2.zero,
            currentRotationVelocity,
            ref smoothRotationVelocity,
            SmoothTime
        );

        if (smoothRotation.magnitude > 0.001f)
        {
            transform.Rotate(Vector3.up, -smoothRotation.x, Space.World);
            transform.Rotate(Vector3.right, smoothRotation.y, Space.World);
        }
    }
}
