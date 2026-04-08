using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform; // Main Camera Transform

    [Header("Idle Detection")]
    [SerializeField] private float idleThreshold = 3f;

    private CharacterController _cc;
    private JoystickInputHandler _input;
    private Animator _animator;
    private bool _hasAnimator;
    private bool _isPaused = true;
    private float _idleTimer;
    private float _verticalVelocity;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null && _animator.runtimeAnimatorController != null;

        // Inspector에 미할당 시 Camera.main 자동 참조
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        _input = JoystickInputHandler.Instance;

        if (_input != null)
            _input.OnFirstInput += OnFirstInput;

        SetPause(true);
    }

    private void OnDestroy()
    {
        if (_input != null)
            _input.OnFirstInput -= OnFirstInput;
    }

    private void Update()
    {
        if (_isPaused) return;

        Vector2 rawInput = _input != null ? _input.MoveInput : Vector2.zero;
        HandleMovement(rawInput);
        HandleIdleTimer(rawInput);
    }

    private void HandleMovement(Vector2 input)
    {
        // 중력
        if (_cc.isGrounded)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        // ── 카메라 기준 방향 변환 ────────────────────────────────────────
        // 카메라의 forward/right에서 y축 성분 제거 후 정규화
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 조이스틱 입력을 카메라 기준 월드 방향으로 변환
        Vector3 moveDir = camForward * input.y + camRight * input.x;

        float speed = moveDir.magnitude * moveSpeed;

        // 이동
        Vector3 velocity = moveDir.normalized * speed;
        velocity.y = _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);

        // 이동 방향으로 부드럽게 회전
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (_hasAnimator)
            _animator.SetFloat(SpeedHash, speed / moveSpeed);
    }

    private void HandleIdleTimer(Vector2 input)
    {
        if (input.sqrMagnitude > 0.01f)
        {
            _idleTimer = 0f;
            UIManager.Instance?.HideGuide();
        }
        else
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= idleThreshold)
            {
                _idleTimer = 0f;
                UIManager.Instance?.ShowGuide();
            }
        }
    }

    public void SetPause(bool paused)
    {
        _isPaused = paused;

        if (paused && _hasAnimator)
            _animator.SetFloat(SpeedHash, 0f);
    }

    private void OnFirstInput()
    {
        SetPause(false);
        UIManager.Instance?.HideGuide();
    }
}