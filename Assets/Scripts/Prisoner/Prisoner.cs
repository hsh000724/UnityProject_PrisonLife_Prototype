using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 죄수 상태머신.
/// Spawning → MovingToQueue → WaitingAtCounter → Served → MovingToPrison
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Prisoner : MonoBehaviour
{
    public enum State { Spawning, MovingToQueue, WaitingAtCounter, Served, MovingToPrison }

    [Header("Settings")]
    [SerializeField] private float arrivalThreshold = 0.3f;  // 목적지 도착 판정 거리

    public int Demand { get; private set; }
    public State CurrentState { get; private set; }

    [SerializeField] private DemandBubbleUI bubbleUI;

    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _hasAnimator;
    private int _currentReceived = 0;
    private Transform _prisonTarget;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null && _animator.runtimeAnimatorController != null;

        // DemandBubbleUI에 자신의 루트 Transform 주입
        if (bubbleUI != null)
            bubbleUI.SetOwner(transform);
    }

    private void Update()
    {
        UpdateAnimator();

        switch (CurrentState)
        {
            case State.MovingToQueue:
            case State.MovingToPrison:
                CheckArrival();
                break;
        }
    }

    // ── 초기화 ───────────────────────────────────────────────────────────
    public void Initialize(int demandMin, int demandMax)
    {
        Demand = Random.Range(demandMin, demandMax + 1);
        _currentReceived = 0;
        bubbleUI.SetActive(false);
        bubbleUI.UpdateGauge(0, Demand);
        SetState(State.Spawning);
    }

    // ── 큐 이동 ──────────────────────────────────────────────────────────
    public void MoveToQueueSlot(Vector3 slotPosition)
    {
        SetState(State.MovingToQueue);
        _agent.SetDestination(slotPosition);
    }

    // ── 카운터 도착 후 대기 ──────────────────────────────────────────────
    public void SetWaitingAtCounter(bool isFront)
    {
        SetState(State.WaitingAtCounter);
        _agent.ResetPath();
        ActivateBubble(isFront);
    }

    // ── 말풍선 활성화 ────────────────────────────────────────────────────
    public void ActivateBubble(bool active)
    {
        bubbleUI.SetActive(active);
    }

    // ── 수갑 납품 수신 ───────────────────────────────────────────────────
    /// <returns>요구량 충족 여부</returns>
    public bool ReceiveHandcuffs(int amount, out int moneyEarned, int moneyPerHandcuff)
    {
        _currentReceived = Mathf.Min(_currentReceived + amount, Demand);
        bubbleUI.UpdateGauge(_currentReceived, Demand);

        if (_currentReceived >= Demand)
        {
            moneyEarned = Demand * moneyPerHandcuff;
            return true;
        }

        moneyEarned = 0;
        return false;
    }

    // ── 감옥으로 이동 ────────────────────────────────────────────────────
    public void MoveToPrison(Transform prisonTarget)
    {
        _prisonTarget = prisonTarget;
        ActivateBubble(false);
        SetState(State.MovingToPrison);
        _agent.SetDestination(prisonTarget.position);
    }

    // ── 도착 체크 ────────────────────────────────────────────────────────
    private void CheckArrival()
    {
        if (_agent.pathPending) return;
        if (_agent.remainingDistance > arrivalThreshold) return;

        switch (CurrentState)
        {
            case State.MovingToQueue:
                // 큐 슬롯 도착 — PrisonerQueue가 상태 전환 처리
                PrisonerQueue.Instance?.OnPrisonerArrivedAtSlot(this);
                break;

            case State.MovingToPrison:
                SetState(State.Served);
                PrisonerQueue.Instance?.OnPrisonerArrivedAtPrison(this);
                break;
        }
    }

    // ── 상태 전환 ────────────────────────────────────────────────────────
    private void SetState(State newState)
    {
        CurrentState = newState;
    }

    // ── 애니메이터 ───────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        if (!_hasAnimator) return;
        _animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
    }
}