using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ǹе������

public class GunController : MonoBehaviour
{
    public Transform weaponHold;    //ʵ����ǹ��λ��
    public Gun[] allGuns;     //�����б�
    Gun equippedGun;    //װ��������


    private void Start()
    {
    }
    public void EquipGun(Gun gunToEquip)
    {
        //����Ƿ�����װ��������
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        //��weaponHoldλ��ʵ����װ����ǹ ����weaponHold��λ�� 
        equippedGun = Instantiate(gunToEquip,weaponHold.position,weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;
    }
    public void EquipGun(int weaponIndex)
    {
        EquipGun(allGuns[weaponIndex]);
    }

    //ִ��������� ����Gun�е�OnTriggerHold()��OnTriggerRelease()
    public void OnTriggerHold()
    {
        if(equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    //��ȡ�����߶�
    public float GunHeight
    {
        get{
            return weaponHold.position.y;
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }
}
