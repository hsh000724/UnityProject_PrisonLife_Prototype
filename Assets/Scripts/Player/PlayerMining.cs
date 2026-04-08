using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어 채광 처리.
/// 도구 비주얼(PlayerToolVisual)과 사운드(SoundManager)를 포함한 통합 버전.
/// </summary>
public class PlayerMining : MonoBehaviour
{
    [Header("Mining Settings")]
    [SerializeField] private float miningDelay = 1.5f;
    [SerializeField] public int maxOreCount = 10;

    [Header("Drill Settings")]
    [SerializeField] private float drillCooldown = 0.2f;   // 드릴 채광 쿨다운
    [SerializeField] private DrillMining drillMining;      // 불도저 범위 채광 컴포넌트

    [Header("Floating Text")]
    [SerializeField] private Vector3 floatingTextOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Color oreCountColor = Color.white;
    [SerializeField] private Color maxColor = Color.red;

    [Header("Ore Stack Visual")]
    [SerializeField] private Transform oreStackRoot;
    [SerializeField] private GameObject oreStackPrefab;
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.3f, 0f);

    public int CurrentOreCount { get; private set; }
    public bool IsMining { get; private set; }

    // 채광 모드
    public enum MiningMode { Pickaxe, Drill, Bulldozer }
    public MiningMode CurrentMode { get; private set; } = MiningMode.Pickaxe;

    private Animator _animator;
    private bool _hasAnimator;
    private OreObject _targetOre;
    private float _drillCooldownTimer;
    private PlayerToolVisual _toolVisual; // 도구 연출 컴포넌트

    private readonly List<GameObject> _oreVisuals = new List<GameObject>();
    private static readonly int MiningHash = Animator.StringToHash("IsMining");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null && _animator.runtimeAnimatorController != null;

        // 도구 비주얼 연동 (동일 오브젝트 혹은 자식에 부착된 경우)
        _toolVisual = GetComponent<PlayerToolVisual>();
    }

    private void Update()
    {
        if (_drillCooldownTimer > 0f)
            _drillCooldownTimer -= Time.deltaTime;
    }

    // ── 모드 전환 (UnlockZone 등에서 호출) ───────────────────────────────────
    public void SetDrillMode(bool active)
    {
        CurrentMode = active ? MiningMode.Drill : MiningMode.Pickaxe;
        _toolVisual?.SetMode(CurrentMode); // 비주얼 모델 변경
    }

    public void SetBulldozerMode(bool active)
    {
        CurrentMode = active ? MiningMode.Bulldozer : MiningMode.Drill;

        if (drillMining != null)
            drillMining.enabled = active;

        _toolVisual?.SetMode(CurrentMode); // 비주얼 모델 변경
    }

    // ── 채광 시도 (OreObject.OnTriggerEnter 등에서 호출) ──────────────────────
    public void TryStartMining(OreObject ore)
    {
        if (CurrentOreCount >= maxOreCount)
        {
            SpawnFloatingText("MAX", maxColor);
            return;
        }

        switch (CurrentMode)
        {
            case MiningMode.Pickaxe:
                if (!IsMining)
                {
                    _targetOre = ore;
                    StartCoroutine(PickaxeRoutine());
                }
                break;

            case MiningMode.Drill:
            case MiningMode.Bulldozer:
                if (_drillCooldownTimer <= 0f)
                    DrillMine(ore);
                break;
        }
    }

    // ── 곡괭이 채광 (딜레이 동안 사운드 반복 + 스윙 연출) ───────────────────────
    private IEnumerator PickaxeRoutine()
    {
        IsMining = true;

        if (_hasAnimator)
            _animator.SetBool(MiningHash, true);

        // 도구 스윙 연출 시작
        _toolVisual?.PlayMiningAction();

        float elapsed = 0f;
        float sfxInterval = 0.5f; // 애니메이션/스윙 리듬에 맞춘 소리 간격
        float lastSfxTime = -sfxInterval;

        while (elapsed < miningDelay)
        {
            // 딜레이 도중 일정 간격으로 곡괭이질 소리 재생
            if (elapsed >= lastSfxTime + sfxInterval)
            {
                SoundManager.Instance?.PlayWithRandomPitch(SoundManager.SFX.Pickaxe);
                lastSfxTime = elapsed;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_hasAnimator)
            _animator.SetBool(MiningHash, false);

        // 채광 완료 처리
        if (_targetOre != null && _targetOre.gameObject.activeSelf)
        {
            _targetOre.Mine();
            AddOre();

            // 타격감 피드백
            SoundManager.Instance?.Play(SoundManager.SFX.OreBreak);
        }

        IsMining = false;
        _targetOre = null;
    }

    // ── 드릴 즉시 채광 (진동 연출 + 드릴 사운드) ────────────────────────────────
    private void DrillMine(OreObject ore)
    {
        if (ore == null || !ore.gameObject.activeSelf) return;

        // 드릴 작동 연출 (진동/회전 등)
        _toolVisual?.PlayMiningAction();

        // 드릴 효과음
        SoundManager.Instance?.Play(SoundManager.SFX.Drill);

        ore.Mine();
        AddOre();
        _drillCooldownTimer = drillCooldown;
    }

    // ── 광석 추가 및 비주얼 생성 ───────────────────────────────────────────
    private void AddOre()
    {
        CurrentOreCount++;
        SpawnOreVisual();
        TutorialManager.Instance?.NotifyMined(); // ← 추가

        if (CurrentOreCount >= maxOreCount)
            SpawnFloatingText("MAX", maxColor);
        else
            SpawnFloatingText($"{CurrentOreCount} / {maxOreCount}", oreCountColor);
    }

    private void SpawnOreVisual()
    {
        if (oreStackPrefab == null || oreStackRoot == null) return;

        GameObject visual = Instantiate(oreStackPrefab, oreStackRoot);
        // 리스트 개수를 인덱스로 활용하여 높이 조절
        visual.transform.localPosition = stackOffset * _oreVisuals.Count;
        visual.transform.localRotation = Quaternion.identity;
        _oreVisuals.Add(visual);
    }

    public void ClearOreStack(int amount)
    {
        int removeCount = Mathf.Min(amount, CurrentOreCount);
        CurrentOreCount -= removeCount;

        for (int i = 0; i < removeCount; i++)
        {
            if (_oreVisuals.Count == 0) break;

            int last = _oreVisuals.Count - 1;
            if (_oreVisuals[last] != null)
                Destroy(_oreVisuals[last]);
            _oreVisuals.RemoveAt(last);
        }
    }

    private void SpawnFloatingText(string text, Color color)
    {
        FloatingTextPool.Instance?.Spawn(
            text,
            transform.position + floatingTextOffset,
            color);
    }
}