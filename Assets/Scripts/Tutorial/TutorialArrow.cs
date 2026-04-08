using System.Collections;
using UnityEngine;

/// <summary>
/// 튜토리얼 화살표.
/// 목적지까지 거리가 멀면 → 플레이어 앞 방향 지시
/// 목적지에 근접하면    → 목적지 위 바운스 표시
/// </summary>
public class TutorialArrow : MonoBehaviour
{
    public enum ArrowMode { Direction, Destination }

    [Header("Mode Settings")]
    [SerializeField] private float switchDistance = 5f;   // 이 거리 이하면 목적지 위 표시

    [Header("Direction Mode (플레이어 앞)")]
    [SerializeField] private float directionHeight = 1.8f;  // 플레이어 기준 높이
    [SerializeField] private float directionForward = 1.2f;  // 플레이어 앞 거리

    [Header("Destination Mode (목적지 위)")]
    [SerializeField] private float bounceHeight = 0.4f;
    [SerializeField] private float bounceSpeed = 2.0f;
    [SerializeField] private float heightOffset = 2.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    private Transform _target;
    private Transform _player;
    private Renderer[] _renderers;
    private Camera _mainCamera;
    private ArrowMode _currentMode;
    private bool _isVisible;
    private float _bounceTimer;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _mainCamera = Camera.main;
        _player = FindFirstObjectByType<PlayerMovement>()?.transform;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_target == null || _player == null) return;

        float dist = Vector3.Distance(
            new Vector3(_player.position.x, 0f, _player.position.z),
            new Vector3(_target.position.x, 0f, _target.position.z));

        // 거리에 따라 모드 전환
        ArrowMode newMode = dist > switchDistance
            ? ArrowMode.Direction
            : ArrowMode.Destination;

        if (newMode != _currentMode)
            _currentMode = newMode;

        switch (_currentMode)
        {
            case ArrowMode.Direction:
                UpdateDirectionMode();
                break;
            case ArrowMode.Destination:
                UpdateDestinationMode();
                break;
        }
    }

    // ── Direction Mode — 플레이어 앞에서 목적지 방향 가리킴 ─────────────
    private void UpdateDirectionMode()
    {
        // 플레이어 → 목적지 방향 (XZ 평면)
        Vector3 dir = _target.position - _player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        // 플레이어 앞 고정 위치
        transform.position = _player.position
            + dir * directionForward
            + Vector3.up * directionHeight;

        // 화살표가 목적지 방향을 가리키도록 회전
        // 화살표 모델의 앞면(Z+)이 방향을 가리킨다고 가정
        transform.rotation = Quaternion.LookRotation(dir)
            * Quaternion.Euler(-90f, 0f, 0f);
    }

    // ── Destination Mode — 목적지 위에서 바운스 ──────────────────────────
    private void UpdateDestinationMode()
    {
        _bounceTimer += Time.deltaTime * bounceSpeed;
        float bounce = Mathf.Sin(_bounceTimer) * bounceHeight;

        transform.position = _target.position
            + Vector3.up * (heightOffset + bounce);

        // 카메라 빌보드 — 항상 카메라를 바라봄
        transform.rotation = _mainCamera.transform.rotation;
    }

    // ── 외부 호출 ────────────────────────────────────────────────────────

    public void Show(Transform target)
    {
        _target = target;
        _bounceTimer = 0f;
        gameObject.SetActive(true);

        if (_isVisible) return;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(true));
    }

    public void Hide()
    {
        if (!_isVisible) return;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(false));
    }

    // ── 페이드 ───────────────────────────────────────────────────────────
    private IEnumerator FadeRoutine(bool fadeIn)
    {
        _isVisible = fadeIn;

        float from = fadeIn ? 0f : 1f;
        float to = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        SetAlpha(from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(to);

        if (!fadeIn)
            gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer rend in _renderers)
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
    }
}