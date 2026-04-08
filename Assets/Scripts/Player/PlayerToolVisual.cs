using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 도구 비주얼 관리.
/// Hand 본에 부착. 곡괭이 스윙 / 드릴·불도저 진동 연출.
/// </summary>
public class PlayerToolVisual : MonoBehaviour
{
    [Header("Hand Bone")]
    [SerializeField] private Transform handBone;  // 플레이어 Hand 본/오브젝트

    [Header("Swing Settings (곡괭이)")]
    [SerializeField] private float swingAngle = 60f;
    [SerializeField] private float swingDuration = 0.15f;
    [SerializeField] private int swingCount = 3;

    [Header("Vibration Settings (드릴 / 불도저)")]
    [SerializeField] private float vibrationAmount = 0.03f;  // 진동 크기
    [SerializeField] private float vibrationSpeed = 25f;    // 진동 속도
    [SerializeField] private float vibrationDuration = 0.4f;   // 진동 지속 시간

    private GameObject _currentTool;
    private Coroutine _actionCoroutine;

    private PlayerMining.MiningMode _currentMode
        = PlayerMining.MiningMode.Pickaxe;

    private void Start()
    {
        BuildTool(PlayerMining.MiningMode.Pickaxe);
        SetToolActive(false);
    }

    // ── 외부 모드 전환 ───────────────────────────────────────────────────
    public void SetMode(PlayerMining.MiningMode mode)
    {
        if (_currentMode == mode) return;
        _currentMode = mode;
        BuildTool(mode);
        SetToolActive(false);
    }

    // ── 채광 연출 시작 ───────────────────────────────────────────────────
    public void PlayMiningAction()
    {
        if (_actionCoroutine != null)
            StopCoroutine(_actionCoroutine);

        _actionCoroutine = _currentMode switch
        {
            PlayerMining.MiningMode.Pickaxe =>
                StartCoroutine(SwingRoutine()),
            PlayerMining.MiningMode.Drill =>
                StartCoroutine(VibrationRoutine()),
            PlayerMining.MiningMode.Bulldozer =>
                StartCoroutine(VibrationRoutine()),
            _ => StartCoroutine(SwingRoutine())
        };
    }

    // ── 도구 활성 / 비활성 ───────────────────────────────────────────────
    private void SetToolActive(bool active)
    {
        if (_currentTool != null)
            _currentTool.SetActive(active);
    }
    // ── 도구 생성 ────────────────────────────────────────────────────────
    private void BuildTool(PlayerMining.MiningMode mode)
    {
        if (_currentTool != null)
            Destroy(_currentTool);

        _currentTool = mode switch
        {
            PlayerMining.MiningMode.Pickaxe => BuildPickaxe(),
            PlayerMining.MiningMode.Drill => BuildDrill(),
            PlayerMining.MiningMode.Bulldozer => BuildBulldozer(),
            _ => BuildPickaxe()
        };
    }

    // ── 곡괭이 ───────────────────────────────────────────────────────────
    private GameObject BuildPickaxe()
    {
        GameObject root = new GameObject("Pickaxe");
        root.transform.SetParent(handBone);

        root.transform.localScale = Vector3.one * 1.8f;
        root.transform.SetLocalPositionAndRotation(
            new Vector3(0f, 0.1f, 0.05f),
            Quaternion.Euler(0f, -90f, -30f));

        // 자루
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: Vector3.zero,
            localScale: new Vector3(0.05f, 0.35f, 0.05f),
            color: new Color(0.55f, 0.35f, 0.1f));

        // 헤드
        CreatePrimitive(PrimitiveType.Cube, root.transform,
            localPos: new Vector3(0f, 0.38f, 0f),
            localScale: new Vector3(0.08f, 0.18f, 0.08f),
            color: new Color(0.5f, 0.5f, 0.55f));

