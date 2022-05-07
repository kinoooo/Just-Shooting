using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; private set; }//得分 对外部类只读
    public static int killCount { get; private set; } = 0;//击杀数 对外部类只读
    float lastEnemyKillTime;//最后击杀时间
    int streakCount;//连杀数
    float streakExpiryTime = 1.5f;//连杀时间 默认1s
    private void Start()
    {
        score = 0;
        killCount = 0;
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;//如果玩家死亡，则取消订阅OnEnemyKilled(),防止重复订阅
    }
    void OnEnemyKilled()//敌人死亡时执行
    {
        if (Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;//连杀清零
        }
        killCount++;
        lastEnemyKillTime = Time.time;
        score += 5 + (int)Mathf.Pow(streakCount, 2);
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
    
}
