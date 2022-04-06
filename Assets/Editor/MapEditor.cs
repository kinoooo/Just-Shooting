using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//��ͼ�༭��
[CustomEditor(typeof(MapGenerator))]
public class NewBehaviourScript : Editor
{
    //��д�������
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI(); �������� ���ܿ���̫��

        //���������Ķ���
        MapGenerator map = target as MapGenerator;

        if (DrawDefaultInspector())//�����޸�ʱִ�д����
        {
            map.GenerateMap();//���ɵ�ͼ 
        }

        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
           
}
