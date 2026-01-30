using UnityEngine;

[DisallowMultipleComponent]
public class RotatableComp : MonoBehaviour
{
    [Header("Template Settings")]
    [SerializeField] private bool _shouldUseGlobalSettings = true;
    [SerializeField] private InteractionSettingsAsset _settingsTemplateOverride;

    [Header("Custom Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool hideCursorWhenRotating = false;
    [SerializeField] private bool _enableMagicBeyblade = false;

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

    private bool isRotating = false;
    private Vector2 currentRotationVelocity;
    private Vector2 targetRotation;
    private Vector2 smoothRotationVelocity;

    private InteractionSettingsAsset _cachedTemplate;
    private bool _isUsingTemplate;

    private void Start()
    {
        _cachedTemplate =
            InteractionSettingsAsset.GetGlobalOrOverrideSettingsTemplate(
                _shouldUseGlobalSettings,
                _settingsTemplateOverride);
        _isUsingTemplate = _cachedTemplate != null;
    }

    private void Update()
    {
        HandleRotationInput();

        if (isRotating)
        {
            UpdateTargetRotation();

            if (!EnableMagicBeyblade)
            {
                ApplySmoothRotation();
            }
        }

        if (EnableMagicBeyblade)
        {
            ApplySmoothRotation();
        }
    }

    private void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartRotating();
        }

        if (Input.GetMouseButtonUp(1))
        {
            StopRotating();
        }
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
        // Smooth the rotation using SmoothDamp
        Vector2 smoothRotation = Vector2.SmoothDamp(
            Vector2.zero,
            currentRotationVelocity,
            ref smoothRotationVelocity,
            SmoothTime
        );

        // Apply the smoothed rotation
        if (smoothRotation.magnitude > 0.001f)
        {
            transform.Rotate(Vector3.up, -smoothRotation.x, Space.World);
            transform.Rotate(Vector3.right, smoothRotation.y, Space.World);
        }
    }
}
