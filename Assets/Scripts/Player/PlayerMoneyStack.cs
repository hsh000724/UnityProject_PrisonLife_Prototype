using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 등 뒤 Money 오브젝트 스택 비주얼.
/// MoneyZone 픽업 시 오브젝트를 등 뒤로 이동시켜 쌓음.
/// </summary>
public class PlayerMoneyStack : MonoBehaviour
{
    [Header("Stack Settings")]
    [SerializeField] private Transform stackRoot;                          // 등 뒤 기준 위치
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private Vector3 moneyScale = new Vector3(0.3f, 0.05f, 0.3f);

    public int StackCount => _moneyVisuals.Count;

    private readonly List<GameObject> _moneyVisuals = new List<GameObject>();

    /// <summary>
    /// MoneyZone에서 픽업 시 호출.
    /// 존의 비주얼 오브젝트 수만큼 등 뒤에 새로 생성.
    /// </summary>
    public void AddMoneyVisuals(int visualCount, GameObject prefab)
    {
        for (int i = 0; i < visualCount; i++)
        {
            if (prefab == null || stackRoot == null) break;

            GameObject visual = Instantiate(prefab, stackRoot);
            visual.transform.localPosition = stackOffset * _moneyVisuals.Count;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = moneyScale;
            _moneyVisuals.Add(visual);
        }
    }

    /// <summary>
    /// 돈 사용 시 (해금존 지불 등) 비주얼 제거.
    /// </summary>
    public void RemoveMoneyVisuals(int amount)
    {
        int removeCount = Mathf.Min(amount, _moneyVisuals.Count);

        for (int i = 0; i < removeCount; i++)
        {
            int last = _moneyVisuals.Count - 1;
            if (_moneyVisuals[last] != null)
                Destroy(_moneyVisuals[last]);
            _moneyVisuals.RemoveAt(last);
        }
    }

    /// <summary>전체 비주얼 제거.</summary>
    public void ClearVisuals()
    {
        foreach (GameObject v in _moneyVisuals)
            if (v != null) Destroy(v);
        _moneyVisuals.Clear();
    }
}