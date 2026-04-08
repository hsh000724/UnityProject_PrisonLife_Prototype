using UnityEngine;

/// <summary>
/// 감옥존. 해금 시 PrisonManager에 통보.
/// </summary>
public class PrisonZone : UnlockZone
{
    protected override string GetUnlockMessage() => "Prison Unlocked!";

    protected override void OnUnlocked()
    {
        PrisonManager.Instance?.OnPrisonZoneUnlocked();
    }
}