using UnityEngine;

public class DrillZone : UnlockZone
{
    [Header("Camera POI")]
    [SerializeField] private Transform revealCameraPoint;

    // 오브젝트가 SetActive(true) 되는 순간 유니티에서 자동 호출
    private void OnEnable()
    {
        // 활성화된 프레임에 즉시 실행
        TriggerCameraReveal();
    }

    protected override string GetUnlockMessage() => "Drill Unlocked!";

    public void TriggerCameraReveal()
    {
        if (revealCameraPoint == null) return;

        // 카메라 이동 로직 실행
        CameraController.Instance?.MoveToPOIAndReturn(
            revealCameraPoint, onComplete: null);
    }

    protected override void OnUnlocked()
    {
        FindFirstObjectByType<PlayerMining>()?.SetDrillMode(true);
        UnlockManager.Instance?.NotifyDrillUnlocked();
    }
}