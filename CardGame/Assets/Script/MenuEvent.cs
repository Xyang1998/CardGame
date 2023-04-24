using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEvent : MonoBehaviour
{
    /// <summary>
    /// 开始对战
    /// </summary>
    public void StartBattle()
    {
        SceneManager.LoadSceneAsync("Scenes/Battle");
    }

    /// <summary>
    /// 卡组构建
    /// </summary>
    public void CardSetCreate()
    {
        
        SceneManager.LoadScene("Scenes/CardSetCreate");
    }
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

}
