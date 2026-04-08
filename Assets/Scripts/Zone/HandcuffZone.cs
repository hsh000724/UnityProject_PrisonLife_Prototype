using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 수갑존. 생산된 수갑을 3D로 쌓고 플레이어 진입 시 자동 픽업.
/// </summary>
public class HandcuffZone : MonoBehaviour
{
    [Header("Stack Settings")]
    [SerializeField] private Transform stackRoot;
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.15f, 0f);
    [SerializeField] private int maxStackCount = 20;

    [Header("Pool Settings")]
    [SerializeField] private HandcuffObject handcuffPrefab;
    [SerializeField] private int poolInitialSize = 20;

    [Header("Pickup Settings")]
    [SerializeField] private int pickupAmount = 5;

    private ObjectPool<HandcuffObject> _pool;

    // 현재 존에 쌓인 오브젝트 리스트
    private readonly List<HandcuffObject> _stackedHandcuffs = new List<HandcuffObject>();

    // 시각적 한계 초과분 — 오브젝트 없이 카운트만 관리
    private int _pendingCount = 0;

    public int TotalCount => _stackedHandcuffs.Count + _pendingCount;

    private void Start()
    {
        _pool = new ObjectPool<HandcuffObject>(handcuffPrefab, poolInitialSize, stackRoot);
    }

    /// <summary>HandcuffMachine 생산 완료 시 호출.</summary>
    public void AddHandcuff()
    {
        if (_stackedHandcuffs.Count < maxStackCount)
        {
            // 풀에서 꺼내 스택 위치에 배치
            Vector3 worldPos = stackRoot.position + stackOffset * _stackedHandcuffs.Count;
            HandcuffObject obj = _pool.Get(worldPos, Quaternion.identity);
            obj.transform.SetParent(stackRoot);
            obj.transform.localPosition = stackOffset * _stackedHandcuffs.Count;
            _stackedHandcuffs.Add(obj);
        }
        else
        {
            _pendingCount++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SoundManager.Instance?.Play(SoundManager.SFX.ZoneInteract);

        PlayerCarry carry = other.GetComponent<PlayerCarry>();
        if (carry == null || carry.IsCarrying) return;
        if (TotalCount <= 0) return;

        int amount = Mathf.Min(pickupAmount, TotalCount);
        RemoveHandcuffs(amount);
        carry.PickupHandcuffs(amount);
        TutorialManager.Instance?.NotifyHandcuffPickedUp();

        FloatingTextPool.Instance?.Spawn(
            $"+{amount} Handcuff",
            other.transform.position + Vector3.up * 2f,
            Color.cyan);
    }

    private void RemoveHandcuffs(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (_stackedHandcuffs.Count > 0)
            {
                int lastIndex = _stackedHandcuffs.Count - 1;
                HandcuffObject obj = _stackedHandcuffs[lastIndex];

                // 리스트에서 먼저 제거 후 풀 반환 — 참조 역전 방지
                _stackedHandcuffs.RemoveAt(lastIndex);
                obj.transform.SetParent(stackRoot); // 부모 유지한 채 비활성화
                _pool.Return(obj);                  // SetActive(false) 처리
            }
            else if (_pendingCount > 0)
            {
                _pendingCount--;
            }
        }
    }
    /// <summary>캐셔가 픽업할 때 호출. 플레이어 픽업과 분리.</summary>
    public void CashierPickup(int amount)
    {
        RemoveHandcuffs(amount);
    }
}