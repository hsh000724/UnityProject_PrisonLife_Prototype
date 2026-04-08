using System.Collections;
using UnityEngine;

/// <summary>
/// 튜토리얼 화살표 단계 관리.
/// 단계별 조건 충족 시 다음 단계로 진행.
/// 모든 단계 완료 후 화살표 영구 제거.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public enum Step
    {
        Mining,         // 1. 광물 채광 유도
        OreZone,        // 2. 광물 5개 채광 후 OreZone 안내
        HandcuffZone,   // 3. HandcuffZone 안내
        CounterZone,    // 4. CounterZone 안내
        MoneyZone,      // 5. MoneyZone 안내
        HandcuffRestock,// 6. HandcuffZone 재안내 (일정량 쌓이면)
        Done            // 완료
    }

    [Header("Arrow")]
    [SerializeField] private TutorialArrow arrow;

    [Header("Targets")]
    [SerializeField] private Transform mineTarget;        // 채광 유도 광물 위치
    [SerializeField] private Transform oreZoneTarget;
    [SerializeField] private Transform handcuffZoneTarget;
    [SerializeField] private Transform counterZoneTarget;
    [SerializeField] private Transform moneyZoneTarget;

    [Header("Settings")]
    [SerializeField] private int miningGoal = 5;   // 2단계 진입 채광 수
    [SerializeField] private int handcuffRestockMin = 5;   // 6단계 진입 수갑 재고

    [Header("References")]
    [SerializeField] private HandcuffZone handcuffZone;

    public Step CurrentStep { get; private set; } = Step.Mining;

    // 내부 카운터
    private int _mineCount = 0;
    private bool _step6Triggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GoToStep(Step.Mining);
    }

    private void Update()
    {
        // 6단계 — HandcuffZone 재고 폴링
        if (CurrentStep == Step.Done) return;
        if (!_step6Triggered
            && CurrentStep > Step.MoneyZone
            && handcuffZone != null
            && handcuffZone.TotalCount >= handcuffRestockMin)
        {
            _step6Triggered = true;
            GoToStep(Step.HandcuffRestock);
        }
    }

    // ── 단계 전환 ────────────────────────────────────────────────────────
    private void GoToStep(Step step)
    {
        CurrentStep = step;

        switch (step)
        {
            case Step.Mining:
                arrow.Show(mineTarget);
                break;

            case Step.OreZone:
                arrow.Show(oreZoneTarget);
                break;

            case Step.HandcuffZone:
                arrow.Show(handcuffZoneTarget);
                break;

            case Step.CounterZone:
                arrow.Show(counterZoneTarget);
                break;

            case Step.MoneyZone:
                arrow.Show(moneyZoneTarget);
                break;

            case Step.HandcuffRestock:
                arrow.Show(handcuffZoneTarget);
                break;

            case Step.Done:
                arrow.Hide();
                break;
        }
    }

    // ── 외부 이벤트 수신 ─────────────────────────────────────────────────

    /// <summary>
    /// 광물 1개 채광 성공 시 PlayerMining에서 호출.
    /// </summary>
    public void NotifyMined()
    {
        if (CurrentStep == Step.Mining)
        {
            // 첫 채광 성공 → 화살표 숨김
            arrow.Hide();
            _mineCount++;

            if (_mineCount >= miningGoal)
                GoToStep(Step.OreZone);
            else
                GoToStep(Step.Mining); // 다음 광물로 재안내
        }
        else if (CurrentStep > Step.Mining)
        {
            _mineCount++;
        }
    }

    /// <summary>
    /// OreZone에 광물 투입 시 OreZone.cs에서 호출.
    /// </summary>
    public void NotifyOreDelivered()
    {
        if (CurrentStep != Step.OreZone) return;
        GoToStep(Step.HandcuffZone);
    }

    /// <summary>
    /// HandcuffZone에서 수갑 픽업 시 HandcuffZone.cs에서 호출.
    /// </summary>
    public void NotifyHandcuffPickedUp()
    {
        if (CurrentStep == Step.HandcuffZone)
        {
            GoToStep(Step.CounterZone);
            return;
        }

        if (CurrentStep == Step.HandcuffRestock)
            GoToStep(Step.CounterZone);
    }

    /// <summary>
    /// CounterZone에 수갑 납품 시 CounterZone.cs에서 호출.
    /// </summary>
    public void NotifyHandcuffDelivered()
    {
        if (CurrentStep != Step.CounterZone) return;
        GoToStep(Step.MoneyZone);
    }

    /// <summary>
    /// MoneyZone에서 돈 픽업 시 MoneyZone.cs에서 호출.
    /// </summary>
    public void NotifyMoneyPickedUp()
    {
        if (CurrentStep != Step.MoneyZone) return;
        GoToStep(Step.Done);
    }
}