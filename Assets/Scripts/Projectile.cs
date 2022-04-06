using UnityEngine;

//子弹脚本  
public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;   //图层蒙版，用于限定需要碰撞检测的目标
    public float damage = 1;//子弹伤害 默认1
    public float lifeTime = 3;//子弹存在时间 默认3s


    float speed = 10;//子弹速率 该参数传入枪械中，由枪械决定速率
    //敌人和子弹可能在1帧移动后恰好二者重叠 射线检测从敌人内部开始 检测不到碰撞体
    float skinWidth = .1f;//皮肤宽度 补偿这种情况


    //设置子弹速率方法
    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime) ;//到达子弹存在时间后消除子弹

        //考虑到子弹在碰撞体内部的情况
        //调用Physics.OverlapSphere() 检测包含与球体接触或位于球体内部的所有碰撞体 并返回
        //参数 球体球心位置 球体半径 碰撞蒙版
        Collider[] initialCollisions=Physics.OverlapSphere(transform.position, .5f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0],transform.position);//命中第一个检测到的碰撞体,命中点为子弹位置
        }
    }

    void FixedUpdate()
    {
        float moveDistance=speed*Time.deltaTime;//子弹移动距离
        CheckCollisions(moveDistance);//碰撞检测
        transform.Translate(Vector3.forward*Time.deltaTime*speed);//子弹前进
    }

    //碰撞检测
    void CheckCollisions(float moveDistance)
    {
        Ray ray=new Ray(transform.position, transform.forward);//从子弹射向子弹前方的射线
        RaycastHit hit;//RaycastHit 用于从射线碰撞检测过程中获取物体信息
        //Physics.Raycast 射线碰撞检测 命中返回true
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider,hit.point);
        }
    }

    //命中物体执行方法
    void OnHitObject(Collider c,Vector3 hitPoint)//获取命中的刚体，命中的位置
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();//获取命中物体的IDamageable组件
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage,hitPoint,transform.forward);//调用TakeDamage()实现造成伤害效果,传入伤害值、命中点、子弹前进方向
        }
        Destroy(gameObject);//消除子弹
    }
}
