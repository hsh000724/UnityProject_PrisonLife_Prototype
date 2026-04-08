using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 임시 UIManager - 조작 가이드 UI 표시/숨김 관리
/// 요구사항 1번(퍼즈 상태 가이드), 16번(유휴 감지 가이드) 대응
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Guide UI")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private CanvasGroup guideCanvasGroup;
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Guide Text")]
    [SerializeField]
    [TextArea]
    private string guideMessage = "Touch the screen to move";

    private Coroutine _fadeCoroutine;
    private bool _isGuideVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // 시작 시 가이드 즉시 표시 (퍼즈 상태)
        if (guideText != null)
            guideText.text = guideMessage;

        ShowGuideImmediate();
    }

    // ── 외부 호출용 ──────────────────────────────────────────────────────

    /// <summary>페이드 인으로 가이드 표시</summary>
    public void ShowGuide()
    {
        if (_isGuideVisible) return;
        StartFade(true);
    }

    /// <summary>페이드 아웃으로 가이드 숨김</summary>
    public void HideGuide()
    {
        if (!_isGuideVisible) return;
        StartFade(false);
    }

    /// <summary>즉시 표시 (게임 시작 퍼즈 상태용)</summary>
    public void ShowGuideImmediate()
    {
        StopCurrentFade();
        guidePanel.SetActive(true);
        guideCanvasGroup.alpha = 1f;
        _isGuideVisible = true;
    }

    /// <summary>즉시 숨김</summary>
    public void HideGuideImmediate()
    {
        StopCurrentFade();
        guideCanvasGroup.alpha = 0f;
        guidePanel.SetActive(false);
        _isGuideVisible = false;
    }

    // ── 내부 페이드 처리 ─────────────────────────────────────────────────

    private void StartFade(bool fadeIn)
    {
        StopCurrentFade();
        _fadeCoroutine = StartCoroutine(FadeRoutine(fadeIn));
    }

    private void StopCurrentFade()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
    }

    private IEnumerator FadeRoutine(bool fadeIn)
    {
        if (fadeIn) guidePanel.SetActive(true);

        float startAlpha = guideCanvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        _isGuideVisible = fadeIn;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 퍼즈 중에도 동작
            guideCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        guideCanvasGroup.alpha = targetAlpha;

        if (!fadeIn) guidePanel.SetActive(false);

        _fadeCoroutine = null;
    }
}