using UnityEngine;

/// <summary>
/// 드릴 불도저존. 해금 시 플레이어를 불도저 모드로 전환.
/// </summary>
public class DrillBulldozerZone : UnlockZone
{
    protected override string GetUnlockMessage() => "Drill Bulldozer Unlocked!";

    protected override void OnUnlocked()
    {
        FindFirstObjectByType<PlayerMining>()?.SetBulldozerMode(true);
    }
}