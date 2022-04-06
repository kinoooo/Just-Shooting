using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject flashHolder;
    public Sprite[] flashSprites;
    public SpriteRenderer[] spriteRenderers;


    public float flashTime;//枪焰持续时间
    private void Awake()
    {
        Deactivate();
    }
    public void Activate()
    {
        flashHolder.SetActive(true);

        int flashSpriteIndex=Random.Range(0, flashSprites.Length);
        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
        }

        Invoke("Deactivate", flashTime);//flashTime时间后执行Deactivate()以停用flashHolder
    }

    void Deactivate()
    {
        flashHolder.SetActive(false);
    }
}
