using UnityEngine;

/// <summary>
/// 수갑 오브젝트. ObjectPool 로 관리.
/// </summary>
public class HandcuffObject : MonoBehaviour
{
    public bool IsCarried { get; private set; }

    public void SetCarried(bool carried)
    {
        IsCarried = carried;
    }
}