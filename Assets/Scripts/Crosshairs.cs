using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask;//蒙版，用于射线检测
    public SpriteRenderer dot;//准心的渲染器
    public Color dotHighLightColor;//射线命中目标时准心高亮颜色
    Color oringinalDotColor;//准心原始颜色

    private void Start()
    {
        Cursor.visible = false;
        oringinalDotColor=dot.color;
    }
    void Update()
    {
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);//准心绕y轴旋转

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
