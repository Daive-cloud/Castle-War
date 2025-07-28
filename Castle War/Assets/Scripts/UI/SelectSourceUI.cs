using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectSourceUI : MonoBehaviour
{
    public TextMeshProUGUI ResourcesFont;

    private Slider slider => GetComponentInChildren<Slider>();
    [SerializeField] private int minAmount = 2000;
    [SerializeField] private int maxAmount = 10000;

    public void AdjustResource()
    {
        float temp = slider.value;

        int number = Mathf.RoundToInt(maxAmount * temp + minAmount * (1 - temp));
        if(number > 10)
            number = number / 50 * 50;
        ResourcesFont.text = number.ToString();
    }
}
