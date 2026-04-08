using UnityEngine;

/// <summary>
/// 스폰 포인트에서 죄수를 생성하고 풀로 관리.
/// </summary>
public class PrisonerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Prisoner prisonerPrefab;
    [SerializeField] private Transform spawnPoint;       // 스폰 위치
    [SerializeField] private int poolSize = 10;

    private ObjectPool<Prisoner> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Prisoner>(prisonerPrefab, poolSize, transform);
    }

    public Prisoner Spawn()
    {
        Prisoner p = _pool.Get(spawnPoint.position, spawnPoint.rotation);
        return p;
    }

    public void Return(Prisoner prisoner)
    {
        _pool.Return(prisoner);
    }
}