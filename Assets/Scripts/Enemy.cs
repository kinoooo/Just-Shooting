using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//���˽ű�

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
public class Enemy : LivingEntity
{
    public enum State{Idle,Chasing,Attacking};//��������״̬ �� ���С�׷�𡢹���
    State currentState;//��ǰ״̬

    public ParticleSystem deathEffect;//����Ч��
    public static event System.Action OnDeathStatic;

    NavMeshAgent pathFinder;    //��������������
    Transform target;    //Ŀ�����
    LivingEntity targetEntity; //Ŀ���������
    Material skinMaterial;    //Դ���� ʵ�ֹ���ʱ��ɫ
    Color oringinalColor;     //ԭʼ��ɫ

    public float attackDistanceThreshold = .5f;//���ù�������
    public float timeBetweenAttack = 1;//���ù������
    public float damage = 1;//�����˺�

    float myCollisionRadius;//�ҵ���ײ�뾶
    float targetCollisionRadius;//Ŀ����ײ�뾶

    float nextAttackTime;//��һ�ι�����ʱ��

    bool hasTarget;//�Ƿ����Ŀ��

    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();//��������

        if (GameObject.FindGameObjectWithTag("Player") != null)//���������� ��ִ�����д���
        {

            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;//����Player��ǩ�ҵ����transform����ֵ��target
            targetEntity = target.GetComponent<LivingEntity>();//��ȡĿ��������

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;//Դ��ײ�뾶
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;//Ŀ����ײ�뾶


        }
    }
    protected override void Start()
    {
        base.Start();//����LivingEntity��Start()
        if (hasTarget)
        {
            currentState = State.Chasing;//״̬��Ϊ��׷��
            targetEntity.OnDeath += OnTargetDeath;//Ŀ������ʱִ�еķ��� ����OnDeath�¼�
            StartCoroutine(UpdataPath());//����Э���������
        }
    }
    //���õ�������
    public void setCharacteristics(float moveSpeed,int hitsToKillPlayer,float enemyHealth,Color skinColor)
    {
        pathFinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillPlayer);//����ȡ��
        }
        startingHealth = enemyHealth;

        deathEffect.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
        skinMaterial = GetComponent<Renderer>().material;//Դ���� ����bug
        skinMaterial.color = skinColor;
        oringinalColor = skinMaterial.color;//�õ�Դ���ʵ���ɫ
    }
    //Ŀ������ʱִ��
    void OnTargetDeath()
    {
        hasTarget = false;//Ŀ�겻����
        currentState = State.Idle;//״̬��Ϊ����
    }

    //��д�����е��÷���
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact",transform.position);//�����ܻ���Ч
        if (damage >= health)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);//����������Ч
            //����������Ч���� �����е� �������з�����ת  2������ٸö���
            Destroy( Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);//���û����TakeHit()
    }

    private void Update()
    {
        //ÿһ֡�������������λ�ã�������ķǳ������Դ 
        //pathFinder.SetDestination(target.position);
        if (hasTarget)//�������Ŀ��
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;//��Ŀ������ƽ��
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttack;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    //ʵ��ײ���ص��Ĺ���Ч��
    IEnumerator Attack()
    {
        currentState = State.Attacking;//����ǰ״̬��Ϊ����
        pathFinder.enabled = false;//�رյ�������ֹ����Ŀ���Ӱ�칥������

        Vector3 oringinalPosition = transform.position;
        //��ҪĿ�귽��          Ŀ��λ��-Դλ�� �õ�Դָ��Ŀ������� �ٱ�׼������
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        //����λ��=Ŀ��λ��-Ŀ���׼����*(�ҵ���ײ�뾶+Ŀ����ײ�뾶+��������)
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;//�����������ٶ�
        float percent = 0 ;//����������ɰٷֱ�

        //������ɫ��Ϊ����ɫ
        skinMaterial.color = Color.red;

        bool hasAppliedDamage = false;//�Ƿ�������˺�

        while (percent <= 1)
        {
            if(percent>.5f && !hasAppliedDamage)
            {
                targetEntity.TakeDamage(damage);
                hasAppliedDamage = true;
            }

            percent += Time.deltaTime * attackSpeed;//����������ɰٷֱ�+=����ʱ��*�����ٶ�

            //y=4*(-x^2+x) �����߲�ֵ ������(1/2,1)x�ύ��Ϊ(0,0)(1,0)
            //�� 0.5��ʱ����Ŀ��㣬1��ʱ�ص�ԭ��
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4 ;
            transform.position = Vector3.Lerp(oringinalPosition, attackPosition, interpolation);

            yield return null;
        }
        skinMaterial.color = oringinalColor;
        currentState = State.Chasing;//״̬��Ϊ��׷��
        pathFinder.enabled = true;//��������Э��
    }


    //��IEnumeratorЭ��ִ�е��� ���õ�ˢ����
    IEnumerator UpdataPath()
    {
        float refreshRate = .25f;//ˢ������

        while (hasTarget)
        {
            if(currentState == State.Chasing) 
            {
                //��ҪĿ�귽��          Ŀ��λ��-Դλ�� �õ�Դָ��Ŀ������� �ٱ�׼������
                Vector3 dirToTarget = (target.position-transform.position).normalized;
                //Ŀ��λ��=Ŀ��λ��-Ŀ���׼����*(�ҵ���ײ�뾶+Ŀ����ײ�뾶+��������)
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead)
                {
                    pathFinder.SetDestination(targetPosition);//�������λ��
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
