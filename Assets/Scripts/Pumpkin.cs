using UnityEngine;

public class Pumpkin : Enemy, IShooter, IFlyer
{
    private const int PumpkinHealth = 1;
    private const float FireInterval = 2f;
    private const int ProjectileDamage = 1;

    [Header("Ataque")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float hoverDistanceInBodies = 4f;

    [Header("Sonidos de la calabaza")]
    [SerializeField] private AudioClip shootSound;

    [Header("Flotación")]
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 0.8f;

    [Header("Patrulla de la calabaza")]
    [SerializeField] private float patrolTravelTime = 3f;

    private Vector2 lastKnownPosition;
    private float bobPhase;
    private float patrolDirection = 1f;
    private float patrolTimer;
    private float pauseTimer;
    private bool pausing;
    private float nextFireTime;

    protected override void Awake()
    {
        stats.maxHealth = PumpkinHealth;
        stats.moveSpeed = Player.PlayerSpeed;
        stats.projectileSpeed = Player.BulletSpeed;
        stats.damage = ProjectileDamage;

        base.Awake();

        rb.gravityScale = 0f;
        bobPhase = Random.value * Mathf.PI * 2f;

        patrolDirection = startPatrolRight ? 1f : -1f;
        patrolTimer = patrolTravelTime * 0.5f;
        SetFacing(patrolDirection);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!isDead)
            ApplyBob();
    }

    private void ApplyBob()
    {
        float omega = bobFrequency * Mathf.PI * 2f;
        float bobVelocity = bobAmplitude * omega * Mathf.Cos(omega * Time.time + bobPhase);

        rb.linearVelocity += new Vector2(0f, bobVelocity);
    }

    protected override void Behave()
    {
        if (HasTarget)
            lastKnownPosition = target.position;

        if (!TargetInRange)
        {
            FlyPatrol();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            nextFireTime = Time.time + FireInterval;
            return;
        }

        pausing = false;
        FaceTarget();

        Vector2 toTarget = lastKnownPosition - (Vector2)transform.position;
        float hoverDistance = hoverDistanceInBodies * BodyWidth;

        Fly(toTarget.magnitude > hoverDistance ? toTarget : Vector2.zero);
        TryShoot();
    }

    private void FlyPatrol()
    {
        if (pausing)
        {
            Move(0f);
            pauseTimer -= Time.fixedDeltaTime;

            if (pauseTimer <= 0f)
            {
                pausing = false;
                patrolDirection = -patrolDirection;
                patrolTimer = patrolTravelTime;
                SetFacing(patrolDirection);
            }

            return;
        }

        patrolTimer -= Time.fixedDeltaTime;

        if (patrolTimer <= 0f)
        {
            pausing = true;
            pauseTimer = turnPauseTime;
            Move(0f);
            return;
        }

        SetFacing(patrolDirection);
        Move(patrolDirection);
    }

    public void Fly(Vector2 direction)
    {
        rb.linearVelocity = direction.sqrMagnitude > 0f ? direction.normalized * stats.moveSpeed : Vector2.zero;
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        Shoot();
        nextFireTime = Time.time + FireInterval;
    }

    public void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 position = transform.position;
        Vector2 direction = lastKnownPosition - position;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right * facingDirection;

        direction.Normalize();

        Bounds b = col.bounds;
        float muzzleDistance = Mathf.Max(b.extents.x, b.extents.y) + 0.2f;

        AudioManager.Play(shootSound);

        Projectile bullet = Instantiate(projectilePrefab, position + direction * muzzleDistance, Quaternion.identity);
        bullet.Initialize(direction, stats.projectileSpeed, stats.damage, tag);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hoverDistanceInBodies * c.bounds.size.x);
    }
}
