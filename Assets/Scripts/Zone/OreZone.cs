using System.Collections;
using UnityEngine;

/// <summary>
/// 광물존.
/// 플레이어 진입 시 광물 소모 + 광부 채광 즉시 수신.
/// </summary>
public class OreZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandcuffMachine machine;

    [Header("Player Consume Settings")]
    [SerializeField] private float consumeInterval = 0.3f;

    // 광물존 내부 재고 (광부가 채광한 광물 누적)
    private int _oreStock = 0;
    private Coroutine _consumeCoroutine;

    // ── 플레이어 진입 — 보유 광물 소모 ──────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMining mining = other.GetComponent<PlayerMining>();
        if (mining == null || mining.CurrentOreCount <= 0) return;

        _consumeCoroutine = StartCoroutine(PlayerConsumeRoutine(mining));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (_consumeCoroutine != null)
        {
            StopCoroutine(_consumeCoroutine);
            _consumeCoroutine = null;
        }
    }

    private IEnumerator PlayerConsumeRoutine(PlayerMining mining)
    {
        while (mining.CurrentOreCount > 0)
        {
            SoundManager.Instance?.Play(SoundManager.SFX.ZoneInteract);
            yield return new WaitForSeconds(consumeInterval);

            mining.ClearOreStack(1);
            machine.StartProduction(1);
            TutorialManager.Instance?.NotifyOreDelivered();

            FloatingTextPool.Instance?.Spawn(
                "-1 Ore",
                mining.transform.position + Vector3.up * 2f,
                Color.yellow);
        }

        _consumeCoroutine = null;
    }

    // ── 광부 채광 즉시 수신 ──────────────────────────────────────────────
    /// <summary>Miner.MineRoutine()에서 채광 성공 시 호출.</summary>
    public void ReceiveOreFromMiner(int amount)
    {
        SoundManager.Instance?.Play(SoundManager.SFX.ZoneInteract);
        _oreStock += amount;
        machine.StartProduction(amount);
    }
}