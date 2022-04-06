using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家脚本

//将所需的组件添加为依赖项 避免设置错误，比如重复添加组件
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    Camera viewCamera;

    PlayerController controller;
    GunController gunController;

    //角色移动速率
    public float moveSpeed = 10f;

    public Crosshairs crosshairs;

    //启动执行
    protected override void Start()
    {
        base.Start();
    }
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;//重置生命值
        gunController.EquipGun(waveNumber - 1);//切枪
    }

    //持续更新
    void Update()
    {
        //------------移动信息输入
        //定义输入向量接收输入设备的水平输入
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        //定义速度向量 取输入向量方向（即标准化为单位向量）乘以速率
        Vector3 moveVelocity=moveInput.normalized*moveSpeed;

        //向PlayerController对象传入速度向量
        controller.Move(moveVelocity);

        //------------面朝方向输入

        Ray ray=viewCamera.ScreenPointToRay(Input.mousePosition);//射线  

        //构造平面 Plane(Vector3.up,Vector3.up * gunController.GunHeight) 平面穿过点(0,1,0)*武器高度，法线为(0,1,0)
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        //射线检测 使射线与平面相交
        //并且将 射线与平面相交处 到 射线源点 的距离赋值给rayDistance
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            //返回距离射线rayDistance的点
            Vector3 aimPoint = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.lookAt(aimPoint);

            crosshairs.transform.position = aimPoint;//将射线位置赋给十字准心
            crosshairs.DetectTargets(ray);
            if((new Vector2(aimPoint.x, aimPoint.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1){
                gunController.Aim(aimPoint); 
            }

        }

        //-------------武器输入
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R)){
            gunController.Reload();
        }

        //-------------枪支切换输入
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            gunController.EquipGun(0);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            gunController.EquipGun(1);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            gunController.EquipGun(2);
        }

        //-------------离开地图死亡
        if (transform.position.y < -20)
        {
            TakeDamage(health);
        }
    }

    //重写死亡执行
    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);//播放玩家死亡音效
        base.Die();
    }
}
