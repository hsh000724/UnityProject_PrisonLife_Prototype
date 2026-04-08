using UnityEngine;

/// <summary>
/// 카운터존 트리거.
/// 플레이어가 수갑을 들고 진입하면 Counter에 적재 요청.
/// </summary>
public class CounterZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerCarry carry = other.GetComponent<PlayerCarry>();
        if (carry == null || !carry.IsCarrying) return;

        int dropped = carry.DropHandcuffs();
        Counter.Instance?.AddHandcuffs(dropped);
        TutorialManager.Instance?.NotifyHandcuffDelivered();

        FloatingTextPool.Instance?.Spawn(
            $"+{dropped} Handcuffs on Counter",
            other.transform.position + Vector3.up * 2f,
            Color.green);
    }
}