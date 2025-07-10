using System.Collections;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public RectTransform ChooseMapUI;
    [Header("Drops")]
    public RectTransform PlayerDropParent;
    public GameObject PlayerDropPrefab;
    public RectTransform PositionDropParent;
    public GameObject PositionDropPrefab;
    [Header("Map Preview")]
    public Image MapImage;
    public TextMeshProUGUI MapName;

    public void ChooseEnemy()
    {
        AudioManager.Get().PlaySFX(3);
    }

    public void ShowMapChooseUI()
    {
        ChooseMapUI.gameObject.SetActive(true);
    }

    public void CloseMapChooseUI()
    {
        ChooseMapUI.gameObject.SetActive(false);
    }

    public void UpdateMapPreview(Sprite _sprite, string _name)
    {
        MapImage.sprite = _sprite;
        MapName.text = _name;
    }

    public void UpdateDropDownScroll(int _count)
    {
        for (int i = 1; i < PlayerDropParent.childCount; i++)
        {
            Destroy(PlayerDropParent.GetChild(i).gameObject);
        }

        foreach (Transform child in PositionDropParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < _count - 1; i++)
        {
            Instantiate(PlayerDropPrefab, PlayerDropParent);
        }

        for (int i = 0; i < _count; i++)
        {
            Instantiate(PositionDropPrefab, PositionDropParent);
        }

        StartCoroutine(UpdatePositionUIWithDelay()); // 为了避免延迟销毁的问题，需要等待一帧的时间在获取子物体数量
    }

    public IEnumerator UpdatePositionUIWithDelay()
    {
        yield return null;

        ChooseMapUI.GetComponent<ChooseMapUI>().ApplyPositionUIUpdate();
    }
}
