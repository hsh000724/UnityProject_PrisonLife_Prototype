using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어 보유 금액 관리.
/// 최초 AddMoney() 시 UnlockManager에 통보.
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI walletText;

    [Header("Unlockables")]
    [SerializeField] private GameObject drillZoneObject;

    public int Money { get; private set; }
    private bool _hasReceivedMoney;
    private PlayerMoneyStack _moneyStack;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _moneyStack = GetComponent<PlayerMoneyStack>();
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        UpdateUI();

        // 최초 돈 획득 시
        if (!_hasReceivedMoney && Money > 0)
        {
            _hasReceivedMoney = true;

            // 1. 매니저 통보
            UnlockManager.Instance?.NotifyFirstMoneyEarned();

            // 2. 드릴 존 해금 실행 (여기서 호출!)
            UnlockDrillZone();
        }
    }

    private void UnlockDrillZone()
    {
        if (drillZoneObject == null) return;

        // 이 코드 한 줄로 DrillZone의 OnEnable()이 실행되어 카메라가 움직입니다.
        drillZoneObject.SetActive(true);

        // 공통 UI 연출
        UnlockNoticeUI.Instance?.Show("Drill Zone Unlocked!");
        FloatingTextPool.Instance?.Spawn(
            "Drill Zone Unlocked!",
            transform.position + Vector3.up * 2.5f,
            Color.cyan);
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount) return false;

        Money -= amount;
        _moneyStack?.RemoveMoneyVisuals(amount);
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (walletText != null)
            walletText.text = $"${Money}";
    }
}