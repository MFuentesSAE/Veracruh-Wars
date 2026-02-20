using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotateSpeed = 12f;
    [SerializeField] private float stopDistance = 0.9f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackPause = 1.0f;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Damage")]
    [Tooltip("Si está activo, si el enemigo toca al player también aplica daño (por si algo falla con el ataque por rango).")]
    [SerializeField] private bool contactDamageEnabled = true;
    [SerializeField] private float contactDamageCooldown = 0.6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathDestroyDelay = 1.2f;

    private Rigidbody _rb;
    private Transform _player;

    private bool _isAttacking;
    private bool _isDead;
    private float _nextAttackTime;

    private float _nextContactDamageTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    private void FixedUpdate()
    {
        if (_isDead || _isAttacking) return;
        if (_player == null) return;

        Vector3 toPlayer = _player.position - _rb.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (animator != null) animator.SetBool("IsMoving", dist > stopDistance);

        if (dist <= attackRange && Time.time >= _nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        if (dist <= stopDistance)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 dir = toPlayer / dist;

        Vector3 newPos = _rb.position + dir * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPos);

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion newRot = Quaternion.Slerp(_rb.rotation, target, rotateSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(newRot);
        }
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _nextAttackTime = Time.time + attackPause + attackCooldown;

        // detenerse
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        if (_player != null)
        {
            Vector3 look = _player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(look, Vector3.up);
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(attackPause);

        if (!_isDead && _player != null)
        {
            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.magnitude <= attackRange + 0.05f)
            {
                DealDamageToPlayer();
            }
        }

        _isAttacking = false;
    }

    private void DealDamageToPlayer()
    {
        if (_player == null) return;

        PlayerShield shield = _player.GetComponentInChildren<PlayerShield>();
        if (shield != null && shield.ConsumeShieldIfAny())
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerKilled();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!contactDamageEnabled || _isDead) return;
        if (Time.time < _nextContactDamageTime) return;

        if (collision.collider.CompareTag("Player"))
        {
            _nextContactDamageTime = Time.time + contactDamageCooldown;
            DealDamageToPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!contactDamageEnabled || _isDead) return;
        if (Time.time < _nextContactDamageTime) return;

        if (other.CompareTag("Player"))
        {
            _nextContactDamageTime = Time.time + contactDamageCooldown;
            DealDamageToPlayer();
        }
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyKilled(transform.position);

        if (WaveManager.Instance != null)
            WaveManager.Instance.UnregisterEnemy();

        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        if (animator != null)
            animator.SetTrigger("Death");

        Destroy(gameObject, deathDestroyDelay);
    }
}
