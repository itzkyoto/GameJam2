using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Enemy : Entity, IMovable
{
    [Header("Enemigo")]
    [SerializeField] protected int contactDamage = 1;
    [SerializeField] protected float detectionRangeInBodies = 12f;

    [Header("Patrulla")]
    [SerializeField] protected LayerMask obstacleLayer;
    [SerializeField] protected float wallCheckDistance = 0.15f;
    [SerializeField] protected bool startPatrolRight = true;
    [SerializeField] protected float turnPauseTime = 1f;

    protected Transform target;
    protected Collider2D col;
    protected float facingDirection = 1f;

    private float patrolDirection = 1f;
    private float pauseEndTime;
    private bool waitingToTurn;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider2D>();

        patrolDirection = startPatrolRight ? 1f : -1f;
        SetFacing(patrolDirection);
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

        Behave();
    }

    protected abstract void Behave();

    protected bool HasTarget => target != null && target.gameObject.activeInHierarchy;

    protected float HorizontalDistanceToTarget => Mathf.Abs(target.position.x - transform.position.x);

    protected float BodyWidth => col.bounds.size.x;

    protected bool TargetInRange => HasTarget && HorizontalDistanceToTarget <= detectionRangeInBodies * BodyWidth;

    protected void SetFacing(float direction)
    {
        if (direction == 0f) return;

        facingDirection = Mathf.Sign(direction);
        transform.localScale = new Vector3(facingDirection, 1f, 1f);
    }

    protected void FaceTarget()
    {
        SetFacing(target.position.x - transform.position.x);
    }

    protected void Patrol()
    {
        if (waitingToTurn)
        {
            Move(0f);

            if (Time.time >= pauseEndTime)
            {
                waitingToTurn = false;
                patrolDirection = -patrolDirection;
                SetFacing(patrolDirection);
            }

            return;
        }

        if (WallAhead(patrolDirection))
        {
            waitingToTurn = true;
            pauseEndTime = Time.time + turnPauseTime;
            Move(0f);
            return;
        }

        SetFacing(patrolDirection);
        Move(patrolDirection);
    }

    protected void CancelPatrolPause()
    {
        waitingToTurn = false;
    }

    private bool WallAhead(float direction)
    {
        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x + direction * b.extents.x, b.center.y);
        Vector2 size = new Vector2(0.05f, b.size.y * 0.8f);

        return Physics2D.BoxCast(origin, size, 0f, Vector2.right * direction, wallCheckDistance, obstacleLayer).collider != null;
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

    protected virtual void OnDrawGizmosSelected()
    {
        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        Bounds b = c.bounds;
        Vector3 boxSize = new Vector3(0.05f, b.size.y * 0.8f, 0f);

        Gizmos.color = Color.yellow;
        foreach (float dir in new[] { -1f, 1f })
        {
            Vector3 origin = new Vector3(b.center.x + dir * b.extents.x, b.center.y, 0f);
            Gizmos.DrawWireCube(origin + Vector3.right * dir * wallCheckDistance, boxSize);
        }

        float range = detectionRangeInBodies * b.size.x;
        Vector3 pos = transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pos + Vector3.left * range + Vector3.down * 0.5f, pos + Vector3.left * range + Vector3.up * 0.5f);
        Gizmos.DrawLine(pos + Vector3.right * range + Vector3.down * 0.5f, pos + Vector3.right * range + Vector3.up * 0.5f);
    }
}
