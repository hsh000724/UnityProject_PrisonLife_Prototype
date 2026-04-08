using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 감옥 내 죄수.
/// NavMesh 없이 Transform 직접 이동.
/// 입구 대기 → 셀 배정 → 침대 이동 → 눕기.
/// </summary>
public class PrisonPrisoner : MonoBehaviour
{
    public enum State { WalkingToPrison, WaitingAtQueue, MovingToCell, InBed }

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float arrivalThreshold = 0.08f;
    [SerializeField] private float lieDownDuration = 0.6f;

    public State CurrentState { get; private set; }

    private Animator _animator;
    private bool _hasAnimator;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private bool _isMoving;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SleepHash = Animator.StringToHash("IsSleeping");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null
            && _animator.runtimeAnimatorController != null;
    }

    private void Update()
    {
        if (!_isMoving) return;

        MoveToTarget();
        CheckArrival();
        UpdateAnimator();
    }

    // ── 이동 처리 ────────────────────────────────────────────────────────
    private void MoveToTarget()
    {
        Vector3 flatTarget = new Vector3(
            _targetPosition.x,
            transform.position.y,  // 수직 이동 방지
            _targetPosition.z);

        Vector3 dir = (flatTarget - transform.position).normalized;

        transform.position += dir * moveSpeed * Time.deltaTime;

        // 이동 방향으로 회전
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ── 도착 체크 ────────────────────────────────────────────────────────
    private void CheckArrival()
    {
        Vector3 flatCurrent = new Vector3(
            transform.position.x, 0f, transform.position.z);
        Vector3 flatTarget = new Vector3(
            _targetPosition.x, 0f, _targetPosition.z);

        if (Vector3.Distance(flatCurrent, flatTarget) > arrivalThreshold) return;

        _isMoving = false;
        OnArrived();
    }

    private void OnArrived()
    {
        switch (CurrentState)
        {
            case State.WalkingToPrison:
                CurrentState = State.WaitingAtQueue;
                SetAnimatorSpeed(0f);
                PrisonManager.Instance?.OnPrisonerArrivedAtQueue(this);
                break;

            case State.MovingToCell:
                StartCoroutine(LieDownRoutine());
                break;
        }
    }

    // ── 감옥 입구로 이동 시작 ────────────────────────────────────────────
    public void WalkToPrison(Vector3 queuePosition)
    {
        CurrentState = State.WalkingToPrison;
        _targetPosition = queuePosition;
        _isMoving = true;
    }

    // ── 셀 배정 → 침대로 이동 ───────────────────────────────────────────
    public void AssignCell(Vector3 sleepPosition, Quaternion sleepRotation)
    {
        _targetPosition = sleepPosition;
        _targetRotation = sleepRotation;
        CurrentState = State.MovingToCell;
        _isMoving = true;
    }

    // ── 침대에 눕기 ─────────────────────────────────────────────────────
    private IEnumerator LieDownRoutine()
    {
        CurrentState = State.InBed;
        SetAnimatorSpeed(0f);

        // 침대 위치로 정확하게 스냅
        transform.position = _targetPosition;

        // 회전 보간 — 침대 방향으로 눕기
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;

        while (elapsed < lieDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lieDownDuration);
            transform.rotation = Quaternion.Slerp(
                startRot, _targetRotation, t);
            yield return null;
        }

        transform.rotation = _targetRotation;

        if (_hasAnimator)
            _animator.SetBool(SleepHash, true);
    }

    // ── 애니메이터 ───────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        if (!_hasAnimator) return;
        SetAnimatorSpeed(_isMoving ? 1f : 0f);
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (_hasAnimator)
            _animator.SetFloat(SpeedHash, speed);
    }
}