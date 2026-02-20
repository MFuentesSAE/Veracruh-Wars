using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float rotationSpeed = 18f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("FirePoint")]
    [SerializeField] private Transform firePoint;

    [Header("Weapon Visuals")]
    [SerializeField] private GameObject rifleGO;
    [SerializeField] private GameObject flamethrowerGO;
    [SerializeField] private GameObject rpgGO;

    [Header("Rifle")]
    [SerializeField] private GameObject rifleBulletPrefab;
    [SerializeField] private float rifleFireRate = 8f;

    [Header("Flamethrower")]
    [SerializeField] private GameObject flameBullet;
    [SerializeField] private float flameFireRate = 12f;
    [SerializeField] private float flameBloomDegrees = 8f;

    [Header("RPG-7")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private float rpgFireRate = 1.0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animDamp = 0.12f;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private Camera camera;

    private bool _isFiring, _isAiming;
    private float _fireTimer;

    private WeaponType _currentWeapon = WeaponType.Rifle;
    private bool _hasRifle = true;
    private bool _hasFlame = false;
    private bool _hasRpg = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (firePoint == null) firePoint = transform;
        if (animator == null) animator = GetComponentInChildren<Animator>();

        camera = Camera.main;

        EquipWeapon(WeaponType.Rifle);
    }

    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    public void OnFire(InputValue value) => _isFiring = value.isPressed;
    public void OnRightClick(InputValue value) => _isAiming = value.isPressed;

    public void PickupWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Rifle: _hasRifle = true; break;
            case WeaponType.Flamethrower: _hasFlame = true; break;
            case WeaponType.RPG: _hasRpg = true; break;
        }
        EquipWeapon(type);
    }

    private bool IsWeaponUnlocked(WeaponType type)
    {
        return type switch
        {
            WeaponType.Rifle => _hasRifle,
            WeaponType.Flamethrower => _hasFlame,
            WeaponType.RPG => _hasRpg,
            _ => false
        };
    }

    public void EquipWeapon(WeaponType type)
    {
        if (!IsWeaponUnlocked(type))
            return;

        _currentWeapon = type;
        _fireTimer = 0f;

        if (rifleGO != null) rifleGO.SetActive(type == WeaponType.Rifle);
        if (flamethrowerGO != null) flamethrowerGO.SetActive(type == WeaponType.Flamethrower);
        if (rpgGO != null) rpgGO.SetActive(type == WeaponType.RPG);
    }

    private void Update()
    {
        // MOVIMIENTO
        Vector3 dir = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float speed = moveSpeed;
        bool isRunning = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed && dir.sqrMagnitude > 0.0001f);
        if (isRunning) speed *= sprintMultiplier;

        _velocity = dir * speed;

        // ROTACIÓN
        if (_isAiming) Aim();
        else if (rotateToMoveDirection && dir.sqrMagnitude > 0.0001f) Rotate(dir);

        // ANIMACIÓN
        if (animator != null)
        {
            float max = moveSpeed * sprintMultiplier;
            float normalizedSpeed = (_velocity.magnitude / max);
            animator.SetFloat("Speed", normalizedSpeed, animDamp, Time.deltaTime);
        }

        // DISPARO
        _fireTimer -= Time.deltaTime;
        if (_isFiring && _fireTimer <= 0f)
        {
            Shoot();
            _fireTimer = 1f / GetCurrentFireRate();
        }
    }

    private float GetCurrentFireRate()
    {
        return _currentWeapon switch
        {
            WeaponType.Rifle => rifleFireRate,
            WeaponType.Flamethrower => flameFireRate,
            WeaponType.RPG => rpgFireRate,
            _ => rifleFireRate
        };
    }

    private void FixedUpdate()
    {
        Vector3 newPos = _rb.position + _velocity * Time.fixedDeltaTime;
        _rb.MovePosition(newPos);
    }

    private void Shoot()
    {
        switch (_currentWeapon)
        {
            case WeaponType.Rifle:
                SpawnSimple(rifleBulletPrefab, firePoint.position, firePoint.rotation);
                break;

            case WeaponType.Flamethrower:
                ShootFlameBloom();
                break;

            case WeaponType.RPG:
                SpawnSimple(rocketPrefab, firePoint.position, firePoint.rotation);
                break;
        }
    }

    private void ShootFlameBloom()
    {
        if (flameBullet == null) return;

        float yaw = Random.Range(-flameBloomDegrees, flameBloomDegrees);
        float pitch = Random.Range(-flameBloomDegrees * 0.25f, flameBloomDegrees * 0.25f);

        Quaternion rot = firePoint.rotation * Quaternion.Euler(pitch, yaw, 0f);
        Instantiate(flameBullet, firePoint.position, rot);
    }

    private void SpawnSimple(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab de proyectil no asignado para el arma actual.");
            return;
        }
        Instantiate(prefab, pos, rot);
    }

    private void Rotate(Vector3 rotationTarget)
    {
        if (rotationTarget.sqrMagnitude <= 0.0001f) return;
        Quaternion targetRotation = Quaternion.LookRotation(rotationTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Aim()
    {
        if (camera == null) camera = Camera.main;

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.value);
        Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity);

        Vector3 point = hit.collider != null ? hit.point : ray.GetPoint(50f);
        Vector3 direction = point - transform.position;
        direction.y = 0;
        Rotate(direction);
    }
}