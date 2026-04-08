using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 클리어 UI.
/// TitleImage, GameIconImage, ContinueBtn 3개 구성요소.
/// 페이드 인 후 ContinueBtn 활성화.
/// </summary>
public class GameClearUI : MonoBehaviour
{
    public static GameClearUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI Components")]
    [SerializeField] private Image titleImage;       // CLEAR 타이틀 이미지
    [SerializeField] private Image gameIconImage;    // 게임 아이콘 이미지
    [SerializeField] private Button continueBtn;     // 계속하기 버튼

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float btnAppearDelay = 0.8f;  // 버튼 등장 딜레이

    [Header("Animation Settings")]
    [SerializeField] private float iconBounceScale = 1.2f;  // 아이콘 등장 바운스 크기
    [SerializeField] private float bounceSpeed = 5f;

    [SerializeField]
    private GameObject joystickUI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        panel.SetActive(false);

        // ContinueBtn 클릭 이벤트 등록
        continueBtn?.onClick.AddListener(OnContinueClicked);
    }

    public void Show()
    {
        if (joystickUI != null) joystickUI.SetActive(false);

        SoundManager.Instance?.Play(SoundManager.SFX.GameClear);
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        canvasGroup.alpha = 0f;
        continueBtn?.gameObject.SetActive(false);

        if (gameIconImage != null)
            gameIconImage.transform.localScale = Vector3.zero;

        panel.SetActive(true);

        // 페이드 인 — unscaledDeltaTime 사용
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        yield return StartCoroutine(IconBounceRoutine());

        // 버튼 등장 딜레이 — unscaledDeltaTime 기반
        elapsed = 0f;
        while (elapsed < btnAppearDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        continueBtn?.gameObject.SetActive(true);
    }

    private IEnumerator IconBounceRoutine()
    {
        if (gameIconImage == null) yield break;

        Transform iconTr = gameIconImage.transform;
        float half = 0.3f;

        // 0 → bounceScale — unscaledDeltaTime 사용
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime * bounceSpeed;
            iconTr.localScale = Vector3.one * Mathf.Lerp(0f, iconBounceScale, elapsed / half);
            yield return null;
        }

        // bounceScale → 1
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime * bounceSpeed;
            iconTr.localScale = Vector3.one * Mathf.Lerp(iconBounceScale, 1f, elapsed / half);
            yield return null;
        }

        iconTr.localScale = Vector3.one;
    }

    // ── ContinueBtn 클릭 ─────────────────────────────────────────────────
    private void OnContinueClicked()
    {
        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        if (joystickUI != null) joystickUI.SetActive(true);

        float elapsed = 0f;
        float fadeDuration = 0.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        panel.SetActive(false);
        Time.timeScale = 1.0f;
        FindFirstObjectByType<PlayerMovement>()?.SetPause(false);
    }
}