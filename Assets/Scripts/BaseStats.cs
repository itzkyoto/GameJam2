using System;
using UnityEngine;


[Serializable]
public class BaseStats
{
    [Header("Vida")]
    public int maxHealth = 100;

    [Header("Movilidad")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Header("Disparo")]
    public int damage = 10;
    public float fireRate = 0.2f;        
    public float projectileSpeed = 18f;
}
