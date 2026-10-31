using UnityEngine;


[RequireComponent(typeof(PlayerInputManager))]
public class Player : Entity, IMovable, IJumper, IShooter
{
   
    private const int PlayerHealth = 1;
    private const float PlayerSpeed = 5f;
    private const float AimThreshold = 0.5f;

    [Header("Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Salto")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

    [Header("Disparo")]
    [SerializeField] private Projectile projectilePrefab;

    [Header("Punto de salida de la bala (offset desde el Player; la X se invierte según hacia dónde mira)")]
    [SerializeField] private Vector2 forwardOffset = new Vector2(0.6f, 0.1f);
    [SerializeField] private Vector2 backOffset = new Vector2(-0.6f, 0.1f);
    [SerializeField] private Vector2 upOffset = new Vector2(0f, 1f);
    [SerializeField] private Vector2 downOffset = new Vector2(0f, -1f);

    private Vector2 moveInput;
    private float facingDirection = 1f;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool isGrounded;

    

    protected override void Awake()
    {
        
        stats.maxHealth = PlayerHealth;
        stats.moveSpeed = PlayerSpeed;

        base.Awake();
    }

    private void Update()
    {
        if (isDead) return;

        CheckGround();
        HandleJumpTimers();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        Move(moveInput.x);
        TryJump();
    }

   

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;

        if (input.x != 0f)
            facingDirection = Mathf.Sign(input.x);
    }

    public void OnJumpPressed() => jumpBufferCounter = jumpBufferTime;

    public void OnJumpReleased()
    {
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    
    public void OnShootPressed()
    {
        if (isDead) return;

        Shoot();
    }


    public void Move(float direction)
    {
        rb.linearVelocity = new Vector2(direction * stats.moveSpeed, rb.linearVelocity.y);

      
        transform.localScale = new Vector3(facingDirection, 1f, 1f);
    }

   

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleJumpTimers()
    {
        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }

    private void TryJump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    public void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
    }



    public void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = GetShootDirection();
        Vector2 origin = GetShotOrigin(direction);

        Projectile bullet = Instantiate(projectilePrefab, origin, Quaternion.identity);
        bullet.Initialize(direction, stats.projectileSpeed, stats.damage, tag);
    }

  
    private Vector2 GetShootDirection()
    {
        if (moveInput.y > AimThreshold)
            return Vector2.up;

        if (moveInput.y < -AimThreshold)
            return isGrounded ? Vector2.right * -facingDirection : Vector2.down;

        return Vector2.right * facingDirection;
    }

    private Vector2 GetShotOrigin(Vector2 direction)
    {
        Vector2 offset;

        if (direction == Vector2.up) offset = upOffset;
        else if (direction == Vector2.down) offset = downOffset;
        else offset = direction.x * facingDirection > 0f ? forwardOffset : backOffset;

        return (Vector2)transform.position + new Vector2(offset.x * facingDirection, offset.y);
    }

 

    protected override void Die()
    {
        isDead = true;
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Player murió");
      
        gameObject.SetActive(false);
    }

  

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        float facing = Application.isPlaying ? facingDirection : 1f;
        Vector2 pos = transform.position;

        Gizmos.color = Color.red;
        foreach (Vector2 o in new[] { forwardOffset, backOffset, upOffset, downOffset })
            Gizmos.DrawWireSphere(pos + new Vector2(o.x * facing, o.y), 0.08f);
    }
}