        // 픽 끝
        GameObject tip = CreatePrimitive(PrimitiveType.Cube, root.transform,
            localPos: new Vector3(0.08f, 0.42f, 0f),
            localScale: new Vector3(0.12f, 0.04f, 0.04f),
            color: new Color(0.45f, 0.45f, 0.5f));
        tip.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);

        return root;
    }

    // ── 드릴 ─────────────────────────────────────────────────────────────
    private GameObject BuildDrill()
    {
        GameObject root = new GameObject("Drill");
        root.transform.SetParent(handBone);

        root.transform.localScale = Vector3.one * 1.8f;
        root.transform.SetLocalPositionAndRotation(
            new Vector3(0f, 0.05f, 0.15f),
            Quaternion.Euler(90f, 0f, 0f));  // 앞을 향해

        // 본체
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: Vector3.zero,
            localScale: new Vector3(0.1f, 0.25f, 0.1f),
            color: new Color(0.2f, 0.2f, 0.8f));

        // 드릴 비트
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: new Vector3(0f, 0.32f, 0f),
            localScale: new Vector3(0.04f, 0.2f, 0.04f),
            color: new Color(0.6f, 0.6f, 0.65f));

        // 손잡이
        CreatePrimitive(PrimitiveType.Cube, root.transform,
            localPos: new Vector3(0f, -0.28f, 0f),
            localScale: new Vector3(0.12f, 0.08f, 0.12f),
            color: new Color(0.15f, 0.15f, 0.15f));

        return root;
    }

    // ── 드릴 불도저 ──────────────────────────────────────────────────────

    private GameObject BuildBulldozer()
    {
        GameObject root = new GameObject("DrillBulldozer");
        root.transform.SetParent(handBone);

        root.transform.SetLocalPositionAndRotation(
            new Vector3(0f, 0.1f, 1.2f),
            Quaternion.Euler(90f, 0f, 0f));

        // 1. 메인 헤드
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: Vector3.zero,
            localScale: new Vector3(5.0f, 0.5f, 3.0f),
            color: new Color(0.8f, 0.4f, 0.1f));

        // 2. 드릴 비트들 
        float bitSpacing = 1.5f;
        float bitSize = 0.4f;

        // 좌측 비트
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: new Vector3(-bitSpacing, 0.6f, 0f),
            localScale: new Vector3(bitSize, 0.8f, bitSize),
            color: new Color(0.6f, 0.6f, 0.65f));

        // 중앙 비트
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: new Vector3(0f, 0.7f, 0f),
            localScale: new Vector3(bitSize + 0.1f, 1.0f, bitSize + 0.1f),
            color: new Color(0.55f, 0.55f, 0.6f));

        // 우측 비트
        CreatePrimitive(PrimitiveType.Cylinder, root.transform,
            localPos: new Vector3(bitSpacing, 0.6f, 0f),
            localScale: new Vector3(bitSize, 0.8f, bitSize),
            color: new Color(0.6f, 0.6f, 0.65f));

        // 3. 연결 암
        CreatePrimitive(PrimitiveType.Cube, root.transform,
            localPos: new Vector3(0f, -0.8f, 0f),
            localScale: new Vector3(4.5f, 0.3f, 0.4f),
            color: new Color(0.3f, 0.3f, 0.35f));

        return root;
    }

    // ── 스윙 연출 (곡괭이) ───────────────────────────────────────────────
    private IEnumerator SwingRoutine()
    {
        SetToolActive(true);

        if (handBone == null) yield break;

        Quaternion originRot = handBone.localRotation;
        Quaternion swingRot = Quaternion.Euler(swingAngle, 0f, 0f) * originRot;

        for (int i = 0; i < swingCount; i++)
        {
            yield return StartCoroutine(
                RotateTool(originRot, swingRot, swingDuration));
            yield return StartCoroutine(
                RotateTool(swingRot, originRot, swingDuration));
        }

        handBone.localRotation = originRot;
        _actionCoroutine = null;

        SetToolActive(false);
    }

    private IEnumerator RotateTool(
        Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            handBone.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        handBone.localRotation = to;
    }

    // ── 진동 연출 (드릴 / 불도저) ───────────────────────────────────────
    private IEnumerator VibrationRoutine()
    {
        SetToolActive(true);

        if (_currentTool == null) yield break;

        Vector3 originLocalPos = _currentTool.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < vibrationDuration)
        {
            elapsed += Time.deltaTime;

            // Sin 기반 진동 — X/Y 축 랜덤 오프셋
            float offsetX = Mathf.Sin(elapsed * vibrationSpeed)
                            * vibrationAmount;
            float offsetY = Mathf.Cos(elapsed * vibrationSpeed * 1.3f)
                            * vibrationAmount * 0.5f;

            _currentTool.transform.localPosition =
                originLocalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        // 원래 위치 복원
        _currentTool.transform.localPosition = originLocalPos;
        _actionCoroutine = null;

        SetToolActive(false);
    }

    // ── Primitive 생성 헬퍼 ──────────────────────────────────────────────
    private GameObject CreatePrimitive(
        PrimitiveType type,
        Transform parent,
        Vector3 localPos,
        Vector3 localScale,
        Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(
                Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard"));
            rend.material.color = color;
        }

        // 도구 콜라이더 제거 — 물리/트리거 영향 차단
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        return obj;
    }
}