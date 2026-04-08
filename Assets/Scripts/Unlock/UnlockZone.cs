using System.Collections;
using UnityEngine;

public class UnlockZone : MonoBehaviour
{
    [Header("Unlock Settings")]
    [SerializeField] protected int requiredAmount;
    [SerializeField] private float payInterval = 0.05f;
    [SerializeField] private int payAmountPerTick = 1;

    [Header("UI")]
    [SerializeField] private UnlockZoneUI zoneUI;

    [Header("Unlock Objects")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    public bool IsUnlocked { get; private set; }

    private int _paidAmount = 0;
    private Coroutine _payCoroutine;

    protected virtual void Start()
    {
        zoneUI?.Init(requiredAmount);
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 코루틴 안전 종료
        StopPayCoroutine();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsUnlocked) return;
        if (!other.CompareTag("Player")) return;

        _payCoroutine = StartCoroutine(
            PayRoutine(other.GetComponent<PlayerWallet>()));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        StopPayCoroutine();
    }

    private IEnumerator PayRoutine(PlayerWallet wallet)
    {
        if (wallet == null) yield break;

        while (_paidAmount < requiredAmount)
        {
            // timeScale = 0 상태에서도 동작하도록 Realtime 사용
            yield return new WaitForSecondsRealtime(payInterval);

            int pay = Mathf.Min(payAmountPerTick, requiredAmount - _paidAmount);
            if (!wallet.SpendMoney(pay)) yield break;

            _paidAmount += pay;
            zoneUI?.UpdateProgress(_paidAmount);

            if (_paidAmount >= requiredAmount)
            {
                Unlock();
                yield break;
            }
        }
    }

    private void Unlock()
    {
        IsUnlocked = true;

        // UI 완료 연출 + 딜레이 후 숨김 — zoneUI가 코루틴 직접 실행
        zoneUI?.SetComplete();
        zoneUI?.HideAfterDelay(1.5f);   // ← zoneUI에서 코루틴 실행

        UnlockNoticeUI.Instance?.Show(GetUnlockMessage());
        OnUnlocked();

        // objectsToDisable은 가장 마지막에 처리
        // 자신이 꺼지기 전에 모든 로직 완료
        foreach (GameObject obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);

        foreach (GameObject obj in objectsToDisable)
            if (obj != null) obj.SetActive(false);

        SoundManager.Instance?.Play(SoundManager.SFX.ZoneUnlock);
    }

    private void StopPayCoroutine()
    {
        if (_payCoroutine != null)
        {
            StopCoroutine(_payCoroutine);
            _payCoroutine = null;
        }
    }

    protected virtual void OnUnlocked() { }
    protected virtual string GetUnlockMessage() => "Unlocked!";
}