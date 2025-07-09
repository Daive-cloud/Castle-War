using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMinerUnit : StructureUnit
{
    [Header("Prodection Info")]
    [SerializeField] private Sprite ProductionImage;
    [SerializeField] private float ProductionFrequency;
    private float ProductionTimer;
    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        if(!IsUnderConstruction && this.CompareTag("BlueUnit"))
        {
            if(Time.time - ProductionTimer >= ProductionFrequency)
            {
                ProductionTimer = Time.time;
                StartCoroutine(ProduceGold());
            }
        }
    }

    private IEnumerator ProduceGold()
    {
        var originalSprite = sr.sprite;
        sr.sprite = ProductionImage;

        m_GameManager.GoldAmount += 150;
        m_GameManager.onResourcesChanged?.Invoke();

        yield return new WaitForSeconds(1f);
        sr.sprite = originalSprite;
    }
}
