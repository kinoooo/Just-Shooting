using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; private set; }//���ⲿ��ֻ��
    float lastEnemyKillTime;//����ɱʱ��
    int streakCount;//��ɱ��
    float streakExpiryTime = 1.5f;//��ɱʱ�� Ĭ��1s
    private void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;//��������������ȡ������OnEnemyKilled(),��ֹ�ظ�����
    }
    void OnEnemyKilled()
    {
        if (Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;//��ɱ����
        }
        lastEnemyKillTime = Time.time;
        score += 5 + (int)Mathf.Pow(streakCount, 2);
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
    
}
