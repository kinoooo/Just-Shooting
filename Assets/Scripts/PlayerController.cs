using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家控制器

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //为使物体移动受物理引擎影响，要添加刚体组件
    Rigidbody myRigidbody;

    //接收Player传入的速度向量
    Vector3 velocity;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    //接收速度向量
    public void Move(Vector3 moveVelocity)
    {
        velocity = moveVelocity;
    }

    public void lookAt(Vector3 lookPoint)
    {
        Vector3 correctPoint=new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(correctPoint);
    }


    //固定时间更新，避免因帧率不稳定产生抖动
    private void FixedUpdate()
    {
        //通过刚体API MovePosition实现基于物理引擎的运动
        //参数为目标位置 这里是 原位置+速度*经过时间
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }
}
