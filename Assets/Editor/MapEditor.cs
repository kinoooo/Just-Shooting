using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//地图编辑器
[CustomEditor(typeof(MapGenerator))]
public class NewBehaviourScript : Editor
{
    //重写检视面板
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI(); 持续更新 性能开销太大

        //设置所检查的对象
        MapGenerator map = target as MapGenerator;

        if (DrawDefaultInspector())//发生修改时执行代码块
        {
            map.GenerateMap();//生成地图 
        }

        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
           
}
