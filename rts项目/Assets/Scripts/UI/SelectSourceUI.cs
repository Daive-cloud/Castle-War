using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectSourceUI : MonoBehaviour
{
    public TextMeshProUGUI ResourcesFont;

    private Slider slider => GetComponentInChildren<Slider>();
    private const int minAmount = 2000;
    private const int maxAmount = 10000;

    public void AdjustResource()
    {
        float temp = slider.value;

        int number = Mathf.RoundToInt(maxAmount * temp + minAmount * (1 - temp));
        number = number / 10 * 10;
        ResourcesFont.text = number.ToString();
    }
}
