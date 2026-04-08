using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 감옥 수용 관리.
/// 죄수 도착 → 입구 대기열 관리 → PrisonZone 해금 시 셀 배정.
/// </summary>
public class PrisonManager : MonoBehaviour
{
    public static PrisonManager Instance { get; private set; }

    [Header("Capacity")]
    [SerializeField] private int maxCapacity = 10;

    [Header("Prison Cells")]
    [SerializeField] private PrisonCell[] cells;

    [Header("Queue Settings")]
    [SerializeField] private Transform[] queueSlots;     // 입구 앞 대기 슬롯
    [SerializeField] private Transform prisonEntrance;   // 대기열 기준 위치

    [Header("Camera POI")]
    [SerializeField] private Transform fullPrisonCameraPoint;
    [SerializeField] private Transform clearPrisonCameraPoint;

    [Header("Prison Prisoner")]
    [SerializeField] private PrisonPrisoner prisonPrisonerPrefab;

    private int _totalReceived = 0;
    private bool _isFull = false;
    private bool _isUnlocked = false;

    // 대기 중인 죄수 큐
    private readonly Queue<PrisonPrisoner> _waitingQueue
        = new Queue<PrisonPrisoner>();

    // 셀 배정 대기 중인 죄수 리스트
    private readonly List<PrisonPrisoner> _allPrisoners
        = new List<PrisonPrisoner>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── 죄수 수용 (PrisonerQueue → 호출) ────────────────────────────────
    public void ReceivePrisoner()
    {
        _totalReceived++;

        // 대기 슬롯 위치 계산
        int slotIndex = Mathf.Min(
            _waitingQueue.Count, queueSlots.Length - 1);
        Vector3 queuePos = queueSlots[slotIndex].position;

        // PrisonPrisoner 스폰 + 감옥 입구로 이동
        PrisonPrisoner pp = Instantiate(
            prisonPrisonerPrefab,
            prisonEntrance.position,
            prisonEntrance.rotation);

        _allPrisoners.Add(pp);
        _waitingQueue.Enqueue(pp);
        pp.WalkToPrison(queuePos);

        // 감옥 가득 참 체크
        if (!_isFull && _totalReceived >= maxCapacity)
        {
            _isFull = true;
            StartCoroutine(PrisonFullSequence());
        }
    }

    // ── 죄수가 대기열 도착 (PrisonPrisoner → 콜백) ──────────────────────
    public void OnPrisonerArrivedAtQueue(PrisonPrisoner prisoner)
    {
        // 이미 해금된 상태면 바로 셀 배정
        if (_isUnlocked)
            AssignCellToPrisoner(prisoner);
    }

    // ── 감옥존 해금 시 호출 ──────────────────────────────────────────────
    public void OnPrisonZoneUnlocked()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;

        // 전체 셀 활성화
        foreach (PrisonCell cell in cells)
            cell.UnlockCell();

        // 대기 중인 죄수 전원 순차 셀 배정
        StartCoroutine(AssignWaitingPrisonersRoutine());

        // 게임 클리어 시퀀스
        StartCoroutine(GameClearSequence());
    }

    // ── 대기 죄수 순차 배정 ──────────────────────────────────────────────
    private IEnumerator AssignWaitingPrisonersRoutine()
    {
        while (_waitingQueue.Count > 0)
        {
            PrisonPrisoner pp = _waitingQueue.Dequeue();

            if (pp != null &&
                pp.CurrentState == PrisonPrisoner.State.WaitingAtQueue)
            {
                AssignCellToPrisoner(pp);
                yield return new WaitForSeconds(0.3f); // 순차 배정 딜레이
            }
        }
    }

    // ── 개별 셀 배정 ────────────────────────────────────────────────────
    private void AssignCellToPrisoner(PrisonPrisoner prisoner)
    {
        PrisonCell emptyCell = GetEmptyCell();
        if (emptyCell == null) return;

        emptyCell.Assign(
            out Vector3 sleepPos,
            out Quaternion sleepRot);

        prisoner.AssignCell(sleepPos, sleepRot);
    }

    private PrisonCell GetEmptyCell()
    {
        foreach (PrisonCell cell in cells)
            if (!cell.IsOccupied && cell.IsUnlocked) return cell;
        return null;
    }

    // ── 감옥 가득 참 연출 ────────────────────────────────────────────────
    private IEnumerator PrisonFullSequence()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);

        CameraController.Instance?.MoveToPOIAndReturn(
            fullPrisonCameraPoint,
            onComplete: () =>
            {
                Time.timeScale = 1f;
                UnlockManager.Instance?.NotifyPrisonFull();
            });
    }

    // ── 게임 클리어 ──────────────────────────────────────────────────────
    private IEnumerator GameClearSequence()
    {
        yield return new WaitForSeconds(2.0f);

        FindFirstObjectByType<PlayerMovement>()?.SetPause(true);

        CameraController.Instance?.MoveToPOIAndReturn(
            clearPrisonCameraPoint,
            onComplete: () => GameClearUI.Instance?.Show());
    }
}