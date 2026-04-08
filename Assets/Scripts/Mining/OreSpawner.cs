using System.Collections;
using UnityEngine;

/// <summary>
/// 씬에 미리 배치된 광물을 등록하고
/// 채광된 광물의 비활성화 + 일정 시간 후 재활성화를 전담.
/// </summary>
public class OreSpawner : MonoBehaviour
{
    public static OreSpawner Instance { get; private set; }

    [Header("씬에 미리 배치된 광물 등록")]
    [SerializeField] private OreObject[] ores;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 5f;   // 비활성화 후 재활성화까지 대기 시간

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        foreach (OreObject ore in ores)
            ore.OnMined += HandleOreMined;
    }

    private void OnDestroy()
    {
        foreach (OreObject ore in ores)
            if (ore != null) ore.OnMined -= HandleOreMined;
    }

    private void HandleOreMined(OreObject ore)
    {
        ore.gameObject.SetActive(false);
        StartCoroutine(RespawnRoutine(ore));
    }

    private IEnumerator RespawnRoutine(OreObject ore)
    {
        yield return new WaitForSeconds(respawnDelay);

        // 오브젝트가 씬에서 삭제되지 않았을 경우에만 재활성화
        if (ore != null)
            ore.gameObject.SetActive(true);
    }
}