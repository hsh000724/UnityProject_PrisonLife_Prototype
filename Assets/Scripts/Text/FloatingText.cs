using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 월드 스페이스에서 위로 떠오르며 페이드 아웃되는 팝업 텍스트.
/// PlayerMining에서 광석 획득 / MAX 도달 시 생성.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatSpeed = 1.5f;    // 위로 올라가는 속도
    [SerializeField] private float fadeDuration = 1.2f;  // 페이드 아웃 시간
    [SerializeField] private float initialDelay = 0.2f;  // 생성 후 페이드 시작까지 대기

    [Header("References")]
    [SerializeField] private TextMeshPro label;          // World Space TMP (TextMeshPro)

    private Color _originalColor;
    private Transform _cameraTransform;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshPro>();

        _originalColor = label.color;
    }

    private void OnEnable()
    {
        _cameraTransform = Camera.main?.transform;
        label.color = _originalColor;
        StartCoroutine(FloatAndFadeRoutine());
    }

    private void LateUpdate()
    {
        // 항상 카메라를 바라보게 빌보드 처리
        if (_cameraTransform != null)
            transform.rotation = _cameraTransform.rotation;
    }

    /// <summary>텍스트 내용 + 색상 지정 후 재사용.</summary>
    public void SetText(string text, Color color)
    {
        label.text = text;
        _originalColor = color;
        label.color = color;
    }

    private IEnumerator FloatAndFadeRoutine()
    {
        // 초기 대기 (텍스트를 잠깐 고정으로 보여줌)
        yield return new WaitForSeconds(initialDelay);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // 위로 이동
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // 알파 페이드 아웃
            Color c = label.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            label.color = c;

            yield return null;
        }

        // 풀에 반환
        FloatingTextPool.Instance?.Return(this);
    }
}