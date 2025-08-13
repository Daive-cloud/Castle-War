using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class PlaceBuildingUI : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancleButton;
    [SerializeField] private RectTransform telemplateParent;
    [SerializeField] private GameObject costTelemplate;
    [SerializeField] private Sprite woodIcon;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite crystalIcon;

    private void Start()
    {
        HideRectangle();
    }

    public void HideRectangle()
    {
        gameObject.SetActive(false);
    }

    public void ShowRectangle(int _goldCost, int _woodCost, int _crystalCost)
    {
        gameObject.SetActive(true);
        var manager = GameManager.Get();
        for (int i = 0; i < telemplateParent.childCount; i++)
        {
            Destroy(telemplateParent.GetChild(i).gameObject);
        }

        if (_goldCost > 0)
        {
            var newTelemplate = Instantiate(costTelemplate, telemplateParent);
            newTelemplate.GetComponentInChildren<Image>().sprite = goldIcon;
            var content = newTelemplate.GetComponentInChildren<TextMeshProUGUI>();
            content.text = _goldCost.ToString("N0");
            content.color = manager.GoldAmount >= _goldCost ? Color.black : Color.red;
        }

        if (_woodCost > 0)
        {
            var newTelemplate = Instantiate(costTelemplate, telemplateParent);
            newTelemplate.GetComponentInChildren<Image>().sprite = woodIcon;
            var content = newTelemplate.GetComponentInChildren<TextMeshProUGUI>();
            content.text = _woodCost.ToString("N0");
            content.color = manager.WoodAmount >= _woodCost ? Color.black : Color.red;
        }
        
        if (_crystalCost > 0)
        {
            var newTelemplate = Instantiate(costTelemplate, telemplateParent);
            newTelemplate.GetComponentInChildren<Image>().sprite = crystalIcon;
            var content = newTelemplate.GetComponentInChildren<TextMeshProUGUI>();
            content.text = _crystalCost.ToString("N0");
            content.color = manager.CrystalAmount >= _crystalCost ? Color.black : Color.red;
        }
    }

    public void RegisterHooks(UnityAction _confirmMethod,UnityAction _cancleAction)
    {
        confirmButton.onClick.AddListener(_confirmMethod);
        cancleButton.onClick.AddListener(_cancleAction);
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancleButton.onClick.RemoveAllListeners();
    }
}
