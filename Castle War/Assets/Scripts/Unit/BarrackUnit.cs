using DG.Tweening;
using TMPro;
using UnityEngine;

public enum BarrackType
{
    Knight,
    Goblin
}

public class BarrackUnit : StructureUnit
{
    [Header("Generate Resources")]
    [SerializeField] private BarrackType barrackType = BarrackType.Knight; 
    [SerializeField] private GameObject woodImagePrefab;
    [SerializeField] private GameObject meatImagePrefab;
    [SerializeField] private float ProductionFrequency;
    private float Timer;

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        if (Time.time - Timer >= ProductionFrequency && CompareTag("BlueUnit") && IsCompleted)
        {
            Timer = Time.time;
            GameObject newImage = null;
            int amount = 20 * FindCastleCount();
            if (barrackType == BarrackType.Knight)
            {
                newImage = Instantiate(woodImagePrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                m_GameManager.WoodAmount += amount;
            }
            else
            {
                newImage = Instantiate(meatImagePrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                m_GameManager.MeatAmount += amount;
            }
            newImage.GetComponentInChildren<TextMeshProUGUI>().text = "+ " + amount.ToString();

            newImage.transform.DOMove(newImage.transform.position + new Vector3(0, 2, 0), 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(newImage.gameObject));

            m_GameManager?.onResourcesChanged?.Invoke();
            AudioManager.Get().PlaySFX(31);
        }
    }
    public override void UnselectedUnit()
    {
        base.UnselectedUnit();

        m_GameManager.CancleTraining();
    }
}
