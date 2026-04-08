using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 죄수 대기열 관리.
/// CounterZone에서 1개씩 ConsumeOneHandcuff()를 호출하는 방식.
/// </summary>
public class PrisonerQueue : MonoBehaviour
{
    public static PrisonerQueue Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PrisonerData data;
    [SerializeField] private PrisonerSpawner spawner;
    [SerializeField] private Transform prisonTarget;

    [Header("Counter Slot")]
    [SerializeField] private Transform counterSlot;

    [Header("Waiting Area")]
    [SerializeField] private Transform[] waitingSlots;
    [SerializeField] private int maxWaitingCount = 5;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f;

    // 카운터 앞 죄수 (1명)
    public Prisoner FrontPrisoner { get; private set; }

    // 대기 중인 죄수 큐
    private readonly Queue<Prisoner> _waitingQueue = new Queue<Prisoner>();

    private int TotalCount =>
        (FrontPrisoner != null ? 1 : 0) + _waitingQueue.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(AutoSpawnRoutine());
    }

    // ── 자동 스폰 루틴 ───────────────────────────────────────────────────
    private IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (TotalCount < maxWaitingCount)
                SpawnOne();
        }
    }

    private void SpawnOne()
    {
        Prisoner p = spawner.Spawn();
        if (p == null) return;

        p.Initialize(data.demandMin, data.demandMax);

        if (FrontPrisoner == null)
            SendToCounter(p);
        else
        {
            int slotIndex = _waitingQueue.Count;
            Vector3 waitPos = slotIndex < waitingSlots.Length
                ? waitingSlots[slotIndex].position
                : waitingSlots[waitingSlots.Length - 1].position;

            _waitingQueue.Enqueue(p);
            p.MoveToQueueSlot(waitPos);
        }
    }

    private void SendToCounter(Prisoner prisoner)
    {
        FrontPrisoner = prisoner;
        prisoner.MoveToQueueSlot(counterSlot.position);
    }

    // ── 슬롯 도착 콜백 ───────────────────────────────────────────────────
    public void OnPrisonerArrivedAtSlot(Prisoner prisoner)
    {
        bool isFront = (prisoner == FrontPrisoner);
        prisoner.SetWaitingAtCounter(isFront);
    }

    // ── 카운터에서 수갑 1개 소모 (CounterZone 호출) ──────────────────────
    public void ConsumeOneHandcuff(MoneyZone moneyZone)
    {
        if (FrontPrisoner == null) return;

        bool fulfilled = FrontPrisoner.ReceiveHandcuffs(
            1, out int earned, data.moneyPerHandcuff);

        if (!fulfilled) return;

        // 요구량 충족
        moneyZone.AddMoney(earned);

        FloatingTextPool.Instance?.Spawn(
            $"+${earned} Earned!",
            FrontPrisoner.transform.position + Vector3.up * 2f,
            Color.yellow);

        FrontPrisoner.MoveToPrison(prisonTarget);
        FrontPrisoner = null;

        AdvanceToCounter();
    }

    // ── 다음 죄수 카운터로 ───────────────────────────────────────────────
    private void AdvanceToCounter()
    {
        if (_waitingQueue.Count == 0) return;

        Prisoner next = _waitingQueue.Dequeue();
        SendToCounter(next);
        RearrangeWaitingSlots();
    }

    private void RearrangeWaitingSlots()
    {
        int index = 0;
        foreach (Prisoner p in _waitingQueue)
        {
            Vector3 targetPos = index < waitingSlots.Length
                ? waitingSlots[index].position
                : waitingSlots[waitingSlots.Length - 1].position;

            p.MoveToQueueSlot(targetPos);
            index++;
        }
    }

    // ── 감옥 도착 콜백 ───────────────────────────────────────────────────
    public void OnPrisonerArrivedAtPrison(Prisoner prisoner)
    {
        // 감옥 수용 카운트 증가
        PrisonManager.Instance?.ReceivePrisoner();

        spawner.Return(prisoner);

        if (TotalCount < maxWaitingCount)
            SpawnOne();
    }
}