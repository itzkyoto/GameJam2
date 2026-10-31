using UnityEngine;


public class Enemy : Entity, IMovable
{
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private int contactDamage = 10;

    private Vector2 startPosition;
    private float direction = 1f;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        float offset = transform.position.x - startPosition.x;
        if (Mathf.Abs(offset) >= patrolDistance)
            direction = -Mathf.Sign(offset);

        Move(direction);
    }

    public void Move(float dir)
    {
        rb.linearVelocity = new Vector2(dir * stats.moveSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(dir, 1f, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<IDamageable>(out var target) && collision.collider.CompareTag("Player"))
            target.TakeDamage(contactDamage);
    }

    protected override void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }
}
