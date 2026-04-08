using UnityEngine;

/// <summary>
/// FloatingText 전용 오브젝트 풀.
/// 씬에 하나만 존재하는 싱글톤.
/// </summary>
public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }

    [SerializeField] private FloatingText prefab;
    [SerializeField] private int initialSize = 10;

    private ObjectPool<FloatingText> _pool;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _pool = new ObjectPool<FloatingText>(prefab, initialSize, transform);
    }

    /// <summary>월드 포지션에 팝업 텍스트 생성.</summary>
    public void Spawn(string text, Vector3 worldPosition, Color color)
    {
        FloatingText ft = _pool.Get(worldPosition, Quaternion.identity);
        ft.SetText(text, color);
    }

    public void Return(FloatingText ft)
    {
        _pool.Return(ft);
    }
}