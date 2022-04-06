using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//枪械控制器

public class GunController : MonoBehaviour
{
    public Transform weaponHold;    //实例化枪的位置
    public Gun[] allGuns;     //武器列表
    Gun equippedGun;    //装备的武器


    private void Start()
    {
    }
    public void EquipGun(Gun gunToEquip)
    {
        //检查是否有已装备的武器
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        //在weaponHold位置实例化装备的枪 并绑定weaponHold的位置 
        equippedGun = Instantiate(gunToEquip,weaponHold.position,weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;
    }
    public void EquipGun(int weaponIndex)
    {
        EquipGun(allGuns[weaponIndex]);
    }

    //执行射击命令 调用Gun中的OnTriggerHold()、OnTriggerRelease()
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

    //获取武器高度
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
