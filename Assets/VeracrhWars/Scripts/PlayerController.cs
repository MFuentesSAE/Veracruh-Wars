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

	[Header("Shooting")]
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform firePoint;
	[SerializeField] private float fireRate = 8f; //Cadencia

	private Rigidbody _rb;
	private Vector2 _moveInput;
	private Vector3 _velocity;
	private Camera camera;

	private bool _isFiring, _isAiming;
	private float _fireTimer;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_rb.useGravity = true;
		_rb.interpolation = RigidbodyInterpolation.Interpolate;

		if (firePoint == null)
			firePoint = transform;

		camera = Camera.main;
	}

	public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

	public void OnFire(InputValue value) => _isFiring = value.isPressed;
	public void OnRightClick(InputValue value) => _isAiming = value.isPressed;


	private void Update()
	{
		// MOVIMIENTO
		Vector3 dir = new Vector3(_moveInput.x, 0f, _moveInput.y);
		if (dir.sqrMagnitude > 1f) dir.Normalize();

		float speed = moveSpeed;

		// Sprinting
		if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
			speed *= sprintMultiplier;

		_velocity = dir * speed;

		if (_isAiming)
		{
			Aim();
		}

		else if (rotateToMoveDirection && dir.sqrMagnitude > 0.0001f)
		{
			Rotate(dir);
		}

		// DISPARO
		_fireTimer -= Time.deltaTime;
		if (_isFiring && _fireTimer <= 0f)
		{
			Shoot();
			_fireTimer = 1f / fireRate;
		}
	}

	private void FixedUpdate()
	{
		Vector3 newPos = _rb.position + _velocity * Time.fixedDeltaTime;
		_rb.MovePosition(newPos);
	}

	private void Shoot()
	{
		if (bulletPrefab == null)
		{
			Debug.LogWarning("No bulletPrefab asignado en PlayerController.");
			return;
		}

		Vector3 pos = firePoint.position;
		Quaternion rot = firePoint.rotation;

		Instantiate(bulletPrefab, pos, rot);
	}

	private void Rotate(Vector3 rotationTarget)
	{
		Quaternion targetRotation = Quaternion.LookRotation(rotationTarget);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
	}

	private void Aim()
	{
		//Hacer un raycast desde la cámara hacia la pocisión del mopuse
		Ray ray = camera.ScreenPointToRay(Mouse.current.position.value);
		RaycastHit hit;
		Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);

		//Si el racyast golpea un punto ese será el objetivo para mirar, si no obtener el útimo punto del rayo
		Vector3 point = hit.collider != null ? hit.point : ray.GetPoint(Mathf.Infinity);
		Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity);

		//Obtener la dirección del punto obtenido respecto al jugador, hacer el eje y 0 para que solamente rote en dicho eje.
		Vector3 direction = point - transform.position;
		direction.y = 0;
		Rotate(direction);
	}
}
