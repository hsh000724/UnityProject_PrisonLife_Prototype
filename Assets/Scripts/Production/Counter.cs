using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카운터 수갑 재고 관리 + 비주얼 + 죄수 소모 루틴.
/// CounterZone에서 적재 요청, PrisonerQueue에 1개씩 소모.
/// </summary>
public class Counter : MonoBehaviour
{
    public static Counter Instance { get; private set; }

    [Header("References")]
    [SerializeField] private MoneyZone moneyZone;

    [Header("Stack Visual")]
    [SerializeField] private Transform stackRoot;
    [SerializeField] private GameObject handcuffVisualPrefab;
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.15f, 0f);
    [SerializeField] private Vector3 stackScale = new Vector3(0.03f, 0.25f, 0.03f);

    [Header("Consume Settings")]
    [SerializeField] private float consumeInterval = 0.4f;

    public int StackedCount { get; private set; }

    private readonly List<GameObject> _stackVisuals = new List<GameObject>();
    private Coroutine _consumeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── 외부 적재 요청 (CounterZone → 호출) ─────────────────────────────
    public void AddHandcuffs(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            StackedCount++;
            SpawnStackVisual();
        }

        if (_consumeCoroutine == null)
            _consumeCoroutine = StartCoroutine(ConsumeRoutine());
    }

    // ── 죄수에게 1개씩 소모 루틴 ────────────────────────────────────────
    private IEnumerator ConsumeRoutine()
    {
        while (StackedCount > 0)
        {
            yield return new WaitForSeconds(consumeInterval);

            // 앞 죄수가 없으면 소모하지 않고 대기
            if (PrisonerQueue.Instance?.FrontPrisoner == null)
                continue;

            StackedCount--;
            RemoveStackVisual();

            PrisonerQueue.Instance.ConsumeOneHandcuff(moneyZone);

            SoundManager.Instance?.Play(SoundManager.SFX.ZoneInteract);
        }

        _consumeCoroutine = null;
    }

    // ── 비주얼 ──────────────────────────────────────────────────────────
    private void SpawnStackVisual()
    {
        if (handcuffVisualPrefab == null || stackRoot == null) return;

        GameObject visual = Instantiate(handcuffVisualPrefab, stackRoot);
        visual.transform.localPosition = stackOffset * _stackVisuals.Count;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = stackScale;
        _stackVisuals.Add(visual);
    }

    private void RemoveStackVisual()
    {
        if (_stackVisuals.Count == 0) return;

        int last = _stackVisuals.Count - 1;
        Destroy(_stackVisuals[last]);
        _stackVisuals.RemoveAt(last);
    }
}