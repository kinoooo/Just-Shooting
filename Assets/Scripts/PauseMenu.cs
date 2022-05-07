using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Œ¥ÕÍ
public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    private void Update()
    {
        //-------------‘›Õ£”Œœ∑
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale != 0)
            {
                Time.timeScale = 0;
                gameIsPaused = true;
            }
            else
            {
                Time.timeScale = 1;
                gameIsPaused = false;
            }
        }
    }
}
