using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform myTransform;
    [SerializeField] private RectTransform levelUI;
    private Slider slider => GetComponentInChildren<Slider>();
    private Unit unit => GetComponentInParent<Unit>();

    private void Start()
    {
        unit.onFlipped += Flip;
        unit.stats.onHealthChanged += HealthChange;
    }

    private void Flip()
    {
        myTransform.Rotate(0, 180, 0);
        if (levelUI != null)
        {
            levelUI.Rotate(0,180,0);
        }
    }

    private void HealthChange()
    {
        if (slider != null)
        {
            slider.value = (float)unit.stats.CurrentHealth / unit.stats.GetMaxHealthValue();
        }
    }

  
}
