using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class GameOptionUI : MonoBehaviour
{
    public void Continue()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void Retreat()
    {
        Time.timeScale = 1f;
        AudioManager.Get().StopPlayBGM(1);
        AudioManager.Get().PlayBGM(0);
        gameObject.SetActive(false);
        SceneManager.LoadScene("选择关卡");
    }
}
