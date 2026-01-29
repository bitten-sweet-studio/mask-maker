using UnityEngine;

public class InteractableHandler : MonoBehaviour
{
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

    [Header("Spring Joint Drag")]
    [SerializeField] private float spring = 1200f;
    [SerializeField] private float damper = 80f;
    [SerializeField] private float massScale = 1f;
    [SerializeField] private float maxDistanceJoint = 0f;     // 0 = “ponto preso”
    [SerializeField] private float minDistanceJoint = 0f;

    [Header("Drag Depth / Plane")]
    [SerializeField] private bool useFixedDepth = true;       // true = mantém profundidade do clique
    [SerializeField] private float dragMaxDepth = 50f;

    [Header("Drag Extras")]
    [SerializeField] private bool freezeRotationWhileDragging = true;

    [Header("Lift")]
    [SerializeField] private float hoverLift = 0.08f; // 8 cm
    [SerializeField] private bool liftAlongCameraUp = false; // se true, usa up da camera

    private Interactable _currentHover;

    // Press state
    private Interactable _pressed;
    private Rigidbody _pressedRb;
    private Vector2 _mouseDownPos;
    private float _mouseDownTime;

    // Drag state
    private bool _isDragging;
    private float _dragDepth;
    private Vector3 _localGrabOffset;        // offset no espaço do rb (pra prender no ponto clicado)
    private RigidbodyConstraints _originalConstraints;

    // Dragger / Joint
    private GameObject _draggerGO;
    private Rigidbody _draggerRb;
    private SpringJoint _springJoint;
    private Vector3 _grabNormal;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        EnsureDragger();
    }

    private void Update()
    {
        UpdateHover();

        if (Input.GetMouseButtonDown(0)) BeginPress();
        if (Input.GetMouseButton(0)) UpdatePressAndMaybeDrag();
        if (Input.GetMouseButtonUp(0)) EndPress();

        if (_isDragging) UpdateDraggerTarget();
    }

    private void EnsureDragger()
    {
        if (_draggerGO != null) return;

        _draggerGO = new GameObject("[JointDragger]");
        _draggerGO.hideFlags = HideFlags.HideInHierarchy;

        _draggerRb = _draggerGO.AddComponent<Rigidbody>();
        _draggerRb.isKinematic = true;
        _draggerRb.useGravity = false;
        _draggerRb.interpolation = RigidbodyInterpolation.Interpolate;
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
            _grabNormal = hit.normal;
            _pressed = null;
            _pressedRb = null;
            return;
        }

        _pressed = hit.collider.GetComponentInParent<Interactable>();
        if (_pressed == null) return;

        _pressedRb = _pressed.GetComponentInParent<Rigidbody>();
        if (_pressedRb == null) return;

        // Profundidade pra manter a “camada” do clique (opcional)
        _dragDepth = Vector3.Distance(cam.transform.position, hit.point);

        // Offset do ponto clicado: prende no ponto real do collider (melhor feeling)
        // Armazenamos em local space pra ser estável enquanto roda.
        _localGrabOffset = _pressedRb.transform.InverseTransformPoint(hit.point);

        _originalConstraints = _pressedRb.constraints;
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

        // Posiciona dragger no ponto inicial “grab”
        Vector3 grabWorld = _pressedRb.transform.TransformPoint(_localGrabOffset);
        _draggerRb.position = grabWorld;

        // Cria joint no dragger (não no objeto)
        _springJoint = _draggerGO.AddComponent<SpringJoint>();
        _springJoint.autoConfigureConnectedAnchor = false;

        _springJoint.connectedBody = _pressedRb;

        // Anchor do joint no dragger = centro do dragger (0)
        _springJoint.anchor = Vector3.zero;

        // ConnectedAnchor = ponto local do rb que foi clicado (prende no “ponto pegado”)
        _springJoint.connectedAnchor = _localGrabOffset;

        _springJoint.spring = spring;
        _springJoint.damper = damper;
        _springJoint.massScale = massScale;

        _springJoint.maxDistance = maxDistanceJoint;
        _springJoint.minDistance = minDistanceJoint;
    }

    private void UpdateDraggerTarget()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        float depth = useFixedDepth ? Mathf.Clamp(_dragDepth, 0.05f, dragMaxDepth) : Mathf.Min(dragMaxDepth, maxDistance);
        Vector3 target = ray.GetPoint(depth);
        target += _grabNormal * hoverLift;

        Vector3 upDir = liftAlongCameraUp ? cam.transform.up : Vector3.up;
        target += upDir * hoverLift;

        _draggerRb.MovePosition(target);
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
            _pressedRb.constraints = _originalConstraints;

        if (_springJoint != null)
        {
            Destroy(_springJoint);
            _springJoint = null;
        }
    }
}
