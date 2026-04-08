using UnityEngine;

public class MinerZone : UnlockZone
{
    protected override string GetUnlockMessage() => "Miners Unlocked!";

    protected override void OnUnlocked()
    {
        // 광부 활성화
        MinerManager.Instance?.ActivateMiners();

        // 캐셔존 해금 통보
        UnlockManager.Instance?.NotifyMinerUnlocked();
    }
}