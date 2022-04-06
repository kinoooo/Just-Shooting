using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//伤害接口
public interface IDamageable
{
    void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection);//被命中 
    void TakeDamage(float damage);//造成伤害
}