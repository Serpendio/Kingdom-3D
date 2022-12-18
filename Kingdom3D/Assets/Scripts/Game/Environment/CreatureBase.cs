using UnityEngine;

public abstract class CreatureBase : MonoBehaviour
{
    [SerializeField, Min(0.1f)] protected float moveSpeed;
    [SerializeField, Min(0.1f)] protected float health;
    protected bool isAlive = true;

    public abstract void Damage(float damage, Vector3 source);

    protected abstract void Die();
}
