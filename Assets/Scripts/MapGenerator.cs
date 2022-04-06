using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//地图生成器

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;//存放地图
    public int mapIndex;//地图索引
    public Transform tilePrefab;//tile瓷砖预制件
    public Transform obstaclePrefab;//obstacle障碍物预制件
    public Transform mapFloor;//地图地板
    public Transform navmeshFloor;//导航网格地板大小
    public Transform navmeshMaskPrefab;//导航遮罩预制件
    public Vector2 maxMapSize;//最大地图大小-导航区域大小
    public float tileSize;//tile瓷砖缩放比例

    [Range(0f, 1f)]
    public float outLinePercent;//瓷砖边缘缩小比例

    List<Coord> allTileCoords;//List存储tile瓷砖坐标
    Queue<Coord> shuffledTileCoords;//Queue存储打乱的瓷砖坐标 用于随机生成障碍物
    Queue<Coord> shuffledOpenTileCoords;//Queue存储可以到达的瓷砖坐标 用于随机生成生物
    Transform[,] tileMap;//存储所有瓷砖的Transform

    Map currentMap;//当前地图
    private void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }
    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;//当前地图数组下标和波数-1对齐
        GenerateMap();//生成地图
    }
    


    //生成地图
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];//设置当前地图
        tileMap=new Transform[currentMap.mapSize.x,currentMap.mapSize.y];//tileMap大小与地图相同
        System.Random prng = new System.Random(currentMap.seed);//随机数生成器 导入地图种子
        //生成Coords
        allTileCoords = new List<Coord>();//allTileCoords用于存放所有tile的坐标Coord
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));//存放坐标到allTileCoords中
            }
        }

        //打乱地图坐标，用于生成障碍物   通过Utility.ShuffleArray()打乱allTileCoords后存入shuffledTileCoords
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray (allTileCoords.ToArray(), currentMap.seed));

        //生成mapHolder对象
        string holderName = "Generated Map";//mapHolder的名字
        if (transform.Find(holderName))//如果存在Generated Map则销毁
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder =new GameObject(holderName).transform;//实例化一个mapHolder 存放tile
        mapHolder.parent = transform;
        
        //生成tiles瓷砖
        for(int x = 0; x < currentMap.mapSize.x; x++)
        {
            for( int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);//瓷砖位置 
                //在tilePosition位置生成tilePrefab 并绕Vector3.right旋转90度
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale=Vector3.one*(1-outLinePercent)*tileSize;//瓷砖根据outLinePercent值缩小
                newTile.parent = mapHolder;//父对象为mapHolder
                tileMap[x,y]=newTile;//生成的瓷砖的Transform存入tileMap
            }
        }

        //生成障碍物
        int currentObstacleCount = 0;//当前障碍物数量 初始置0
        bool[,] obstacleMap=new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];//boll型二维数组 存障碍物位置图
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); //障碍物数量=全区块数*障碍物百分比
        List<Coord> allOpenCoords=new List<Coord>(allTileCoords);//设置List存储所有可到达瓷砖坐标
        for ( int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();//得到一个随机的坐标
            obstacleMap[randomCoord.x, randomCoord.y] = true;//在障碍物位置图中置为true，表示该坐标已有障碍物
            currentObstacleCount++;//当前障碍物数量+1
            //判断能否在该坐标生成障碍物
            //该坐标不能为地图中心 且 确保该坐标置障碍物后也可以到达地图所有空位
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                //设置障碍物高度 在当前地图的最小障碍物高度到最大障碍物高度之间随机
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);//障碍物位置由随机的Coord转换而来

                //设置障碍物位置并生成
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity);
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outLinePercent) * tileSize, obstacleHeight, (1 - outLinePercent) * tileSize);//瓷砖根据outLinePercent值缩小

                //设置障碍物材质
                Renderer obstacleRenderer=newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;//插值
                obstacleMaterial.color=Color.Lerp(currentMap.foregroundColor,currentMap.backgroundColor,colorPercent);//在前景色到后景色之间根据坐标位置插值
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);//障碍物生成，则该位置不可达
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;//在障碍物位置图中置为false，表示该坐标没有障碍物
                currentObstacleCount--;//障碍物数量减1
            }
        }

        //打乱可到达区域坐标，用于生成生物
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        //生成导航遮罩  避免导航至地图外
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) ;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        //变换navmeshFloor以适应变化的地图大小  navmeshFloor沿x旋转了90度，所以世界坐标的z轴现在相对于它来说是y轴
        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;

        //地图地板适应地图大小
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
    }

    //确保该坐标生成障碍物后也可以到达地图所有空位  利用洪水算法 FloodFill 
    //传入障碍物地图和当前障碍物数量
    bool MapIsFullyAccessible(bool[,]obstacleMap,int currentObstacleCount)
    {
        bool[,] mapFLags=new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];//mapFlags 记录区域是否检测过
        Queue<Coord> queue = new Queue<Coord>();//队列queue存放坐标
        queue.Enqueue(currentMap.mapCenter); //地图中心入队，从地图中心开始向周围检测
        mapFLags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;//mapFlags中置地图中心可以到达

        int accessibleTileCount = 1;

        //开始遍历地图 记录所有可以到达的区域
        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();//出队
            //检测该目标点及周围8个点 九宫格
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)//不检测斜角 只检测上下左右及自身 5个格子 而自身已标记 实际上检测4个格子
                    {
                        //判断x和y坐标是否在障碍物位置图范围内
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            //该坐标未被标记 且 该位置没有障碍物 则执行代码块
                            if (!mapFLags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFLags[neighbourX, neighbourY] = true;//标记该位置
                                queue.Enqueue(new Coord(neighbourX, neighbourY));//该位置入队，作为下一个检测点进行下一次循环
                                accessibleTileCount++;//可到达的区域数+1
                            }
                        }
                    }
                }
            }
        }
        //如果 目标可到达的区域的数量!=当前可以到达的区域数量 ,说明存在无法到达的区域，则该障碍物不可生成
        //目标可到达的区域的数量 =地图所有区域数量-当前障碍物数量
        int targetAccessTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        //比较目标可到达的区域数量和当前可以到达的区域数量是否相同
        return targetAccessTileCount == accessibleTileCount;
    }

    //坐标转换为三维向量
    Vector3 CoordToPosition(int x,int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + x + .5f, 0, -currentMap.mapSize.y / 2f + y + .5f) * tileSize;
    }
    
    //通过一个三维坐标获取最近瓷砖的Transform
    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x=Mathf.Clamp(x, 0, tileMap.GetLength(0)-1);//将x值钳制在0到tileMap的x轴最大值的范围内，防止数组越界
        y=Mathf.Clamp(y, 0, tileMap.GetLength(1)-1);//将y值钳制在0到tileMap的y轴最大值的范围内，防止数组越界
        return tileMap[x,y];
    }

    //从已打乱的地图坐标队列中取出一个tile坐标，返回该坐标
    public Coord GetRandomCoord() 
    {
        Coord randomCoord=shuffledTileCoords.Dequeue();//坐标出队
        shuffledTileCoords.Enqueue(randomCoord);//取出的坐标再放回队尾
        return randomCoord; 
    }

    //从已打乱的可到达区域队列中取出一个坐标，返回由该坐标指定位置的tile的transform，这会用于Spawner生物生成器中
    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();//坐标出队
        shuffledOpenTileCoords.Enqueue(randomCoord);//取出的坐标再放回队尾
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    //二维坐标结构体，存储tile的坐标
    public struct Coord
    {
        public int x;
        public int y;
        public Coord(int _x, int _y){
            x = _x;
            y = _y;
        }
        public static bool operator ==(Coord a, Coord b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Coord a, Coord b)
        {
            return !(a==b);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0f,1f)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }

}
