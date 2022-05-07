using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//����������
public class Spawner : MonoBehaviour
{
    public bool devMode;

    public Wave[] waves;//�洢������Ϣ
    public Enemy enemy;//���ɵĵ���

    LivingEntity playerEntity;//���ʵ��
    Transform playerT;//���λ��

    Wave currentWave;//��ǰ��
    int currentWaveNumber;//��ǰ����

    int enemiesRemainingToSpawn;//ʣ��δ���ɵ�������
    int enemiesRemainingAlive;//����������
    float nextSpawnTime;//����´�����ʱ��

    MapGenerator map;

    float timeBetweenCampingChecks = 2f;//������ͣ��ʱ��ļ�� Ĭ��Ϊ4��
    float nextCampCheckTime;//��һ�μ�����ͣ����ʱ��
    float campThresholdDistance = 1.5f;//�ж��һ����ƶ�������ֵ  ÿ���μ��֮������ƶ����벻��������ֵ�����ж��һ�
    Vector3 campPositionOld;//��¼����ϴ��ж�ʱλ��
    bool isCamping;//�Ƿ�һ�
    bool isDisabled;//����Ƿ�����

    //����ˢ��Ƶ��
    float nextLevelUpTime;
    float timeBetweenLevelUp = 5f;//ÿ��������� *30��
    float nextLevelPercent = .95f;

    public event System.Action<int> OnNewWave;//�¼����� ����

    //������
    [System.Serializable]
    public class Wave
    {
        public bool infinite;//�����Ƿ�����
        public int enemyCount;//��������
        public float timeBetweenSpawns;//ÿ��������֮��ļ��ʱ��

        public float moveSpeed;//�����ƶ��ٶ�
        public int hitsToKillPlayer;//���˹�����
        public float enemyHealth;//��������
        public Color skinColor;//������ɫ
    }

    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        playerEntity.OnDeath += OnPlayerDeath;//������������ִ��

        nextCampCheckTime = Time.time + timeBetweenCampingChecks;
        campPositionOld = playerT.position;

        map = FindObjectOfType<MapGenerator>();
        NextWave();//��ʼ��һ��



}

    private void Update()
    {
        if (!isDisabled)
        {
            //�������Ƿ�һ�
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime += timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }

            //����Ƿ���δ���ɵĹ����Լ��Ƿ񵽴�ˢ��ʱ��
            if ((enemiesRemainingToSpawn > 0|| currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;//δ���ɵ�������-1
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;//�趨��һ�����������ʱ��
                StartCoroutine(nameof(SpawnEnemy));
            }

            //����
            if (devMode)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    StopCoroutine(nameof(SpawnEnemy));
                    foreach(Enemy enemy in FindObjectsOfType<Enemy>())//������Ϲ���
                    {
                        Destroy(enemy.gameObject);
                    }
                    NextWave();
                }
            }

            //��������޲��Σ��򲻶�����ˢ��Ƶ��
            if (currentWave.infinite && currentWave.timeBetweenSpawns > .1f)
            {
                if (Time.time > nextLevelUpTime)
                {
                    currentWave.timeBetweenSpawns *= .95f;
                    nextLevelUpTime = Time.time + timeBetweenLevelUp;
                    //Debug.Log("ˢ�ּ����" + currentWave.timeBetweenSpawns);
                }
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;//��������ǰ�ӳ�ʱ�� Ĭ��1��
        float tileFlashSpeed = 4;//ÿ����˸���� Ĭ��4��

        Transform spawnTile=map.GetRandomOpenTile();//�õ�����ɵ��������tile��transform

        //�����ҹһ� ������ҽ���ˢ��
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }

        //��ש��˸
        Material tileMat=spawnTile.GetComponent<Renderer>().material;//�õ���tile��material
        //Color initialColor=Color.white;//��¼�����ɫ
        Color flashColor = Color.red;//��˸��ɫ �趨Ϊ��ɫ
        float spawnTimer = 0f;
        while(spawnTimer < spawnDelay)
        {
            //��ש��ɫ��ʱ��仯 �ڰ�ɫ����˸��ɫ֮������
            tileMat.color = Color.Lerp(Color.white, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer+=Time.deltaTime;
            yield return null;//��һ֡��������
        }

        //���ɹ���
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;//��������ʱִ��OnEnemyDeath

        spawnedEnemy.setCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    //��������ʱִ��
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;//��ǰ����������-1
        //Debug.Log("Enemy died");
        if (enemiesRemainingAlive == 0)//��ǰ����������=0 ��ʼ��һ��
        {
            NextWave();
        }
    }    
    //�������ʱִ��
    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    //�������λ��
    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;//���ڵ�ͼ�м� ����3��
    }

    //��ʼ��һ��
    void NextWave()
    {
        if (currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");//2D��ʽ���Ź�����Ч
        }
        currentWaveNumber ++;//��ǰ����+1
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
                OnNewWave(currentWaveNumber);//�Ե�ǰ�����¼����벨������֪ͨMapGenerator������һ����ͼ
            }
            ResetPlayerPosition();
        }
    }


}
