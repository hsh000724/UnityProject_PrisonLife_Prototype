using UnityEngine;
using UnityEngine.UI;

public class MuteButtonUI : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private Image _buttonImage;

    private void Awake()
    {
        _buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateIcon();
    }

    public void OnClickMute()
    {
        // 1. 매니저 상태 변경
        SoundManager.Instance.ToggleMute();

        // 2. 아이콘 변경
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (SoundManager.Instance == null) return;

        _buttonImage.sprite = SoundManager.Instance.IsMuted ? soundOffSprite : soundOnSprite;
    }
}