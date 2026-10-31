using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    private int damage;
    private string ownerTag;

    public void Initialize(Vector2 direction, float speed, int damage, string ownerTag)
    {
        this.damage = damage;
        this.ownerTag = ownerTag;

        direction = direction.normalized;

        var body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.linearVelocity = direction * speed; 
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag(ownerTag)) return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
           
            Destroy(gameObject);
        }
    }
}
