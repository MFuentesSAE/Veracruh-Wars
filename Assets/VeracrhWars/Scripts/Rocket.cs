using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rocket : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifeTime = 3.5f;
    [SerializeField] private float explosionRadius = 4f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        _rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var h in hits)
        {
            EnemyController enemy = h.GetComponentInParent<EnemyController>();
            if (enemy != null) enemy.Die();
        }

        Destroy(gameObject);
    }
}