using UnityEngine;

public class Skeleton : Enemy, IShooter
{
    private const int SkeletonHealth = 20;
    private const float SpeedFactor = 0.5f;
    private const int FireballDamage = 1;

    [Header("Ataque")]
    [SerializeField] private Projectile fireballPrefab;
    [SerializeField] private float stopDistanceInBodies = 5f;
    [SerializeField] private float fireInterval = 1.5f;

    [Header("Punto de salida de la bola (X se invierte según hacia dónde mira)")]
    [SerializeField] private Vector2 fireOffset = new Vector2(0.6f, 0.2f);

    private float nextFireTime;

    protected override void Awake()
    {
        stats.maxHealth = SkeletonHealth;
        stats.moveSpeed = Player.PlayerSpeed * SpeedFactor;
        stats.projectileSpeed = Player.BulletSpeed;
        stats.damage = FireballDamage;

        base.Awake();
    }

    protected override void Behave()
    {
        if (!TargetInRange)
        {
            Patrol();
            return;
        }

        CancelPatrolPause();
        FaceTarget();

        float stopDistance = stopDistanceInBodies * BodyWidth;

        if (HorizontalDistanceToTarget > stopDistance)
        {
            Move(facingDirection);
        }
        else
        {
            Move(0f);
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        Shoot();
        nextFireTime = Time.time + fireInterval;
    }

    public void Shoot()
    {
        if (fireballPrefab == null) return;

        Vector2 direction = Vector2.right * facingDirection;
        Vector2 origin = (Vector2)transform.position + new Vector2(fireOffset.x * facingDirection, fireOffset.y);

        Projectile fireball = Instantiate(fireballPrefab, origin, Quaternion.identity);
        fireball.Initialize(direction, stats.projectileSpeed, stats.damage, tag);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        float stopDistance = stopDistanceInBodies * c.bounds.size.x;
        Vector3 pos = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pos + Vector3.left * stopDistance + Vector3.down * 0.5f, pos + Vector3.left * stopDistance + Vector3.up * 0.5f);
        Gizmos.DrawLine(pos + Vector3.right * stopDistance + Vector3.down * 0.5f, pos + Vector3.right * stopDistance + Vector3.up * 0.5f);

        float facing = Application.isPlaying ? facingDirection : 1f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos + new Vector3(fireOffset.x * facing, fireOffset.y, 0f), 0.08f);
    }
}
