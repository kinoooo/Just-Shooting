using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public enum AudioChannel { Master,Sfx,Music};
    public float masterVolumePercent { get; private set; }//总音量
    public float sfxVolumePercent { get; private set; }//音效音量
    public float musicVolumePercent { get; private set; }//音乐音量

    AudioSource sfx2DSource;//2D音频组件对象
    AudioSource[] musicSources;//两个音频组件对象
    int activeMusicSourceIndex;//当前播放的音乐对象的数组下标

    Transform audioListener;//音频接收器的变换组件
    Transform playerT;//玩家的变换组件

    SoundLibrary library;

    public static AudioManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2]; //生成音乐播放对象
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source " + (i + 1));//生成两个物体对象挂载音频组件
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();//为两个物体添加音频组件
                newMusicSource.transform.parent = transform;//使生成的音频对象的父对象为音频管理器
            }
            GameObject newSfx2DSource = new GameObject("2D sfx source");//生成物体挂载音频组件
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();//为物体添加音频组件
            newSfx2DSource.transform.parent = transform;//使生成的音频对象的父对象为音频管理器
            audioListener = FindObjectOfType<AudioListener>().transform;//获取音频接收器的变换组件
            if (FindObjectOfType<Player>() != null) { 
                playerT = FindObjectOfType<Player>().transform;//获取玩家的变换组件
            }

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1);
        }
    }

    void OnLevelWasLoaded(int index)
    {
        if (playerT == null)
        {
            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
        }
    }


    private void Update()
    {
        if(playerT != null)
        {
            audioListener.position = playerT.position;
        }
    }

    //设置音量大小 传入音量百分比、音量类型
    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }
        if (musicSources[0] != null)
        {
            for(int i = 0; i < musicSources.Length; i++)
            {
                musicSources[i].volume = masterVolumePercent * musicVolumePercent;
            }
        }

        //记录玩家偏好，以便下次启动时应用
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    //播放音乐  fadeDuration 淡入淡出时间
    public void PlayMusic(AudioClip clip,float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play(); 

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    //播放音效
    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null) { 
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);//在pos位置播放音频clip
        }
    }

    public void PlaySound(string soundName,Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    //播放2D音效
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }

    }
}
