using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrainingUnitUI : MonoBehaviour
{
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button CancleButton;
    [SerializeField] private TextMeshProUGUI goldCost;
    [SerializeField] private TextMeshProUGUI meatCost;

    private void Start()
    {
        HideRectangle();
    }

    public void ShowRectangle(int _goldCost, int _meatCost)
    {
        gameObject.SetActive(true);
        goldCost.text = _goldCost.ToString();
        meatCost.text = _meatCost.ToString();

        var manager = GameManager.Get();
        goldCost.color = manager.GoldAmount >= _goldCost ? Color.black : Color.red;
        meatCost.color = manager.MeatAmount >= _meatCost ? Color.black : Color.red;
    }

    public void HideRectangle()
    {
        gameObject.SetActive(false);
    }

    public void RegisterHooks(UnityAction _confirmAction,UnityAction _cancleAction)
    {
        ConfirmButton.onClick.RemoveAllListeners();
        CancleButton.onClick.RemoveAllListeners();

        ConfirmButton.onClick.AddListener(_confirmAction);
        CancleButton.onClick.AddListener(_cancleAction);   
    }

    private void OnDisable()
    {
        ConfirmButton.onClick.RemoveAllListeners();
        CancleButton.onClick.RemoveAllListeners();
    }
}
