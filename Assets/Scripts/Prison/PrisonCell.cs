using UnityEngine;

/// <summary>
/// 감옥 셀.
/// PrisonZone 해금 시 SetActive(true)로 등장.
/// 죄수 배정 시 침대 위치 + 회전 제공.
/// </summary>
public class PrisonCell : MonoBehaviour
{
    [Header("Cell Objects")]
    [SerializeField] private GameObject cellSpace;      // 셀 공간 오브젝트 (벽/바닥 등)
    [SerializeField] private GameObject bedObject;      // 침대 오브젝트

    [Header("Sleep Transform")]
    [SerializeField] private Transform sleepPoint;      // 죄수가 누울 위치
    // sleepPoint의 Rotation이 침대 방향을 결정

    public bool IsOccupied { get; private set; }
    public bool IsUnlocked { get; private set; }

    private void Awake()
    {
        // 해금 전 전체 비활성
        if (cellSpace != null) cellSpace.SetActive(false);
        if (bedObject != null) bedObject.SetActive(false);
    }

    /// <summary>PrisonZone 해금 시 PrisonManager에서 호출.</summary>
    public void UnlockCell()
    {
        IsUnlocked = true;

        if (cellSpace != null) cellSpace.SetActive(true);
        if (bedObject != null) bedObject.SetActive(true);
    }

    /// <summary>죄수 배정 시 호출. 위치 + 회전 반환.</summary>
    public void Assign(out Vector3 position, out Quaternion rotation)
    {
        IsOccupied = true;
        position = sleepPoint != null ? sleepPoint.position : transform.position;
        rotation = sleepPoint != null ? sleepPoint.rotation : Quaternion.identity;
    }

    public Vector3 GetSleepPosition() =>
        sleepPoint != null ? sleepPoint.position : transform.position;

    public Quaternion GetSleepRotation() =>
        sleepPoint != null ? sleepPoint.rotation : Quaternion.identity;
}