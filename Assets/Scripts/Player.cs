using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ҽű�

//�������������Ϊ������ �������ô��󣬱����ظ�������
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    Camera viewCamera;

    PlayerController controller;
    GunController gunController;

    //��ɫ�ƶ�����
    public float moveSpeed = 10f;

    public Crosshairs crosshairs;

    //����ִ��
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
        health = startingHealth;//��������ֵ
        gunController.EquipGun(waveNumber - 1);//��ǹ
    }

    //��������
    void Update()
    {
        //------------�ƶ���Ϣ����
        //���������������������豸��ˮƽ����
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        //�����ٶ����� ȡ�����������򣨼���׼��Ϊ��λ��������������
        Vector3 moveVelocity=moveInput.normalized*moveSpeed;

        //��PlayerController�������ٶ�����
        controller.Move(moveVelocity);

        //------------�泯��������

        Ray ray=viewCamera.ScreenPointToRay(Input.mousePosition);//����  

        //����ƽ�� Plane(Vector3.up,Vector3.up * gunController.GunHeight) ƽ�洩����(0,1,0)*�����߶ȣ�����Ϊ(0,1,0)
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        //���߼�� ʹ������ƽ���ཻ
        //���ҽ� ������ƽ���ཻ�� �� ����Դ�� �ľ��븳ֵ��rayDistance
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            //���ؾ�������rayDistance�ĵ�
            Vector3 aimPoint = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.lookAt(aimPoint);

            crosshairs.transform.position = aimPoint;//������λ�ø���ʮ��׼��
            crosshairs.DetectTargets(ray);
            if((new Vector2(aimPoint.x, aimPoint.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1){
                gunController.Aim(aimPoint); 
            }

        }

        //-------------��������
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

        //-------------ǹ֧�л�����
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

        //-------------�뿪��ͼ����
        if (transform.position.y < -20)
        {
            TakeDamage(health);
        }
    }

    //��д����ִ��
    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);//�������������Ч
        base.Die();
    }
}
