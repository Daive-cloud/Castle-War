using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] private BlackScreenController BlackScreen;
    public List<ButtonAnimation> Buttons;
    public RectTransform Warning;

    void Start()
    {
        StartCoroutine(EnterAnimation());
        AudioManager.Get().PlayBGM(0);
    }
    public void StartGame()
    {
        StartCoroutine(StartGameProcess());
    }

    private IEnumerator EnterAnimation()
    {
        yield return new WaitForSeconds(.3f);
        for (int i = 0; i < Buttons.Count; i++)
        {
            yield return new WaitForSeconds(.1f);
            Buttons[i].MoveInScreen();
        }
    }

    private IEnumerator OffAnimation()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            yield return new WaitForSeconds(.05f);
            Buttons[i].MoveOffScreen();
        }
    }

    private IEnumerator StartGameProcess()
    {
        AudioManager.Get().PlaySFX(1);
        StartCoroutine(OffAnimation());
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowWarning()
    {
      //  AudioManager.Get().PlayBGM(1);
        Warning.gameObject.SetActive(true);
    }

    public void CloseWarning()
    {
       // AudioManager.Get().PlayBGM(2);
        Warning.gameObject.SetActive(false);
    }
}
