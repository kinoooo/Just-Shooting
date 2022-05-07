using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    public Text scoreUI;
    public Text killCount;
    public Text gameOverScoreUI;
    public Text gameOverKillCountUI;
    public Text ProjectilesUI;
    public RectTransform healthBar;
    Player player;

    Spawner spawner;
    private void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }
    private void Awake()
    {
        spawner =FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    private void Update()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D8");
        killCount.text = ScoreKeeper.killCount.ToString("D4");
        
        if (FindObjectOfType<Player>() != null)
        {
            Gun currentGun = FindObjectOfType<Player>().GetComponent<GunController>().equippedGun;//��ȡ��ǰװ��������
            ProjectilesUI.text = currentGun.projectilesRemainingInMag / currentGun.projectileSpawn.Length//��ȡ��ǰ��ϻʣ���ӵ����͵�ǰ������ϻ����
                + " / " + currentGun.projectilesPerMag / currentGun.projectileSpawn.Length;
        }
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;//����ֵ�ٷֱ�
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
    } 


    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, new Color(0,0,0,.95f), 1));//����Э�̣���͸�����ڣ�1��
        gameOverScoreUI.text = scoreUI.text;
        gameOverKillCountUI.text= killCount.text;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
        Cursor.visible = true;
    }
    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
        newWaveEnemyCount.text = ((spawner.waves[waveNumber - 1].infinite))? "Infinite": "Enemies: " + spawner.waves[waveNumber - 1].enemyCount;
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    //�²��ζ���
    IEnumerator AnimateNewWaveBanner()
    {
        float delay = 1f;//ͣ��ʱ��
        float speed = 3f;//�����ٶ�
        float animatePercent = 0;//��������
        float endDelayTime = Time.time + 1 / speed + delay;//��ǰʱ��+����������ʱ��+ͣ��ʱ��
        int dir = 1;//���� �����Ϻ�����

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if(Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-350, -45, animatePercent);
            yield return null;
        }
    }

    //������UI�������
    IEnumerator Fade(Color from,Color to,float time)//��from��to����ʱtime
    {
        float speed = 1f / time;
        float percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;//ÿִ֡��һ��
        }
    }


    //UI����
    public void StartNewGame()
    {
        //Application.LoadLevel("Game");
        SceneManager.LoadScene("Game");
    }

    //�������˵�
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
