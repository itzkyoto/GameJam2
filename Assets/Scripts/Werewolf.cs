using System.Collections;
using UnityEngine;

public class Werewolf : Enemy, IShooter, IJumper
{
    private const int HitsToKill = 10;
    private const float SpeedFactor = 0f;
    private const int ProjectileDamage = 1;
    private const int BurstCount = 3;
    private const int FramesBetweenShots = 3;
    private const float BurstInterval = 2f;

    [Header("Ataque")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float stopDistanceInBodies = 4f;

    [Header("Punto de salida de los proyectiles (X se invierte según hacia dónde mira)")]
    [SerializeField] private Vector2 fireOffset = new Vector2(0.6f, 0.2f);

    [Header("Salto")]
    [SerializeField] private float jumpCheckDistance = 0.4f;
    [SerializeField] private float jumpCooldown = 0.4f;

    private float nextBurstTime;
    private float nextJumpTime;
    private bool isBursting;

    protected override void Awake()
    {
        stats.maxHealth = Player.BulletDamage * HitsToKill;
        stats.moveSpeed = Player.PlayerSpeed * SpeedFactor;
        stats.projectileSpeed = Player.BulletSpeed;
        stats.damage = ProjectileDamage;

        base.Awake();
    }

    protected override void Behave()
    {
        if (isBursting)
        {
            Move(0f);
            return;
        }

        if (!TargetInRange)
        {
            Patrol();
            nextBurstTime = 0f;
            return;
        }

        CancelPatrolPause();
        FaceTarget();

        if (HorizontalDistanceToTarget > stopDistanceInBodies * BodyWidth)
        {
            TryJumpObstacle();
            Move(facingDirection);
            nextBurstTime = 0f;
        }
        else
        {
            Move(0f);
            TryBurst();
        }
    }

    private void TryBurst()
    {
        if (Time.time < nextBurstTime) return;

        nextBurstTime = Time.time + BurstInterval;
        StartCoroutine(FireBurst());
    }

    private IEnumerator FireBurst()
    {
        isBursting = true;

        for (int i = 0; i < BurstCount; i++)
        {
            if (isDead) break;

            Shoot();

            if (i < BurstCount - 1)
            {
                for (int f = 0; f < FramesBetweenShots; f++)
                    yield return null;
            }
        }

        isBursting = false;
    }

    private bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - 0.05f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.1f);

        return Physics2D.OverlapBox(center, size, 0f, obstacleLayer) != null;
    }

    private bool ObstacleAhead()
    {
        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x + facingDirection * b.extents.x, b.min.y + 0.15f);

        return Physics2D.Raycast(origin, Vector2.right * facingDirection, jumpCheckDistance, obstacleLayer).collider != null;
    }

    private void TryJumpObstacle()
    {
        if (Time.time < nextJumpTime) return;
        if (!IsGrounded() || !ObstacleAhead()) return;

        Jump();
        nextJumpTime = Time.time + jumpCooldown;
    }

    public void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
    }

    public void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = Vector2.right * facingDirection;
        Vector2 origin = (Vector2)transform.position + new Vector2(fireOffset.x * facingDirection, fireOffset.y);

        Projectile bullet = Instantiate(projectilePrefab, origin, Quaternion.identity);
        bullet.Initialize(direction, stats.projectileSpeed, stats.damage, tag);
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

        Bounds b = c.bounds;

        Gizmos.color = Color.green;
        Vector3 rayOrigin = new Vector3(b.center.x + facing * b.extents.x, b.min.y + 0.15f, 0f);
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.right * facing * jumpCheckDistance);

        Gizmos.DrawWireCube(new Vector3(b.center.x, b.min.y - 0.05f, 0f), new Vector3(b.size.x * 0.9f, 0.1f, 0f));
    }
}
