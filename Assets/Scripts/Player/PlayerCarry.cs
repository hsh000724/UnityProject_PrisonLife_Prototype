using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 수갑 운반 처리.
/// 픽업 시 carryRoot 위치에서 위로 스케일 축소된 수갑을 쌓음.
/// </summary>
public class PlayerCarry : MonoBehaviour
{
    [Header("Carry Visual")]
    [SerializeField] private Transform carryRoot;               // 플레이어 손/등 위치
    [SerializeField] private GameObject handcuffVisualPrefab;   // 들고 있을 때 표시할 수갑 프리팹
    [SerializeField] private Vector3 carriedScale = new Vector3(0.03f, 0.25f, 0.03f);
    [SerializeField] private float stackSpacing = 0.27f;        // 수갑 간 y 간격 (스케일 y + 여유)

    public bool IsCarrying => _carryCount > 0;
    public int CarryCount => _carryCount;

    private int _carryCount;
    private readonly List<GameObject> _carryVisuals = new List<GameObject>();

    public void PickupHandcuffs(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnCarryVisual();
            _carryCount++;
        }
    }

    /// <summary>CounterZone에서 호출. 운반 중인 수갑 전량 반환.</summary>
    public int DropHandcuffs()
    {
        int dropped = _carryCount;
        _carryCount = 0;
        ClearCarryVisuals();
        return dropped;
    }

    // ── 비주얼 생성 ──────────────────────────────────────────────────────
    private void SpawnCarryVisual()
    {
        if (handcuffVisualPrefab == null || carryRoot == null) return;

        // 현재 스택 개수 기준으로 localPosition y 계산
        float yOffset = _carryVisuals.Count * stackSpacing;
        Vector3 localPos = new Vector3(0f, yOffset, 0f);

        GameObject visual = Instantiate(handcuffVisualPrefab, carryRoot);
        visual.transform.localPosition = localPos;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = carriedScale;

        _carryVisuals.Add(visual);
    }

    private void ClearCarryVisuals()
    {
        foreach (GameObject visual in _carryVisuals)
        {
            if (visual != null)
                Destroy(visual);
        }
        _carryVisuals.Clear();
    }
}