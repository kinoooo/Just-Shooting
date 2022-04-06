using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ǹе�ű�

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };//�������ģʽ ȫ�Զ� ���Զ� 
    public FireMode fireMode;//���ÿ���ģʽ

    public Transform[] projectileSpawn;//ǹ��λ��Transform
    public Projectile projectile;//�ӵ�����
    public float msBetweenShots = 100;//�ӵ�������ʱ�� ͨ���ò�����������
    public float muzzleSpeed = 35;//�ӵ�����
    public int burstCount;//һ�ο�����������ӵ���
    public int projectilesPerMag;//��ϻ�ӵ���
    public float reloadTime = .3f;//����ʱ��

    [Header("Recoil")]
    public Vector2 recoilValueMinMax = new Vector2(.05f, .2f);//������ˮƽλ����С-���ֵ
    public Vector2 recoilAngleMinMax = new Vector2(3, 5);//�������Ƕ���С-���ֵ
    public float recoilMoveSettleTime = .1f;//������ֵƽ��ʱ��
    public float recoilRotationSettleTime = .1f;//�������Ƕ�ƽ��ʱ��

    [Header("Effects")]
    public Transform shell;//����λ��Transform
    public Transform shellEjection;//���ǵ����Transform
    public AudioClip shootClip;//�����Ч
    public AudioClip reloadClip;//װ����Ч

    MuzzleFlash muzzleFlash;
    float nextShootTime;//��һ���ӵ������ʱ��

    bool triggerReleasedSinceLastShot;//�����ϴ�����Ƿ��ͷ�������
    bool isReloading;//�Ƿ����ڻ���ϻ
    int shotsRemainingInBurst;//һ�����������ʣ���ӵ���
    int projectilesRemainingInMag;//��ϻʣ���ӵ���

    float recoilAngle;//������ƫת�Ƕ�
    Vector3 recoilSmoothDampVelocity;//ƽ��������Ч��ʱ�����ٶȱ���
    float recoilRotSmoothDampVelocity;//ƽ���������Ƕ�ʱ�������

    private void Start()
    {
        muzzleFlash=GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    private void LateUpdate()//��ִ�У����⡰ָ��׼�ġ������˺������Ƕȱ仯
    {
        //������ƽ������
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        //�������Ƕ�ƽ��
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

            if (fireMode == FireMode.Burst)//������ģʽ
            {
                if (shotsRemainingInBurst == 0)//û�ӵ���
                {
                    return;
                }
                shotsRemainingInBurst--;//ʣ���ӵ���-1
            }
            else if (fireMode == FireMode.Single) {//����ģʽ
                if (!triggerReleasedSinceLastShot)//���û���ͷ����룬���޷����
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
                projectilesRemainingInMag --;//ʣ���ӵ�-1
                nextShootTime = Time.time + msBetweenShots / 1000;
                //�����ӵ�
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation);
                newProjectile.setSpeed(muzzleSpeed);
            }
            //���ɵ���
            Instantiate(shell, shellEjection.position, shellEjection.rotation);

            //��ǹ��
            muzzleFlash.Activate();

            //����������
            //transform.localPosition -= Vector3.forward * Random.Range(recoilValueMinMax.x,recoilValueMinMax.y);
            //recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            transform.localPosition -= Vector3.forward * Random.Range(recoilValueMinMax.x, recoilValueMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            //���������Ч
            AudioManager.instance.PlaySound(shootClip, transform.position);
        }
    }

    //����
    public void Reload()
    {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadClip, transform.position);//����װ����Ч
        }
    }

    //װ������
    IEnumerator AnimateReload()
    {
        isReloading = true;
        //yield return new WaitForSeconds(.2f);//��bug

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;//����Ƕ�
        float maxReloadAngle = 30;//�����ת�Ƕ�
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

    public void Aim(Vector3 aimPoint)//ʹǹ��׼׼��
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
