using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//怪物生成器
public class Spawner : MonoBehaviour
{
    public bool devMode;

    public Wave[] waves;//存储波次信息
    public Enemy enemy;//生成的敌人

    LivingEntity playerEntity;//玩家实体
    Transform playerT;//玩家位置

    Wave currentWave;//当前波
    int currentWaveNumber;//当前波数

    int enemiesRemainingToSpawn;//剩余未生成敌人数量
    int enemiesRemainingAlive;//存活敌人数量
    float nextSpawnTime;//标记下次生成时间

    MapGenerator map;

    float timeBetweenCampingChecks = 2f;//检测玩家停留时间的间隔 默认为4秒
    float nextCampCheckTime;//下一次检测玩家停留的时间
    float campThresholdDistance = 1.5f;//判定挂机的移动距离阈值  每两次检查之间如果移动距离不超过此阈值，则判定挂机
    Vector3 campPositionOld;//记录玩家上次判定时位置
    bool isCamping;//是否挂机
    bool isDisabled;//玩家是否死亡

    //提升刷怪频率
    float nextLevelUpTime;
    float timeBetweenLevelUp = 5f;//每次提升间隔 *30满
    float nextLevelPercent = .95f;

    public event System.Action<int> OnNewWave;//事件对象 接收

    //波次类
    [System.Serializable]
    public class Wave
    {
        public bool infinite;//敌人是否无限
        public int enemyCount;//敌人数量
        public float timeBetweenSpawns;//每两次生成之间的间隔时间

        public float moveSpeed;//敌人移动速度
        public int hitsToKillPlayer;//敌人攻击力
        public float enemyHealth;//敌人生命
        public Color skinColor;//敌人颜色
    }

    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        playerEntity.OnDeath += OnPlayerDeath;//如果玩家死亡则执行

        nextCampCheckTime = Time.time + timeBetweenCampingChecks;
        campPositionOld = playerT.position;

        map = FindObjectOfType<MapGenerator>();
        NextWave();//开始第一波



}

    private void Update()
    {
        if (!isDisabled)
        {
            //检测玩家是否挂机
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime += timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }

            //检测是否还有未生成的怪物以及是否到达刷怪时间
            if ((enemiesRemainingToSpawn > 0|| currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;//未生成敌人数量-1
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;//设定下一次生成生物的时间
                StartCoroutine(nameof(SpawnEnemy));
            }

            //跳关
            if (devMode)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    StopCoroutine(nameof(SpawnEnemy));
                    foreach(Enemy enemy in FindObjectsOfType<Enemy>())//清除场上怪物
                    {
                        Destroy(enemy.gameObject);
                    }
                    NextWave();
                }
            }

            //如果是无限波次，则不断提升刷怪频率
            if (currentWave.infinite && currentWave.timeBetweenSpawns > .1f)
            {
                if (Time.time > nextLevelUpTime)
                {
                    currentWave.timeBetweenSpawns *= .95f;
                    nextLevelUpTime = Time.time + timeBetweenLevelUp;
                    //Debug.Log("刷怪间隔：" + currentWave.timeBetweenSpawns);
                }
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;//生成生物前延迟时间 默认1秒
        float tileFlashSpeed = 4;//每秒闪烁次数 默认4次

        Transform spawnTile=map.GetRandomOpenTile();//得到随机可到达区域的tile的transform

        //如果玩家挂机 将在玩家脚下刷怪
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }

        //瓷砖闪烁
        Material tileMat=spawnTile.GetComponent<Renderer>().material;//得到该tile的material
        //Color initialColor=Color.white;//记录最初颜色
        Color flashColor = Color.red;//闪烁颜色 设定为红色
        float spawnTimer = 0f;
        while(spawnTimer < spawnDelay)
        {
            //瓷砖颜色随时间变化 在白色到闪烁颜色之间往返
            tileMat.color = Color.Lerp(Color.white, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer+=Time.deltaTime;
            yield return null;//下一帧继续运行
        }

        //生成怪物
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;//敌人死亡时执行OnEnemyDeath

        spawnedEnemy.setCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    //敌人死亡时执行
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;//当前存活敌人数量-1
        //Debug.Log("Enemy died");
        if (enemiesRemainingAlive == 0)//当前存活敌人数量=0 开始下一波
        {
            NextWave();
        }
    }    
    //玩家死亡时执行
    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    //重置玩家位置
    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;//置于地图中间 空中3米
    }

    //开始下一波
    void NextWave()
    {
        if (currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");//2D形式播放过关音效
        }
        currentWaveNumber ++;//当前波次+1
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
            if (currentWave.infinite)
            {
                nextLevelUpTime=Time.time + timeBetweenLevelUp;
            }
            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);//对当前波次事件传入波次数，通知MapGenerator生成下一个地图
            }
            ResetPlayerPosition();
        }
    }


}
