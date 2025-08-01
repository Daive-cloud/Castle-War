using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    private RectTransform myTransform => GetComponent<RectTransform>();
    private Slider slider => GetComponent<Slider>();
    private Unit unit => GetComponentInParent<Unit>();

    private void Start()
    {
        unit.onFlipped += Flip;
        unit.stats.onHealthChanged += HealthChange;
        
        gameObject.SetActive(false);
    }

    private void Flip()
    {
        myTransform.Rotate(0, 180, 0);
    }

    private void HealthChange()
    {
        if (slider != null)
        {
            float temp = (float)unit.stats.CurrentHealth / unit.stats.GetMaxHealthValue();
//            Debug.Log($"temp value : {temp}");
            slider.value = (float)unit.stats.CurrentHealth / unit.stats.GetMaxHealthValue();
         //   Debug.Log($"slider value : {slider.value}.");
        }
    }

  
}
