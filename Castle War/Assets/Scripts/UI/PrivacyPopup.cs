using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivacyPopup : MonoBehaviour
{
    [SerializeField] private GameObject privacyPanel;

    void Start()
    {
        if (PlayerPrefs.GetInt("PrivacyAccepted", 0) == 0)
        {
            privacyPanel.SetActive(true);
        }
        else
        {
            privacyPanel.SetActive(false);
        }
    }

    public void OnAgree()
    {
        // 玩家点击“同意”时，记录状态
        PlayerPrefs.SetInt("PrivacyAccepted", 1);
        PlayerPrefs.Save();
        privacyPanel.SetActive(false); // 关闭弹窗
        // 继续进入游戏（这里可以放游戏主逻辑的启动）
    }

    public void OnDisagree()
    {
        // 玩家点击“不同意”时，直接退出应用
        Application.Quit();
    }

    [ContextMenu("Clear PlayerPrefs")]
    void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs 已清空！");
    }
}
