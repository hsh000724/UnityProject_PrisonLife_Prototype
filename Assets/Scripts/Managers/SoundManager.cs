using UnityEngine;

/// <summary>
/// 게임 전체 효과음 관리.
/// AudioSource 풀 기반으로 동시 재생 지원.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    // ── 효과음 열거형 ────────────────────────────────────────────────────
    public enum SFX
    {
        Pickaxe,        // 1. 곡괭이 채광 (플레이어 + 광부 공용)
        ZoneInteract,   // 2. Zone 아이템 투입/수령 (Money 제외)
        HandcuffProduce,// 3. 수갑 기계 생산
        MoneyPickup,    // 4. 돈 들어올리기
        ZoneUnlock,     // 5. 새 Zone 해금
        Drill,          // 6. 드릴 채광
        OreBreak,       // 7. 광물 채광 완료 (광물 부서지는 소리)
        GameClear       // 8. 게임 클리어
    }

    [Header("AudioSource Pool")]
    [SerializeField] private int poolSize = 8;           // 동시 재생 최대 수

    [Header("SFX Clips")]
    [SerializeField] private AudioClip pickaxeClip;
    [SerializeField] private AudioClip zoneInteractClip;
    [SerializeField] private AudioClip handcuffProduceClip;
    [SerializeField] private AudioClip moneyPickupClip;
    [SerializeField] private AudioClip zoneUnlockClip;
    [SerializeField] private AudioClip drillClip;
    [SerializeField] private AudioClip oreBreakClip;
    [SerializeField] private AudioClip gameClearClip;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float pickaxeVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float zoneInteractVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] private float handcuffVolume = 0.9f;
    [SerializeField][Range(0f, 1f)] private float moneyVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float unlockVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float drillVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float oreBreakVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] private float gameClearVolume = 1f;


    // AudioSource 풀
    private AudioSource[] _pool;
    private int _poolIndex = 0;

    // 음소거
    private bool _isMuted = false;
    public bool IsMuted => _isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildPool();
    }

    // ── 풀 생성 ──────────────────────────────────────────────────────────
    private void BuildPool()
    {
        _pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"SFX_Source_{i}");
            go.transform.SetParent(transform);
            _pool[i] = go.AddComponent<AudioSource>();
            _pool[i].playOnAwake = false;
        }
    }

    // ── 외부 호출 — 재생 ─────────────────────────────────────────────────
    public void Play(SFX sfx)
    {
        if (_isMuted) return;

        AudioClip clip = GetClip(sfx);
        float volume = GetVolume(sfx) * masterVolume;

        if (clip == null) return;

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = volume;
        source.pitch = GetPitch(sfx);
        source.Play();
    }

    /// <summary>피치 랜덤 변형이 필요한 경우 오버로드로 호출.</summary>
    public void PlayWithRandomPitch(SFX sfx, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (_isMuted) return;

        AudioClip clip = GetClip(sfx);
        float volume = GetVolume(sfx) * masterVolume;

        if (clip == null) return;

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(minPitch, maxPitch);
        source.Play();
    }

    // ── 마스터 볼륨 제어 ─────────────────────────────────────────────────
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    // ── 재생 가능한 AudioSource 반환 ────────────────────────────────────
    private AudioSource GetAvailableSource()
    {
        // 현재 재생 중이지 않은 소스 우선 탐색
        for (int i = 0; i < _pool.Length; i++)
        {
            if (!_pool[i].isPlaying)
                return _pool[i];
        }

        // 모두 재생 중이면 라운드 로빈으로 덮어씀
        AudioSource source = _pool[_poolIndex];
        _poolIndex = (_poolIndex + 1) % _pool.Length;
        return source;
    }

    // ── 클립 매핑 ────────────────────────────────────────────────────────
    private AudioClip GetClip(SFX sfx)
    {
        return sfx switch
        {
            SFX.Pickaxe => pickaxeClip,
            SFX.ZoneInteract => zoneInteractClip,
            SFX.HandcuffProduce => handcuffProduceClip,
            SFX.MoneyPickup => moneyPickupClip,
            SFX.ZoneUnlock => zoneUnlockClip,
            SFX.Drill => drillClip,
            SFX.OreBreak => oreBreakClip,
            SFX.GameClear => gameClearClip,
            _ => null
        };
    }

    // ── 볼륨 매핑 ────────────────────────────────────────────────────────
    private float GetVolume(SFX sfx)
    {
        return sfx switch
        {
            SFX.Pickaxe => pickaxeVolume,
            SFX.ZoneInteract => zoneInteractVolume,
            SFX.HandcuffProduce => handcuffVolume,
            SFX.MoneyPickup => moneyVolume,
            SFX.ZoneUnlock => unlockVolume,
            SFX.Drill => drillVolume,
            SFX.OreBreak => oreBreakVolume,
            SFX.GameClear => gameClearVolume,
            _ => 1f
        };
    }

    // ── 피치 매핑 ────────────────────────────────────────────────────────
    private float GetPitch(SFX sfx)
    {
        return sfx switch
        {
            // 채광 계열은 피치 고정 (PlayWithRandomPitch 사용 시 덮어씀)
            SFX.Pickaxe => 1f,
            SFX.Drill => 1f,
            SFX.OreBreak => 1f,
            _ => 1f
        };
    }
    public void ToggleMute()
    {
        _isMuted = !_isMuted;

        // 현재 풀(Pool)에 있는 모든 오디오 소스의 볼륨을 0 또는 원래 볼륨으로 조절
        foreach (var source in _pool)
        {
            if (_isMuted)
            {
                source.mute = true; // 소스 자체를 뮤트
            }
            else
            {
                source.mute = false;
            }
        }
    }
}