using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string currentSceneName;
    private void Start()
    {
        OnLevelWasLoaded(0);
    }

    void OnLevelWasLoaded(int sceneIndex)
    {
        string newSceneName=SceneManager.GetActiveScene().name;
        if (newSceneName != currentSceneName)
        {
            currentSceneName=newSceneName;
            Invoke("PlayMusic", .2f);//��Ϊʵ���˵��뵭��Ч�������Բ�����ֱ�ӵ���PlayMusic()
        }
    }

    void PlayMusic()
    {
        AudioClip clipToPlay = null;

        if (currentSceneName == "Menu")
        {
            clipToPlay = menuTheme;
        }
        else if (currentSceneName == "Game")
        {
            clipToPlay = mainTheme;
        }

        if(clipToPlay != null)
        { 
            AudioManager.instance.PlayMusic(clipToPlay,2);
            Invoke("PlayMusic",clipToPlay.length);//��������ٵ���PlayMusicѭ������
        }
    }
}
