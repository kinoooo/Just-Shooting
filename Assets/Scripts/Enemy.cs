using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//敌人脚本

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
public class Enemy : LivingEntity
{
    public enum State{Idle,Chasing,Attacking};//设置三种状态 ： 空闲、追逐、攻击
    State currentState;//当前状态

    public ParticleSystem deathEffect;//死亡效果
    public static event System.Action OnDeathStatic;

    NavMeshAgent pathFinder;    //导航网格代理对象
    Transform target;    //目标对象
    LivingEntity targetEntity; //目标生物组件
    Material skinMaterial;    //源材质 实现攻击时变色
    Color oringinalColor;     //原始颜色

    public float attackDistanceThreshold = .5f;//设置攻击距离
    public float timeBetweenAttack = 1;//设置攻击间隔
    public float damage = 1;//攻击伤害

    float myCollisionRadius;//我的碰撞半径
    float targetCollisionRadius;//目标碰撞半径

    float nextAttackTime;//下一次攻击的时间

    bool hasTarget;//是否存在目标

    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();//导航代理

        if (GameObject.FindGameObjectWithTag("Player") != null)//如果存在玩家 则执行下列代码
        {

            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;//根据Player标签找到玩家transform，赋值给target
            targetEntity = target.GetComponent<LivingEntity>();//获取目标活体组件

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;//源碰撞半径
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;//目标碰撞半径


        }
    }
    protected override void Start()
    {
        base.Start();//调用LivingEntity的Start()
        if (hasTarget)
        {
            currentState = State.Chasing;//状态置为：追逐
            targetEntity.OnDeath += OnTargetDeath;//目标死亡时执行的方法 传入OnDeath事件
            StartCoroutine(UpdataPath());//启动协程搜索玩家
        }
    }
    //设置敌人属性
    public void setCharacteristics(float moveSpeed,int hitsToKillPlayer,float enemyHealth,Color skinColor)
    {
        pathFinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillPlayer);//向上取整
        }
        startingHealth = enemyHealth;

        deathEffect.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
        skinMaterial = GetComponent<Renderer>().material;//源材质 存在bug
        skinMaterial.color = skinColor;
        oringinalColor = skinMaterial.color;//得到源材质的颜色
    }
    //目标死亡时执行
    void OnTargetDeath()
    {
        hasTarget = false;//目标不存在
        currentState = State.Idle;//状态置为空闲
    }

    //重写被击中调用方法
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact",transform.position);//播放受击音效
        if (damage >= health)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);//播放死亡音效
            //生成粒子特效对象 在命中点 根据命中方向旋转  2秒后销毁该对象
            Destroy( Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);//调用基类的TakeHit()
    }

    private void Update()
    {
        //每一帧都重新搜索玩家位置，这会消耗非常多的资源 
        //pathFinder.SetDestination(target.position);
        if (hasTarget)//如果存在目标
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;//到目标距离的平方
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttack;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    //实现撞击回弹的攻击效果
    IEnumerator Attack()
    {
        currentState = State.Attacking;//将当前状态置为攻击
        pathFinder.enabled = false;//关闭导航，防止重设目标点影响攻击动作

        Vector3 oringinalPosition = transform.position;
        //需要目标方向          目标位置-源位置 得到源指向目标的向量 再标准化向量
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        //攻击位置=目标位置-目标标准向量*(我的碰撞半径+目标碰撞半径+攻击距离)
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;//攻击动作的速度
        float percent = 0 ;//攻击动作完成百分比

        //材质颜色置为：红色
        skinMaterial.color = Color.red;

        bool hasAppliedDamage = false;//是否造成了伤害

        while (percent <= 1)
        {
            if(percent>.5f && !hasAppliedDamage)
            {
                targetEntity.TakeDamage(damage);
                hasAppliedDamage = true;
            }

            percent += Time.deltaTime * attackSpeed;//攻击动作完成百分比+=经过时间*攻击速度

            //y=4*(-x^2+x) 抛物线插值 顶点在(1/2,1)x轴交点为(0,0)(1,0)
            //即 0.5秒时到达目标点，1秒时回到原点
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4 ;
            transform.position = Vector3.Lerp(oringinalPosition, attackPosition, interpolation);

            yield return null;
        }
        skinMaterial.color = oringinalColor;
        currentState = State.Chasing;//状态置为：追逐
        pathFinder.enabled = true;//重启导航协程
    }


    //用IEnumerator协程执行导航 设置低刷新率
    IEnumerator UpdataPath()
    {
        float refreshRate = .25f;//刷新速率

        while (hasTarget)
        {
            if(currentState == State.Chasing) 
            {
                //需要目标方向          目标位置-源位置 得到源指向目标的向量 再标准化向量
                Vector3 dirToTarget = (target.position-transform.position).normalized;
                //目标位置=目标位置-目标标准向量*(我的碰撞半径+目标碰撞半径+攻击距离)
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead)
                {
                    pathFinder.SetDestination(targetPosition);//搜索玩家位置
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
