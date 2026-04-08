using System.Collections;
using UnityEngine;

/// <summary>
/// 광물을 소비해 수갑을 생산하는 기계.
/// 광물 1개당 수갑 1개 생산. 생산 완료 시 HandcuffZone에 전달.
/// </summary>
public class HandcuffMachine : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] private float productionTimePerOre = 2f;  // 광물 1개당 생산 시간

    [Header("References")]
    [SerializeField] private HandcuffZone handcuffZone;        // 생산된 수갑을 쌓을 존
    [SerializeField] private Animator machineAnimator;         // 기계 애니메이터 (없어도 동작)

    private bool _hasAnimator;
    private static readonly int WorkingHash = Animator.StringToHash("IsWorking");

    private void Awake()
    {
        _hasAnimator = machineAnimator != null
            && machineAnimator.runtimeAnimatorController != null;
    }

    /// <summary>OreZone에서 광물 소모 후 호출.</summary>
    public void StartProduction(int oreCount)
    {
        if (oreCount <= 0) return;
        StartCoroutine(ProductionRoutine(oreCount));
    }

    private IEnumerator ProductionRoutine(int oreCount)
    {
        if (_hasAnimator)
            machineAnimator.SetBool(WorkingHash, true);

        for (int i = 0; i < oreCount; i++)
        {
            yield return new WaitForSeconds(productionTimePerOre);
            handcuffZone.AddHandcuff();
        }

        if (_hasAnimator)
            machineAnimator.SetBool(WorkingHash, false);

        SoundManager.Instance?.Play(SoundManager.SFX.HandcuffProduce);
    }
}