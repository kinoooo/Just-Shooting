using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask;//�ɰ棬�������߼��
    public SpriteRenderer dot;//׼�ĵ���Ⱦ��
    public Color dotHighLightColor;//��������Ŀ��ʱ׼�ĸ�����ɫ
    Color oringinalDotColor;//׼��ԭʼ��ɫ

    private void Start()
    {
        Cursor.visible = false;
        oringinalDotColor=dot.color;
    }
    void Update()
    {
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);//׼����y����ת

    }

    public void DetectTargets(Ray ray)
    {
        if(Physics.Raycast(ray,100, targetMask))
        {
            dot.color = dotHighLightColor;
        }
        else
        {
            dot.color = oringinalDotColor;
        }
    }
}
