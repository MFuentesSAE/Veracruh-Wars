using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 2f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void Init(float newSpeed, float newLifeTime)
    {
        speed = newSpeed;
        lifeTime = newLifeTime;
    }

    private void Start()
    {
        _rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            enemy.Die();
            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Player"))
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        EnemyController enemy = collision.collider.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            enemy.Die();
            Destroy(gameObject);
            return;
        }

        if (!collision.collider.CompareTag("Player"))
            Destroy(gameObject);
    }
}