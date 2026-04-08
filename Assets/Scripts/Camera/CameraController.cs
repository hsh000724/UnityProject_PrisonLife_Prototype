using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 카메라 이동 연출 전담.
/// PrisonCameraPoint의 position + rotation을 그대로 사용.
/// 별도 위치/각도 조정 불필요.
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float stayDuration = 2.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// target Transform의 position + rotation으로 부드럽게 이동.
    /// 대기 후 원래 위치/회전으로 복귀.
    /// </summary>
    public void MoveToPOIAndReturn(Transform target, Action onComplete = null)
    {
        StartCoroutine(POIRoutine(target, onComplete));
    }

    private IEnumerator POIRoutine(Transform target, Action onComplete)
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // 출발 시점의 position + rotation 저장
        Vector3 originPosition = transform.position;
        Quaternion originRotation = transform.rotation;

        // target의 position + rotation으로 이동
        yield return StartCoroutine(MoveRoutine(
            originPosition, originRotation,
            target.position, target.rotation,
            moveDuration));

        // 대기
        yield return new WaitForSecondsRealtime(stayDuration);

        // 원래 위치/회전으로 복귀
        yield return StartCoroutine(MoveRoutine(
            transform.position, transform.rotation,
            originPosition, originRotation,
            moveDuration));

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        onComplete?.Invoke();
    }

    /// <summary>
    /// position + rotation 동시에 보간.
    /// </summary>
    private IEnumerator MoveRoutine(
        Vector3 fromPos, Quaternion fromRot,
        Vector3 toPos, Quaternion toRot,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // 퍼즈 중에도 동작
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            transform.position = Vector3.Lerp(fromPos, toPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, t);

            yield return null;
        }

        transform.position = toPos;
        transform.rotation = toRot;
    }
}