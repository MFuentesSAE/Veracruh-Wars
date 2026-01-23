using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float rotationSpeed = 18f;

    private Rigidbody _rb;

    private Vector2 _moveInput;     
    private Vector3 _velocity;      

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Player Input (Send Messages)
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        Vector3 dir = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        _velocity = dir * moveSpeed;

        if (rotateToMoveDirection && dir.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        Vector3 newPos = _rb.position + _velocity * Time.fixedDeltaTime;
        _rb.MovePosition(newPos);
    }
}
