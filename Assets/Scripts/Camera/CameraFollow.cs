using UnityEngine;

/// <summary>
/// 플레이어를 따라가는 카메라.
/// 오프셋 기반 추적 + 부드러운 이동(SmoothDamp).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -6f);  // 카메라 위치 오프셋
    [SerializeField] private bool lookAtTarget = true;                     // 항상 플레이어를 바라볼지

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.15f;   // 낮을수록 빠르게 따라옴
    [SerializeField] private float maxSpeed = 30f;       // 추적 최대 속도

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 = 플레이어 위치 + 오프셋
        Vector3 desiredPosition = target.position + offset;

        // SmoothDamp로 부드럽게 이동
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            smoothTime,
            maxSpeed);

        // 플레이어 방향으로 회전
        if (lookAtTarget)
            transform.LookAt(target.position);
    }

    // 씬 시작 시 카메라를 즉시 목표 위치로 스냅 (첫 프레임 튀는 현상 방지)
    private void Start()
    {
        if (target == null) return;
        transform.position = target.position + offset;

        if (lookAtTarget)
            transform.LookAt(target.position);
    }
}