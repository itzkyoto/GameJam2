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

public interface IFlyer
{
    void Fly(UnityEngine.Vector2 direction);
}

public interface IMeleeAttacker
{
    void MeleeAttack();
}
