using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 해금 알림 팝업 UI.
/// 해금 시 메시지를 표시하고 일정 시간 후 자동으로 사라짐.
/// </summary>
public class UnlockNoticeUI : MonoBehaviour
{
    public static UnlockNoticeUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine _showCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string message)
    {
        if (_showCoroutine != null)
            StopCoroutine(_showCoroutine);

        _showCoroutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        messageText.text = message;
        canvasGroup.alpha = 0f;
        panel.SetActive(true);

        // 페이드 인
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(displayDuration);

        // 페이드 아웃
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        panel.SetActive(false);
        _showCoroutine = null;
    }
}