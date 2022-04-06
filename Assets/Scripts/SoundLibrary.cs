using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    Dictionary<string, AudioClip[]> groupDictionary=new Dictionary<string, AudioClip[]>();

    private void Awake()
    {
        foreach(SoundGroup soundGroup in soundGroups)
        {
            groupDictionary.Add(soundGroup.groupID, soundGroup.group);//����soundGroups��������Ԫ����ӽ�Dictionary
        }
    }

    public AudioClip GetClipFromName(string name)
    {
        if (groupDictionary.ContainsKey(name))//�ֵ���������
        {
            AudioClip[]sounds=groupDictionary[name];//�洢�ֵ�����������Ƶ����
            return sounds[Random.Range(0, sounds.Length)];//������Ƶ����������һ��
        }
        return null;
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string groupID;
        public AudioClip[] group;
    }
}
