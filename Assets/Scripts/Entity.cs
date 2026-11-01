using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour, IDamageable
{
    [SerializeField] protected BaseStats stats = new BaseStats();

    [Header("Sonidos")]
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip deathSound;

    protected Rigidbody2D rb;
    protected int currentHealth;
    protected bool isDead;

    public int CurrentHealth => currentHealth;
    public BaseStats Stats => stats;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = stats.maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        AudioManager.Play(currentHealth > 0 ? hitSound : deathSound);
        OnDamaged(amount);

        if (currentHealth <= 0)
            Die();
    }

    
    protected virtual void OnDamaged(int amount) { }

   
    protected abstract void Die();
}
