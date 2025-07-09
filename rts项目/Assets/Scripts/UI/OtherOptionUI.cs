using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class OtherOptionUI : MonoBehaviour
{
    public RectTransform Tick;


    public void PutTick()
    {
        AudioManager.Get().PlaySFX(4);
        if (Tick.gameObject.activeSelf)
        {
            Tick.gameObject.SetActive(false);
        }
        else
        {
            Tick.gameObject.SetActive(true);
        }
    }
}
