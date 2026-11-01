using System.Collections;
using UnityEngine;

public class FinalBoss : Enemy, IShooter, IMeleeAttacker
{
    private const int HitsToKill = 100;
    private const int ProjectileDamage = 1;
    private const int SlashDamage = 1;
    private const float SlashReachInBodies = 2f;
    private const int SlashActiveFrames = 2;
    private const int SlashRecoveryFrames = 1;
    private const int TeleportAttempts = 8;

    [Header("Teletransporte")]
    [SerializeField] private float teleportRangeInBodies = 6f;

    [Header("Ciclo de ataque")]
    [SerializeField] private float actionDelay = 0.5f;
    [SerializeField] private float cycleInterval = 1.5f;

    [Header("Proyectil")]
    [SerializeField] private Projectile projectilePrefab;

    [Header("Sablazo")]
    [SerializeField] private GameObject slashEffect;

    private bool isAttacking;

    protected override void Awake()
    {
        stats.maxHealth = Player.BulletDamage * HitsToKill;
        stats.moveSpeed = 0f;
        stats.projectileSpeed = Player.BulletSpeed;
        stats.damage = ProjectileDamage;

        base.Awake();

        if (slashEffect != null)
            slashEffect.SetActive(false);

        if (obstacleLayer.value == 0)
            Debug.LogWarning("FinalBoss: Obstacle Layer está en Nothing. Sin esa capa no puede validar el suelo y nunca se teletransporta. Ponla en Ground.", this);
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(BossLoop());
    }

    protected override void Behave()
    {
        Move(0f);

        if (!isAttacking && HasTarget && TargetInRange)
            FaceTarget();
    }

    private IEnumerator BossLoop()
    {
        while (!isDead)
        {
            if (!HasTarget || !TargetInRange)
            {
                yield return null;
                continue;
            }

            TeleportNearTarget();
            FaceTarget();

            yield return new WaitForSeconds(actionDelay);

            if (!HasTarget) continue;

            FaceTarget();

            if (TargetInSlashRange())
            {
                MeleeAttack();
                yield return new WaitUntil(() => !isAttacking);
            }
            else
            {
                Shoot();
            }

            yield return new WaitForSeconds(cycleInterval);
        }
    }

    private bool TargetInSlashRange()
    {
        float reach = BodyWidth * 0.5f + SlashReachInBodies * BodyWidth;
        return HorizontalDistanceToTarget <= reach;
    }

    private void TeleportNearTarget()
    {
        float range = teleportRangeInBodies * BodyWidth;
        float minDistance = BodyWidth;

        for (int i = 0; i < TeleportAttempts; i++)
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            float distance = Random.Range(minDistance, Mathf.Max(minDistance, range));
            Vector2 candidate = new Vector2(target.position.x + side * distance, transform.position.y);

            if (!IsValidTeleportPoint(candidate)) continue;

            transform.position = candidate;
            rb.position = candidate;
            rb.linearVelocity = Vector2.zero;
            return;
        }
    }

    private bool IsValidTeleportPoint(Vector2 candidate)
    {
        Bounds b = col.bounds;
        Vector2 offset = b.center - transform.position;
        Vector2 center = candidate + offset;

        Vector2 toCandidate = center - (Vector2)b.center;
        RaycastHit2D pathHit = Physics2D.BoxCast(b.center, b.size * 0.9f, 0f, toCandidate.normalized, toCandidate.magnitude, obstacleLayer);
        if (pathHit.collider != null) return false;

        bool blocked = Physics2D.OverlapBox(center, b.size * 0.95f, 0f, obstacleLayer) != null;
        if (blocked) return false;

        return Physics2D.Raycast(center, Vector2.down, b.extents.y + 1f, obstacleLayer).collider != null;
    }

    public void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 position = transform.position;
        Vector2 direction = HasTarget ? (Vector2)target.position - position : Vector2.right * facingDirection;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right * facingDirection;

        direction.Normalize();

        Bounds b = col.bounds;
        float muzzleDistance = Mathf.Max(b.extents.x, b.extents.y) + 0.2f;

        Projectile bullet = Instantiate(projectilePrefab, position + direction * muzzleDistance, Quaternion.identity);
        bullet.Initialize(direction, stats.projectileSpeed, stats.damage, tag);
    }

    public void MeleeAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        StartCoroutine(SlashRoutine());
    }

    private IEnumerator SlashRoutine()
    {
        bool hasHit = false;

        if (slashEffect != null)
            slashEffect.SetActive(true);

        for (int f = 0; f < SlashActiveFrames; f++)
        {
            if (!hasHit)
                hasHit = SlashHitsTarget();

            yield return null;
        }

        if (slashEffect != null)
            slashEffect.SetActive(false);

        for (int f = 0; f < SlashRecoveryFrames; f++)
            yield return null;

        isAttacking = false;
    }

    private bool SlashHitsTarget()
    {
        GetSlashBox(col.bounds, facingDirection, out Vector2 center, out Vector2 size);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            if (hit.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(SlashDamage);
                return true;
            }
        }

        return false;
    }

    private void GetSlashBox(Bounds b, float facing, out Vector2 center, out Vector2 size)
    {
        float length = SlashReachInBodies * b.size.x;

        size = new Vector2(length, b.size.y);
        center = new Vector2(b.center.x + facing * (b.extents.x + length * 0.5f), b.center.y);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        float facing = Application.isPlaying ? facingDirection : 1f;
        Vector3 pos = transform.position;

        float teleportRange = teleportRangeInBodies * c.bounds.size.x;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos + Vector3.left * teleportRange + Vector3.down * 0.5f, pos + Vector3.left * teleportRange + Vector3.up * 0.5f);
        Gizmos.DrawLine(pos + Vector3.right * teleportRange + Vector3.down * 0.5f, pos + Vector3.right * teleportRange + Vector3.up * 0.5f);

        GetSlashBox(c.bounds, facing, out Vector2 slashCenter, out Vector2 slashSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(slashCenter, slashSize);
    }
}
