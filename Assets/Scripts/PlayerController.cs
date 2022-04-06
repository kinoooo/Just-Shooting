using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ҿ�����

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //Ϊʹ�����ƶ�����������Ӱ�죬Ҫ��Ӹ������
    Rigidbody myRigidbody;

    //����Player������ٶ�����
    Vector3 velocity;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    //�����ٶ�����
    public void Move(Vector3 moveVelocity)
    {
        velocity = moveVelocity;
    }

    public void lookAt(Vector3 lookPoint)
    {
        Vector3 correctPoint=new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(correctPoint);
    }


    //�̶�ʱ����£�������֡�ʲ��ȶ���������
    private void FixedUpdate()
    {
        //ͨ������API MovePositionʵ�ֻ�������������˶�
        //����ΪĿ��λ�� ������ ԭλ��+�ٶ�*����ʱ��
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }
}
