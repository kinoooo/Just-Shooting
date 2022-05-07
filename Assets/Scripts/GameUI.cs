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
            Gun currentGun = FindObjectOfType<Player>().GetComponent<GunController>().equippedGun;//获取当前装备的武器
            ProjectilesUI.text = currentGun.projectilesRemainingInMag / currentGun.projectileSpawn.Length//获取当前弹匣剩余子弹数和当前武器弹匣容量
                + " / " + currentGun.projectilesPerMag / currentGun.projectileSpawn.Length;
        }
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;//生命值百分比
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
    } 


    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, new Color(0,0,0,.95f), 1));//启动协程，从透明到黑，1秒
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

    //新波次动画
    IEnumerator AnimateNewWaveBanner()
    {
        float delay = 1f;//停留时间
        float speed = 3f;//上升速度
        float animatePercent = 0;//动画进度
        float endDelayTime = Time.time + 1 / speed + delay;//当前时间+动画上升的时间+停留时间
        int dir = 1;//方向 先向上后向下

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

    //死亡后UI渐变出现
    IEnumerator Fade(Color from,Color to,float time)//从from到to，用时time
    {
        float speed = 1f / time;
        float percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;//每帧执行一次
        }
    }


    //UI输入
    public void StartNewGame()
    {
        //Application.LoadLevel("Game");
        SceneManager.LoadScene("Game");
    }

    //返回主菜单
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
