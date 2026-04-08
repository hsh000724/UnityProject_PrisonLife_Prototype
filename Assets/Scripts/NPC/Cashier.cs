using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 캐셔 AI.
/// HandcuffZone → Counter 왕복하며 수갑을 자동 운반.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Cashier : MonoBehaviour
{
    public enum State { Idle, GoingToHandcuffZone, PickingUp, GoingToCounter, Delivering }

    [Header("References")]
    [SerializeField] private Transform handcuffZonePoint; // HandcuffZone 이동 목표
    [SerializeField] private Transform counterPoint;      // Counter 이동 목표
    [SerializeField] private HandcuffZone handcuffZone;
    [SerializeField] private Counter counter;

    [Header("Settings")]
    [SerializeField] private float arrivalThreshold = 0.4f;
    [SerializeField] private float pickupDelay = 0.5f;  // 픽업 딜레이
    [SerializeField] private float deliverDelay = 0.5f;  // 전달 딜레이
    [SerializeField] private int carryAmount = 5;     // 1회 운반 수량

    [Header("Carry Visual")]
    [SerializeField] private Transform carryRoot;
    [SerializeField] private GameObject handcuffVisualPrefab;
    [SerializeField] private Vector3 carryScale = new Vector3(0.03f, 0.25f, 0.03f);
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.27f, 0f);

    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _hasAnimator;
    private State _state;
    private int _carryCount;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null
            && _animator.runtimeAnimatorController != null;
    }

    private void OnEnable()
    {
        SetState(State.GoingToHandcuffZone);
        _agent.SetDestination(handcuffZonePoint.position);
    }

    private void Update()
    {
        UpdateAnimator();

        if (_state != State.GoingToHandcuffZone &&
            _state != State.GoingToCounter) return;

        if (_agent.pathPending) return;
        if (_agent.remainingDistance > arrivalThreshold) return;

        switch (_state)
        {
            case State.GoingToHandcuffZone:
                StartCoroutine(PickupRoutine());
                break;
            case State.GoingToCounter:
                StartCoroutine(DeliverRoutine());
                break;
        }
    }

    // ── HandcuffZone 도착 — 픽업 ─────────────────────────────────────────
    private IEnumerator PickupRoutine()
    {
        SetState(State.PickingUp);
        _agent.ResetPath();

        yield return new WaitForSeconds(pickupDelay);

        // HandcuffZone 재고 확인
        if (handcuffZone.TotalCount <= 0)
        {
            // 재고 없으면 잠시 대기 후 재시도
            yield return new WaitForSeconds(1f);
            SetState(State.GoingToHandcuffZone);
            _agent.SetDestination(handcuffZonePoint.position);
            yield break;
        }

        int pickup = Mathf.Min(carryAmount, handcuffZone.TotalCount);
        handcuffZone.CashierPickup(pickup);
        _carryCount = pickup;
        SpawnCarryVisuals();

        SetState(State.GoingToCounter);
        _agent.SetDestination(counterPoint.position);
    }

    // ── Counter 도착 — 전달 ──────────────────────────────────────────────
    private IEnumerator DeliverRoutine()
    {
        SetState(State.Delivering);
        _agent.ResetPath();

        yield return new WaitForSeconds(deliverDelay);

        counter?.AddHandcuffs(_carryCount);

        FloatingTextPool.Instance?.Spawn(
            $"+{_carryCount} Delivered",
            transform.position + Vector3.up * 1.5f,
            Color.cyan);

        _carryCount = 0;
        ClearCarryVisuals();

        // 다시 HandcuffZone으로
        SetState(State.GoingToHandcuffZone);
        _agent.SetDestination(handcuffZonePoint.position);
    }

    // ── 비주얼 ──────────────────────────────────────────────────────────
    private void SpawnCarryVisuals()
    {
        if (handcuffVisualPrefab == null || carryRoot == null) return;

        for (int i = 0; i < _carryCount; i++)
        {
            GameObject visual = Instantiate(handcuffVisualPrefab, carryRoot);
            visual.transform.localPosition = stackOffset * i;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = carryScale;
        }
    }

    private void ClearCarryVisuals()
    {
        if (carryRoot == null) return;

        foreach (Transform child in carryRoot)
            Destroy(child.gameObject);
    }

    private void SetState(State newState) => _state = newState;

    private void UpdateAnimator()
    {
        if (!_hasAnimator) return;
        _animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
    }
}