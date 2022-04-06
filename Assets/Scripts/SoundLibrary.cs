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
            groupDictionary.Add(soundGroup.groupID, soundGroup.group);//遍历soundGroups，将所有元素添加进Dictionary
        }
    }

    public AudioClip GetClipFromName(string name)
    {
        if (groupDictionary.ContainsKey(name))//字典搜索名字
        {
            AudioClip[]sounds=groupDictionary[name];//存储字典搜索到的音频数组
            return sounds[Random.Range(0, sounds.Length)];//返回音频数组中任意一个
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
