
public interface IDamageable
{
    void TakeDamage(int amount);
}


public interface IMovable
{
    void Move(float direction);
}


public interface IJumper
{
    void Jump();
}


public interface IShooter
{
    void Shoot();
}
