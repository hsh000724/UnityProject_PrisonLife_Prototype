using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DemandBubbleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI demandText;
    [SerializeField] private GameObject bubbleRoot;

    [Header("Position")]
    [SerializeField] private Vector3 headOffset = new Vector3(0f, 2.2f, 0f); // 머리 위 오프셋

    [Header("Fill Colors")]
    [SerializeField] private Color colorLow = new Color(0.91f, 0.30f, 0.24f);
    [SerializeField] private Color colorMid = new Color(0.95f, 0.61f, 0.07f);
    [SerializeField] private Color colorHigh = new Color(0.18f, 0.80f, 0.44f);
    [SerializeField] private Color colorFull = new Color(0.94f, 0.75f, 0.25f);

    private Transform _ownerTransform;  // 죄수 루트 Transform
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;

        // 부모가 죄수 오브젝트라면 바로 사용
        // 아니라면 외부에서 SetOwner()로 주입
        _ownerTransform = transform.parent;
    }

    /// <summary>
    /// 죄수 루트 Transform을 외부에서 주입.
    /// Prefab 구조상 부모가 죄수가 아닐 경우 사용.
    /// </summary>
    public void SetOwner(Transform owner)
    {
        _ownerTransform = owner;
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        // 매 프레임 죄수 머리 위 월드 포지션으로 이동
        if (_ownerTransform != null)
            transform.position = _ownerTransform.position + headOffset;

        // 카메라를 향해 빌보드 회전
        transform.rotation = _mainCamera.transform.rotation;
    }

    public void SetActive(bool active)
    {
        bubbleRoot.SetActive(active);
    }

    public void UpdateGauge(int current, int demand)
    {
        float ratio = demand > 0 ? (float)current / demand : 0f;
        ratio = Mathf.Clamp01(ratio);

        fillImage.fillAmount = ratio;
        fillImage.color = GetFillColor(ratio);
        demandText.text = $"{current} / {demand}";
    }

    private Color GetFillColor(float ratio)
    {
        if (ratio >= 1.0f) return colorFull;
        if (ratio >= 0.67f) return colorHigh;
        if (ratio >= 0.34f) return colorMid;
        return colorLow;
    }
}