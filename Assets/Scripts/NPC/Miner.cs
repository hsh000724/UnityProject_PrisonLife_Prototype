using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 광부 AI.
/// NavMesh 없이 Transform 직접 이동으로 웨이포인트 순찰.
/// OreObject 접촉 시 자동 채광 → OreZone 즉시 전달.
/// </summary>
public class Miner : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float waypointThreshold = 0.3f;
    [SerializeField] private float waitAtWaypoint = 0.5f;

    [Header("Mining Settings")]
    [SerializeField] private float miningDelay = 1.0f;
    [SerializeField] private float miningCooldown = 0.5f;

    [Header("References")]
    [SerializeField] private OreZone oreZone;

    private Animator _animator;
    private bool _hasAnimator;
    private bool _isMining;
    private bool _isWaiting;
    private float _cooldownTimer;
    private int _currentWaypointIndex;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MiningHash = Animator.StringToHash("IsMining");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null
            && _animator.runtimeAnimatorController != null;
    }

    private void OnEnable()
    {
        _isMining = false;
        _isWaiting = false;
        _cooldownTimer = 0f;
        _currentWaypointIndex = 0;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (_isMining || _isWaiting) return;

        MoveToCurrentWaypoint();
        CheckWaypointArrival();
        UpdateAnimator();
    }

    // ── 이동 ─────────────────────────────────────────────────────────────
    private void MoveToCurrentWaypoint()
    {
        if (waypoints.Length == 0) return;

        Vector3 target = waypoints[_currentWaypointIndex].position;
        target.y = transform.position.y; // 수직 이동 방지

        Vector3 dir = (target - transform.position).normalized;

        // 이동
        transform.position += dir * moveSpeed * Time.deltaTime;

        // 목표 방향으로 회전
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ── 웨이포인트 도착 체크 ─────────────────────────────────────────────
    private void CheckWaypointArrival()
    {
        if (waypoints.Length == 0) return;

        Vector3 target = waypoints[_currentWaypointIndex].position;
        target.y = transform.position.y;

        float dist = Vector3.Distance(transform.position, target);
        if (dist > waypointThreshold) return;

        StartCoroutine(WaitAndMoveNext());
    }

    private IEnumerator WaitAndMoveNext()
    {
        _isWaiting = true;
        SetAnimatorSpeed(0f);

        yield return new WaitForSeconds(waitAtWaypoint);

        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
        _isWaiting = false;
    }

    // ── 채광 시도 (OreObject → 호출) ────────────────────────────────────
    public void TryStartMining(OreObject ore)
    {
        if (_isMining || _cooldownTimer > 0f) return;
        if (ore == null || ore.IsMining) return;

        StartCoroutine(MineRoutine(ore));
    }

    private IEnumerator MineRoutine(OreObject ore)
    {
        _isMining = true;
        SoundManager.Instance?.PlayWithRandomPitch(SoundManager.SFX.Pickaxe);
        SetAnimatorSpeed(0f);

        if (_hasAnimator)
            _animator.SetBool(MiningHash, true);

        yield return new WaitForSeconds(miningDelay);

        if (_hasAnimator)
            _animator.SetBool(MiningHash, false);

        if (ore != null && ore.gameObject.activeSelf && !ore.IsMining)
        {
            ore.Mine();
            oreZone?.ReceiveOreFromMiner(1);

            FloatingTextPool.Instance?.Spawn(
                "+1 Ore",
                transform.position + Vector3.up * 1.5f,
                Color.green);
        }

        _cooldownTimer = miningCooldown;
        _isMining = false;
    }

    // ── 애니메이터 ───────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        if (!_hasAnimator) return;
        float speed = (_isMining || _isWaiting) ? 0f : 1f;
        SetAnimatorSpeed(speed);
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (_hasAnimator)
            _animator.SetFloat(SpeedHash, speed);
    }
}