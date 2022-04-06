using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//活体组件 生命值、死亡状态、伤害系统
public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;//默认生命值
    public float health { get; protected set; }//生命值
    protected bool dead;//死亡状态

    public event System.Action OnDeath;//System.Action委托 名为Ondeath的事件

    protected virtual void Start()//虚函数
    {
        health = startingHealth;
    }
    //被击中
    public virtual void TakeHit(float damage, Vector3 hitPoint,Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    //造成伤害
    public virtual void TakeDamage(float damage)
    {
        health -= damage;//生命值减少
        if (health <= 0 && !dead)
        {
            Die();
        }
    }
    [ContextMenu("Self Destruct")]
    public virtual void Die()
    {
        dead = true;
        if(OnDeath != null)
        {
            OnDeath();
        }
        Destroy(gameObject);
    }
}
