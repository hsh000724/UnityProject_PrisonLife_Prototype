using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 해금존 Plane UI 전담.
/// requiredAmount는 UnlockZone.Start()에서 Init()으로 주입받음.
/// </summary>
public class UnlockZoneUI : MonoBehaviour
{
    [Header("Plane")]
    [SerializeField] private MeshRenderer planeMeshRenderer;
    [SerializeField] private string fillPropertyName = "_FillAmount";

    [Header("Canvas UI (World Space)")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image coinIcon;

    [Header("Fill Colors")]
    [SerializeField] private Color colorEmpty = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color colorFilling = new Color(0.2f, 0.7f, 0.3f);
    [SerializeField] private Color colorComplete = new Color(0.9f, 0.8f, 0.1f);

    [Header("Unlock Info")]
    [SerializeField] private string itemName = "Drill";  // 구매 항목명
    [SerializeField] private Sprite itemSprite;          // 항목 아이콘 스프라이트

    // requiredAmount는 UnlockZone에서 주입 — Inspector에서 직접 설정 불필요
    private int _requiredAmount;

    // ── UnlockZone.Start()에서 1회 호출 ─────────────────────────────────
    public void Init(int requiredAmount)
    {
        _requiredAmount = requiredAmount;

        if (labelText != null)
            labelText.text = itemName;

        if (itemIcon != null && itemSprite != null)
            itemIcon.sprite = itemSprite;

        SetFill(0f);
        UpdateCostText(_requiredAmount);
    }

    // ── UnlockZone.PayRoutine()에서 틱마다 호출 ─────────────────────────
    public void UpdateProgress(int paid)
    {
        float ratio = _requiredAmount > 0 ? (float)paid / _requiredAmount : 0f;
        ratio = Mathf.Clamp01(ratio);

        SetFill(ratio);
        UpdateCostText(_requiredAmount - paid);
    }

    // ── 해금 완료 시 호출 ────────────────────────────────────────────────
    public void SetComplete()
    {
        SetFill(1f);
        UpdateCostText(0);

        if (fillImage != null)
            fillImage.color = colorComplete;

        if (planeMeshRenderer != null)
            planeMeshRenderer.material.color = colorComplete;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    /// <summary>
    /// 딜레이 후 숨김 — UnlockZone 대신 zoneUI가 코루틴 실행.
    /// UnlockZone 자신이 꺼져도 정상 동작.
    /// </summary>
    public void HideAfterDelay(float delay)
    {
        StartCoroutine(HideAfterDelayRoutine(delay));
    }

    private IEnumerator HideAfterDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    // ── 내부 처리 ────────────────────────────────────────────────────────
    private void SetFill(float ratio)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = ratio;
            fillImage.color = ratio >= 1f ? colorComplete
                            : ratio > 0f ? colorFilling
                            : colorEmpty;
        }

        if (planeMeshRenderer != null)
        {
            Material mat = planeMeshRenderer.material;

            if (mat.HasProperty(fillPropertyName))
                mat.SetFloat(fillPropertyName, ratio);
            else
                mat.color = Color.Lerp(colorEmpty, colorFilling, ratio);
        }
    }

    private void UpdateCostText(int remaining)
    {
        if (costText != null)
            costText.text = remaining > 0 ? $"{remaining}" : "UNLOCKED";
    }
}