using UnityEngine;
using UnityEngine.Events;

public class InteractableHandler : MonoBehaviour
{
    [Header("Enable")]
    [SerializeField] private bool startEnabled = true;
    public bool IsEnabled => _enabled;
    private bool _enabled;

    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private LayerMask interactableMask = ~0;

    [Header("Highlight")]
    [SerializeField] private Material hoverOverlayMaterial;

    [Header("Click vs Drag")]
    [SerializeField] private float dragStartPixelDistance = 8f;
    [SerializeField] private float dragStartHoldTime = 0.12f;

    [Header("Drag Movement")]
    [Tooltip("Mantém a mesma profundidade do clique (bom pra plano da câmera).")]
    [SerializeField] private bool useFixedDepth = true;

    [SerializeField] private float dragMaxDepth = 50f;

    [Tooltip("Levanta o ponto do target pra não raspar na mesa.")]
    [SerializeField] private float hoverLift = 0.08f;

    [Tooltip("Se true, levanta na normal da superfície clicada (ótimo pra mesa inclinada).")]
    [SerializeField] private bool liftAlongHitNormal = true;

    [Header("Drag Stability")]
    [SerializeField] private bool freezeRotationWhileDragging = true;
    [SerializeField] private float maxDragSpeed = 30f; // limita teleporte em FPS baixo

    private Interactable _currentHover;
    public UnityEvent onLift;
    public UnityEvent onDrop;

    // Press state
    private Interactable _pressed;
    private Rigidbody _pressedRb;
    private Vector2 _mouseDownPos;
    private float _mouseDownTime;

    // Drag state
    private bool _isDragging;
    private float _dragDepth;
    private Vector3 _grabOffsetWorld;     // mantém o ponto clicado no mouse
    private Vector3 _hitNormal;
    private RigidbodyConstraints _originalConstraints;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        SetEnabled(startEnabled);
    }

    public void SetEnabled(bool value)
    {
        if (_enabled == value) return;
        _enabled = value;

        if (!_enabled)
        {
            ClearHover();
            CancelPressAndDrag();
        }
    }

    private void Update()
    {
        if (!_enabled) return;

        UpdateHover();

        if (Input.GetMouseButtonDown(0)) BeginPress();
        if (Input.GetMouseButton(0)) UpdatePressAndMaybeDrag();
        if (Input.GetMouseButtonUp(0)) EndPress();
    }

    private void FixedUpdate()
    {
        if (!_enabled) return;
        if (_isDragging && _pressedRb != null)
            DragMove();
    }

    private void UpdateHover()
    {
        if (_isDragging) return;

        Interactable hitInteractable = null;
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, maxDistance, interactableMask, QueryTriggerInteraction.Ignore))
            hitInteractable = hit.collider.GetComponentInParent<Interactable>();

        if (hitInteractable == _currentHover) return;

        if (_currentHover != null)
            _currentHover.SetHighlighted(false, hoverOverlayMaterial);

        _currentHover = hitInteractable;

        if (_currentHover != null)
            _currentHover.SetHighlighted(true, hoverOverlayMaterial);
    }

    private void BeginPress()
    {
        _mouseDownPos = Input.mousePosition;
        _mouseDownTime = Time.time;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, maxDistance, interactableMask, QueryTriggerInteraction.Ignore))
        {
            _pressed = null;
            _pressedRb = null;
            return;
        }

        _pressed = hit.collider.GetComponentInParent<Interactable>();
        if (_pressed == null) return;

        _pressedRb = _pressed.GetComponentInParent<Rigidbody>();
        if (_pressedRb == null) return;

        _originalConstraints = _pressedRb.constraints;

        // Profundidade do ponto clicado (mantém “na mesma camada”)
        _dragDepth = Vector3.Distance(cam.transform.position, hit.point);

        // Offset pra manter exatamente o ponto clicado sob o mouse
        _grabOffsetWorld = _pressedRb.position - hit.point;

        _hitNormal = hit.normal;
    }

    private void UpdatePressAndMaybeDrag()
    {
        if (_pressed == null || _pressedRb == null) return;
        if (_isDragging) return;

        float pixelMoved = ((Vector2)Input.mousePosition - _mouseDownPos).magnitude;
        float heldTime = Time.time - _mouseDownTime;

        if (pixelMoved >= dragStartPixelDistance || heldTime >= dragStartHoldTime)
            StartDragging();
    }

    private void StartDragging()
    {
        _isDragging = true;

        if (freezeRotationWhileDragging)
            _pressedRb.constraints = _originalConstraints | RigidbodyConstraints.FreezeRotation;

        // opcional, mas geralmente ajuda no “feeling”
        _pressedRb.interpolation = RigidbodyInterpolation.Interpolate;
        _pressedRb.useGravity = false;
        // se atravessar coisas, considere setar Continuous no inspector
        onLift.Invoke();
    }

    private void DragMove()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        float depth = useFixedDepth ? Mathf.Clamp(_dragDepth, 0.05f, dragMaxDepth)
                                    : Mathf.Min(dragMaxDepth, maxDistance);

        Vector3 targetPoint = ray.GetPoint(depth);

        // Lift pra não raspar
        Vector3 liftDir = (liftAlongHitNormal ? _hitNormal : Vector3.up);
        targetPoint += liftDir * hoverLift;

        // Mantém o ponto clicado sob o mouse
        Vector3 targetPos = targetPoint + _grabOffsetWorld;

        // Limita velocidade pra evitar “teleporte”/instabilidade
        Vector3 current = _pressedRb.position;
        Vector3 desiredDelta = targetPos - current;

        float maxStep = maxDragSpeed * Time.fixedDeltaTime;
        if (desiredDelta.magnitude > maxStep)
            targetPos = current + desiredDelta.normalized * maxStep;

        _pressedRb.MovePosition(targetPos);
    }

    private void EndPress()
    {
        if (_pressed == null) return;

        if (_isDragging)
        {
            StopDragging();
        }
        else
        {
            _pressed.Click();
        }

        _pressed = null;
        _pressedRb = null;
    }

    private void StopDragging()
    {
        _isDragging = false;

        if (_pressedRb != null)
        {
            _pressedRb.constraints = _originalConstraints;
            _pressedRb.useGravity = true;
        }

        onLift.Invoke();
    }

    private void ClearHover()
    {
        if (_currentHover != null)
        {
            _currentHover.SetHighlighted(false, hoverOverlayMaterial);
            _currentHover = null;
        }
    }

    private void CancelPressAndDrag()
    {
        if (_isDragging) StopDragging();
        _pressed = null;
        _pressedRb = null;
    }
}
