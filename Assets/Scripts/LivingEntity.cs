using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//������� ����ֵ������״̬���˺�ϵͳ
public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;//Ĭ������ֵ
    public float health { get; protected set; }//����ֵ
    protected bool dead;//����״̬

    public event System.Action OnDeath;//System.Actionί�� ��ΪOndeath���¼�

    protected virtual void Start()//�麯��
    {
        health = startingHealth;
    }
    //������
    public virtual void TakeHit(float damage, Vector3 hitPoint,Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    //����˺�
    public virtual void TakeDamage(float damage)
    {
        health -= damage;//����ֵ����
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
