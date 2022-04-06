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
        float force=Random.Range(forceMin,forceMax);//����ֵ�����Χ
        myRigidbody.AddForce(transform.right * force);//ʩ�����ҵ���
        myRigidbody.AddTorque(Random.insideUnitSphere * force);//���������Ť��

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime); //�ȴ���������ʱ�� �ٿ�ʼ��ʧ
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
