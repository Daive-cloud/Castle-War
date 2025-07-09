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
    [SerializeField] private TextMeshProUGUI goldCost;
    [SerializeField] private TextMeshProUGUI woodCost;

    private void Start()
    {
        HideRectangle();
    }

    public void HideRectangle()
    {
        gameObject.SetActive(false);
    }

    public void ShowRectangle(int _goldCost,int _woodCost)
    {
        gameObject.SetActive(true);
        goldCost.text = _goldCost.ToString();
        woodCost.text = _woodCost.ToString();
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
