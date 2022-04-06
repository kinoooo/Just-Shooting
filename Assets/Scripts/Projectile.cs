using UnityEngine;

//�ӵ��ű�  
public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;   //ͼ���ɰ棬�����޶���Ҫ��ײ����Ŀ��
    public float damage = 1;//�ӵ��˺� Ĭ��1
    public float lifeTime = 3;//�ӵ�����ʱ�� Ĭ��3s


    float speed = 10;//�ӵ����� �ò�������ǹе�У���ǹе��������
    //���˺��ӵ�������1֡�ƶ���ǡ�ö����ص� ���߼��ӵ����ڲ���ʼ ��ⲻ����ײ��
    float skinWidth = .1f;//Ƥ����� �����������


    //�����ӵ����ʷ���
    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime) ;//�����ӵ�����ʱ��������ӵ�

        //���ǵ��ӵ�����ײ���ڲ������
        //����Physics.OverlapSphere() ������������Ӵ���λ�������ڲ���������ײ�� ������
        //���� ��������λ�� ����뾶 ��ײ�ɰ�
        Collider[] initialCollisions=Physics.OverlapSphere(transform.position, .5f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0],transform.position);//���е�һ����⵽����ײ��,���е�Ϊ�ӵ�λ��
        }
    }

    void FixedUpdate()
    {
        float moveDistance=speed*Time.deltaTime;//�ӵ��ƶ�����
        CheckCollisions(moveDistance);//��ײ���
        transform.Translate(Vector3.forward*Time.deltaTime*speed);//�ӵ�ǰ��
    }

    //��ײ���
    void CheckCollisions(float moveDistance)
    {
        Ray ray=new Ray(transform.position, transform.forward);//���ӵ������ӵ�ǰ��������
        RaycastHit hit;//RaycastHit ���ڴ�������ײ�������л�ȡ������Ϣ
        //Physics.Raycast ������ײ��� ���з���true
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider,hit.point);
        }
    }

    //��������ִ�з���
    void OnHitObject(Collider c,Vector3 hitPoint)//��ȡ���еĸ��壬���е�λ��
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();//��ȡ���������IDamageable���
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage,hitPoint,transform.forward);//����TakeDamage()ʵ������˺�Ч��,�����˺�ֵ�����е㡢�ӵ�ǰ������
        }
        Destroy(gameObject);//�����ӵ�
    }
}
