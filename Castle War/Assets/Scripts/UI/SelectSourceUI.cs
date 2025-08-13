using System.Collections;
using System.Collections.Generic;
using IUnit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectSourceUI : MonoBehaviour , ISaveData
{
    public TextMeshProUGUI ResourcesFont;
    private Slider slider => GetComponentInChildren<Slider>();
    [SerializeField] private int minAmount = 2000;
    [SerializeField] private int maxAmount = 10000;
    public ResourceType resourceType = ResourceType.wood;
    private int resourceAmount;

    public void AdjustResource()
    {
        float temp = slider.value;

        resourceAmount = Mathf.RoundToInt(maxAmount * temp + minAmount * (1 - temp));
        if (resourceAmount > 10)
            resourceAmount = resourceAmount / 50 * 50;
        ResourcesFont.text = resourceAmount.ToString();
    }

    public void LoadData(GameData _gameData)
    {
        switch (resourceType)
        {
            case ResourceType.wood:
                resourceAmount = _gameData.woodAmount;
                break;
            case ResourceType.meat:
                resourceAmount = _gameData.meatAmount;
                break;
            case ResourceType.gold:
                resourceAmount = _gameData.goldAmount;
                break;
            case ResourceType.army:
                resourceAmount = _gameData.armyAmount;
                break;
            default:
                return;
        }

        ResourcesFont.text = resourceAmount.ToString();
        slider.value =(float) (resourceAmount - minAmount) / (maxAmount - minAmount);
    }

    public void SaveData(ref GameData _gameData)
    {   
         switch (resourceType)
        {
            case ResourceType.wood:
                _gameData.woodAmount = resourceAmount;
                break;
            case ResourceType.meat:
                _gameData.meatAmount = resourceAmount;
                break;
            case ResourceType.gold:
                _gameData.goldAmount = resourceAmount;
                break;
            case ResourceType.army:
                _gameData.armyAmount = resourceAmount;
                break;
            default:
                return;
        }
    }
}
