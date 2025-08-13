using DG.Tweening;
using DG.Tweening.Core.Easing;
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
    [SerializeField] private int ProductionAmount;
    [SerializeField] private float ProductionFrequency;
    private float ProductionTimer;
    private GameObjectPool pool;
    protected override void Start()
    {
        base.Start();
        ProductionTimer = Time.time;
        pool = GameObjectPool.Get();
    }

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        if (Time.time - ProductionTimer >= ProductionFrequency && CompareTag("BlueUnit") && IsCompleted && !IsDead)
        {
            ProductionTimer = Time.time;
            GameObject newImage = null;
            int amount = ProductionAmount * FindCastleCount();
            if (barrackType == BarrackType.Knight)
            {
                newImage = pool.GetFromPool(woodImagePrefab.name);
                m_GameManager.WoodAmount += amount;
                EventCenter.Instance.EventTrigger("WoodProduction", amount);
            }
            else
            {
                newImage = pool.GetFromPool(meatImagePrefab.name);
                m_GameManager.MeatAmount += amount;
                EventCenter.Instance.EventTrigger("MeatProduction", amount);
            }
            BounceEffect();
            newImage.transform.position = transform.position + new Vector3(0, 1, 0);
            newImage.transform.rotation = Quaternion.identity;

            newImage.GetComponentInChildren<TextMeshProUGUI>().text = "+ " + amount.ToString();
            newImage.transform.DOMove(newImage.transform.position + new Vector3(0, 2, 0), .8f).SetEase(Ease.Linear).OnComplete(() => OnPopUpImage(newImage));

            m_GameManager?.onResourcesChanged?.Invoke();
            AudioManager.Get().PlaySFX(31);
        }
    }
    public override void UnselectedUnit()
    {
        base.UnselectedUnit();

        m_GameManager.CancleTraining();
    }
    
    private void OnPopUpImage(GameObject _image)
    {
        var name = _image.name.Replace("(Clone)", "");
        GameObjectPool.Get().ReturnToPool(name,_image);
    }
}
