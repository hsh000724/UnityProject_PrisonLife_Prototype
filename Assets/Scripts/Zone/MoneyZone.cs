using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 죄수가 지불한 돈을 오브젝트로 적재.
/// 플레이어 진입 시 적재된 돈을 PlayerWallet에 전달 + 등 뒤 스택으로 이동.
/// 금액 표시는 PlayerWallet에서 전담.
/// </summary>
public class MoneyZone : MonoBehaviour
{
    [Header("Stack Visual")]
    [SerializeField] private Transform stackRoot;
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private Vector3 moneyScale = new Vector3(0.3f, 0.05f, 0.3f);
    [SerializeField] private int maxVisualStack = 20;

    private readonly List<GameObject> _moneyStack = new List<GameObject>();
    private int _totalMoney = 0;

    public int TotalMoney => _totalMoney;

    // ── 죄수 지불 시 호출 ────────────────────────────────────────────────
    public void AddMoney(int amount)
    {
        _totalMoney += amount;

        for (int i = 0; i < amount; i++)
        {
            if (_moneyStack.Count < maxVisualStack)
                SpawnMoneyVisual();
        }
    }

    // ── 플레이어 진입 — 픽업 ────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_totalMoney <= 0) return;

        PlayerWallet wallet = other.GetComponent<PlayerWallet>();
        PlayerMoneyStack moneyStack = other.GetComponent<PlayerMoneyStack>();
        if (wallet == null) return;

        wallet.AddMoney(_totalMoney);
        TutorialManager.Instance?.NotifyMoneyPickedUp();
        moneyStack?.AddMoneyVisuals(_moneyStack.Count, moneyPrefab);

        FloatingTextPool.Instance?.Spawn(
            $"+${_totalMoney} Picked Up",
            other.transform.position + Vector3.up * 2f,
            Color.yellow);

        ClearStack();
        SoundManager.Instance?.Play(SoundManager.SFX.MoneyPickup);
    }

    // ── 비주얼 ──────────────────────────────────────────────────────────
    private void SpawnMoneyVisual()
    {
        if (moneyPrefab == null || stackRoot == null) return;

        GameObject visual = Instantiate(moneyPrefab, stackRoot);
        visual.transform.localPosition = stackOffset * _moneyStack.Count;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = moneyScale;
        _moneyStack.Add(visual);
    }

    private void ClearStack()
    {
        foreach (GameObject m in _moneyStack)
            if (m != null) Destroy(m);

        _moneyStack.Clear();
        _totalMoney = 0;
    }
}