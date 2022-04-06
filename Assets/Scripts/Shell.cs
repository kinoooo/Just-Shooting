using System.Collections;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;

    float lifeTime = 4;
    float fadeTime = 2;
    private void Awake()
    {
        float force=Random.Range(forceMin,forceMax);//力的值随机范围
        myRigidbody.AddForce(transform.right * force);//施加向右的力
        myRigidbody.AddTorque(Random.insideUnitSphere * force);//加随机方向扭矩

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime); //等待到达生命时间 再开始消失
        float percent = 0;
        float fadeSpeed = 1 / fadeTime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;
        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color=Color.Lerp(initialColor,Color.clear,fadeSpeed);
            yield return null;
        }
        Destroy(gameObject);
    }
}
