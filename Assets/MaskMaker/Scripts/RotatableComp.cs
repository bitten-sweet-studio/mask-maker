using UnityEngine;

public class RotatableComp : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool hideCursorWhenRotating = false;
    [SerializeField] private bool _enableMagicBeyblade = false;

    private bool isRotating = false;
    private Vector2 currentRotationVelocity;
    private Vector2 targetRotation;
    private Vector2 smoothRotationVelocity;

    private void Update()
    {
        HandleRotationInput();

        if (isRotating)
        {
            UpdateTargetRotation();

            if (!_enableMagicBeyblade)
            {
                ApplySmoothRotation();
            }
        }

        if (_enableMagicBeyblade)
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

        if (hideCursorWhenRotating)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void StopRotating()
    {
        isRotating = false;

        if (hideCursorWhenRotating)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void UpdateTargetRotation()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Apply rotation speed with time delta
        currentRotationVelocity = mouseDelta * rotationSpeed;

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
            smoothTime
        );

        // Apply the smoothed rotation
        if (smoothRotation.magnitude > 0.001f)
        {
            transform.Rotate(Vector3.up, -smoothRotation.x, Space.World);
            transform.Rotate(Vector3.right, smoothRotation.y, Space.World);
        }
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetSmoothTime(float time)
    {
        smoothTime = Mathf.Max(0.01f, time);
    }
}
