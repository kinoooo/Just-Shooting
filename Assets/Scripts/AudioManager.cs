using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public enum AudioChannel { Master,Sfx,Music};
    public float masterVolumePercent { get; private set; }//������
    public float sfxVolumePercent { get; private set; }//��Ч����
    public float musicVolumePercent { get; private set; }//��������

    AudioSource sfx2DSource;//2D��Ƶ�������
    AudioSource[] musicSources;//������Ƶ�������
    int activeMusicSourceIndex;//��ǰ���ŵ����ֶ���������±�

    Transform audioListener;//��Ƶ�������ı任���
    Transform playerT;//��ҵı任���

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

            musicSources = new AudioSource[2]; //�������ֲ��Ŷ���
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source " + (i + 1));//��������������������Ƶ���
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();//Ϊ�������������Ƶ���
                newMusicSource.transform.parent = transform;//ʹ���ɵ���Ƶ����ĸ�����Ϊ��Ƶ������
            }
            GameObject newSfx2DSource = new GameObject("2D sfx source");//�������������Ƶ���
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();//Ϊ���������Ƶ���
            newSfx2DSource.transform.parent = transform;//ʹ���ɵ���Ƶ����ĸ�����Ϊ��Ƶ������
            audioListener = FindObjectOfType<AudioListener>().transform;//��ȡ��Ƶ�������ı任���
            if (FindObjectOfType<Player>() != null) { 
                playerT = FindObjectOfType<Player>().transform;//��ȡ��ҵı任���
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

    //����������С ���������ٷֱȡ���������
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

        //��¼���ƫ�ã��Ա��´�����ʱӦ��
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    //��������  fadeDuration ���뵭��ʱ��
    public void PlayMusic(AudioClip clip,float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play(); 

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    //������Ч
    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null) { 
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);//��posλ�ò�����Ƶclip
        }
    }

    public void PlaySound(string soundName,Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    //����2D��Ч
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
