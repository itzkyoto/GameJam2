using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Enemy : Entity, IMovable
{
    [Header("Enemigo")]
    [SerializeField] protected int contactDamage = 1;

    protected Transform target;
    protected Collider2D col;
    protected float facingDirection = 1f;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Move(0f);
            return;
        }

        Behave();
    }

    protected abstract void Behave();

    protected float HorizontalDistanceToTarget => Mathf.Abs(target.position.x - transform.position.x);

    protected float BodyWidth => col.bounds.size.x;

    protected void FaceTarget()
    {
        float dx = target.position.x - transform.position.x;

        if (dx != 0f)
            facingDirection = Mathf.Sign(dx);

        transform.localScale = new Vector3(facingDirection, 1f, 1f);
    }

    public void Move(float direction)
    {
        rb.linearVelocity = new Vector2(direction * stats.moveSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.collider.CompareTag("Player") &&
            collision.collider.TryGetComponent<IDamageable>(out var victim))
        {
            victim.TakeDamage(contactDamage);
        }
    }

    protected override void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }
}
