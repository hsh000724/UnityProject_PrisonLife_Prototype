using UnityEngine;

/// <summary>
/// 전체 해금 조건을 감지하고 해금존을 활성화하는 매니저.
/// Update에서 조건을 폴링하거나 이벤트로 수신.
/// </summary>
public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    [Header("Drill Zone")]
    [SerializeField] private GameObject drillZoneObject;        // 최초 돈 획득 시 해금

    [Header("Drill Bulldozer Zone")]
    [SerializeField] private GameObject drillBulldozerZoneObject; // 드릴 구매 후 해금

    [Header("Miner Zone")]
    [SerializeField] private GameObject minerZoneObject;         // 드릴 구매 후 해금

    [Header("Cashier Zone")]
    [SerializeField] private GameObject cashierZoneObject;       // 광부 해금 후 해금

    [Header("Prison Zone")]
    [SerializeField] private GameObject prisonZoneObject;        // 감옥 가득 찼을 때 해금

    // 해금 상태 플래그
    private bool _drillZoneUnlocked;
    private bool _drillBulldozerZoneUnlocked;
    private bool _minerZoneUnlocked;
    private bool _cashierZoneUnlocked;
    private bool _prisonZoneUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 모든 해금존 초기 비활성
        SetActive(drillZoneObject, false);
        SetActive(drillBulldozerZoneObject, false);
        SetActive(minerZoneObject, false);
        SetActive(cashierZoneObject, false);
        SetActive(prisonZoneObject, false);
    }

    // ── 외부 트리거 — 각 조건 달성 시 호출 ─────────────────────────────

    /// <summary>최초 돈 획득 시 — PlayerWallet.AddMoney()에서 호출</summary>
    public void NotifyFirstMoneyEarned()
    {
        if (_drillZoneUnlocked) return;
        _drillZoneUnlocked = true;

        SetActive(drillZoneObject, true);
        Notify("Drill Zone Unlocked!");
    }

    /// <summary>드릴 구매 완료 시 — DrillZone.OnUnlocked()에서 호출</summary>
    public void NotifyDrillUnlocked()
    {
        if (!_drillBulldozerZoneUnlocked)
        {
            _drillBulldozerZoneUnlocked = true;
            SetActive(drillBulldozerZoneObject, true);
            Notify("Drill Bulldozer Zone Unlocked!");
        }

        if (!_minerZoneUnlocked)
        {
            _minerZoneUnlocked = true;
            SetActive(minerZoneObject, true);
            Notify("Miner Zone Unlocked!");
        }
    }

    /// <summary>광부 해금 완료 시 — MinerZone.OnUnlocked()에서 호출</summary>
    public void NotifyMinerUnlocked()
    {
        if (_cashierZoneUnlocked) return;
        _cashierZoneUnlocked = true;

        SetActive(cashierZoneObject, true);
        Notify("Cashier Zone Unlocked!");
    }

    /// <summary>감옥 가득 찼을 때 — PrisonManager에서 호출</summary>
    public void NotifyPrisonFull()
    {
        if (_prisonZoneUnlocked) return;
        _prisonZoneUnlocked = true;

        SetActive(prisonZoneObject, true);
        Notify("Prison Zone Unlocked!");
    }

    // ── 헬퍼 ─────────────────────────────────────────────────────────────
    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    private void Notify(string message)
    {
        UnlockNoticeUI.Instance?.Show(message);

        FloatingTextPool.Instance?.Spawn(
            message,
            Camera.main.transform.position + Camera.main.transform.forward * 3f,
            Color.cyan);
    }
}