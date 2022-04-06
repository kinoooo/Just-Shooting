using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//枪械脚本

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };//定义射击模式 全自动 半自动 
    public FireMode fireMode;//设置开火模式

    public Transform[] projectileSpawn;//枪口位置Transform
    public Projectile projectile;//子弹类型
    public float msBetweenShots = 100;//子弹射出间隔时间 通过该参数控制射速
    public float muzzleSpeed = 35;//子弹速率
    public int burstCount;//一次开火最大连射子弹数
    public int projectilesPerMag;//弹匣子弹数
    public float reloadTime = .3f;//换弹时间

    [Header("Recoil")]
    public Vector2 recoilValueMinMax = new Vector2(.05f, .2f);//后坐力水平位移最小-最大值
    public Vector2 recoilAngleMinMax = new Vector2(3, 5);//后坐力角度最小-最大值
    public float recoilMoveSettleTime = .1f;//后坐力值平复时间
    public float recoilRotationSettleTime = .1f;//后坐力角度平复时间

    [Header("Effects")]
    public Transform shell;//弹壳位置Transform
    public Transform shellEjection;//弹壳弹射点Transform
    public AudioClip shootClip;//射击音效
    public AudioClip reloadClip;//装弹音效

    MuzzleFlash muzzleFlash;
    float nextShootTime;//下一次子弹射出的时间

    bool triggerReleasedSinceLastShot;//距离上次射击是否释放了输入
    bool isReloading;//是否正在换弹匣
    int shotsRemainingInBurst;//一次连续射击的剩余子弹数
    int projectilesRemainingInMag;//弹匣剩余子弹数

    float recoilAngle;//后坐力偏转角度
    Vector3 recoilSmoothDampVelocity;//平复后坐力效果时所需速度变量
    float recoilRotSmoothDampVelocity;//平复后坐力角度时所需变量

    private void Start()
    {
        muzzleFlash=GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    private void LateUpdate()//后执行，避免“指向准心”抵消了后坐力角度变化
    {
        //后坐力平复动画
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        //后坐力角度平复
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }
    void shoot()
    {
        if (!isReloading && Time.time > nextShootTime && projectilesRemainingInMag > 0) {

            if (fireMode == FireMode.Burst)//多连发模式
            {
                if (shotsRemainingInBurst == 0)//没子弹了
                {
                    return;
                }
                shotsRemainingInBurst--;//剩余子弹数-1
            }
            else if (fireMode == FireMode.Single) {//点射模式
                if (!triggerReleasedSinceLastShot)//如果没有释放输入，则无法射击
                {
                    return;
                }
            }
            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if (projectilesRemainingInMag == 0)
                {
                    break;
                }
                projectilesRemainingInMag --;//剩余子弹-1
                nextShootTime = Time.time + msBetweenShots / 1000;
                //生成子弹
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation);
                newProjectile.setSpeed(muzzleSpeed);
            }
            //生成弹壳
            Instantiate(shell, shellEjection.position, shellEjection.rotation);

            //打开枪焰
            muzzleFlash.Activate();

            //产生后坐力
            //transform.localPosition -= Vector3.forward * Random.Range(recoilValueMinMax.x,recoilValueMinMax.y);
            //recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            transform.localPosition -= Vector3.forward * Random.Range(recoilValueMinMax.x, recoilValueMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            //播放射击音效
            AudioManager.instance.PlaySound(shootClip, transform.position);
        }
    }

    //换弹
    public void Reload()
    {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadClip, transform.position);//播放装弹音效
        }
    }

    //装弹动画
    IEnumerator AnimateReload()
    {
        isReloading = true;
        //yield return new WaitForSeconds(.2f);//有bug

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;//最初角度
        float maxReloadAngle = 30;//最大旋转角度
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void Aim(Vector3 aimPoint)//使枪瞄准准心
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
        }
    }

    public void OnTriggerHold()
    {
        shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot=true;
        shotsRemainingInBurst = burstCount;
    }
}
