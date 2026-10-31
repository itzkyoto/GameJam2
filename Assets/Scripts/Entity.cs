using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour, IDamageable
{
    [SerializeField] protected BaseStats stats = new BaseStats();

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
        OnDamaged(amount);

        if (currentHealth <= 0)
            Die();
    }

    
    protected virtual void OnDamaged(int amount) { }

   
    protected abstract void Die();
}
