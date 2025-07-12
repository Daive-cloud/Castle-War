using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEditor;
using Unity.VisualScripting;

public class ResourcesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI WoodAmount;
    [SerializeField] private TextMeshProUGUI GoldAmount;
    [SerializeField] private TextMeshProUGUI MeatAmount;

    private GameManager m_GameManager;

    private void Start()
    {
        m_GameManager = GameManager.Get();

        InitializeResourcesValue();
        m_GameManager.onResourcesChanged += ResourcesChange;
    }

    private void ResourcesChange()
    {
        DOTween.To(() => int.Parse(WoodAmount.text.Replace(",", "")), x => { WoodAmount.text = x.ToString("N0"); },
                                m_GameManager.WoodAmount, .5f).SetEase(Ease.InQuad);

        DOTween.To(() => int.Parse(GoldAmount.text.Replace(",", "")), x => { GoldAmount.text = x.ToString("N0"); },
                                    m_GameManager.GoldAmount, .5f).SetEase(Ease.InQuad);

        DOTween.To(() => int.Parse(MeatAmount.text.Replace(",", "")), x => { MeatAmount.text = x.ToString("N0"); },
                                    m_GameManager.MeatAmount, .5f).SetEase(Ease.InQuad);
    }

    private void InitializeResourcesValue()
    {
        WoodAmount.text = m_GameManager.WoodAmount.ToString("N0");
        GoldAmount.text = m_GameManager.GoldAmount.ToString("N0");
        MeatAmount.text = m_GameManager.MeatAmount.ToString("N0");
    }

}
