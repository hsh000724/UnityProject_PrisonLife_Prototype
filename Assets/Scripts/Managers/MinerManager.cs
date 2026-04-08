using UnityEngine;

/// <summary>
/// 광부 3명 관리.
/// MinerZone 해금 시 활성화.
/// </summary>
public class MinerManager : MonoBehaviour
{
    public static MinerManager Instance { get; private set; }

    [SerializeField] private Miner[] miners;  // 씬에 배치된 광부 3명

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 시작 시 전부 비활성
        foreach (Miner miner in miners)
            if (miner != null) miner.gameObject.SetActive(false);
    }

    /// <summary>MinerZone.OnUnlocked()에서 호출.</summary>
    public void ActivateMiners()
    {
        foreach (Miner miner in miners)
            if (miner != null) miner.gameObject.SetActive(true);
    }
}