using UnityEngine;
using System;

/// <summary>
/// Joystick Pack (fenerax studios) 의 FloatingJoystick을 참조.
/// 모바일 터치(조이스틱)와 PC 마우스 입력을 모두 지원하도록 수정됨.
/// </summary>
public class JoystickInputHandler : MonoBehaviour
{
    public static JoystickInputHandler Instance { get; private set; }

    [SerializeField] private FloatingJoystick joystick;

    public event Action OnFirstInput;
    private bool _hasReceivedInput;

    // ── 마우스 시뮬레이션 변수 (전처리기 제거) ───────────────────────────
    private Vector2 _mouseOrigin;
    private Vector2 _mouseInput;
    private bool _isMouseDragging;

    [Header("PC Mouse Controls")]
    [SerializeField] private float mouseSensitivity = 0.01f; // 감도는 테스트하며 조절하세요.

    // ── 입력값 프로퍼티 ──────────────────────────────────────────────────
    public Vector2 MoveInput
    {
        get
        {
            // 1. 조이스틱 입력 확인
            Vector2 joyInput = joystick != null
                ? new Vector2(joystick.Horizontal, joystick.Vertical)
                : Vector2.zero;

            // 2. 조이스틱 입력이 거의 없다면 마우스 입력 반환
            if (joyInput.sqrMagnitude < 0.01f)
            {
                return _mouseInput;
            }

            return joyInput;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // 런타임(빌드 후)과 에디터 모두에서 작동하도록 호출
        HandleMouseSimulation();

        // 최초 입력 감지 (퍼즈 해제 트리거)
        if (!_hasReceivedInput && MoveInput.sqrMagnitude > 0.01f)
            NotifyFirstInput();
    }

    public void NotifyFirstInput()
    {
        if (_hasReceivedInput) return;
        _hasReceivedInput = true;
        OnFirstInput?.Invoke();
    }

    // ── 마우스 시뮬레이션 로직 (전처리기 제거 및 통합) ───────────────────
    private void HandleMouseSimulation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _mouseOrigin = Input.mousePosition;
            _isMouseDragging = true;
        }

        if (Input.GetMouseButton(1) && _isMouseDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - _mouseOrigin;
            // 감도를 곱해 0~1 사이 값으로 제한
            _mouseInput = Vector2.ClampMagnitude(delta * mouseSensitivity, 1f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            _mouseInput = Vector2.zero;
            _isMouseDragging = false;
        }
    }
}