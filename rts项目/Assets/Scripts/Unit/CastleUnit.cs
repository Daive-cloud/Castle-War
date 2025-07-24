using TMPro;
using UnityEngine;
using DG.Tweening;

public class CastleUnit : StructureUnit
{
    [Header("Generate Resource")]
    [SerializeField] private GameObject goldImagePrefab;
    [SerializeField] private float ProductionFrequency;
    private float Timer;

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        if (Time.time - Timer >= ProductionFrequency && CompareTag("BlueUnit") && IsCompleted)
        {
            Timer = Time.time;
            int amount = 160 + FindCastleCount() * 40;
            GameObject newImage = Instantiate(goldImagePrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);

            newImage.GetComponentInChildren<TextMeshProUGUI>().text = "+ " + amount.ToString();
            newImage.transform.DOMove(newImage.transform.position + new Vector3(0, 2, 0), 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(newImage.gameObject));

            m_GameManager.GoldAmount += amount;
            m_GameManager.onResourcesChanged?.Invoke();
            AudioManager.Get().PlaySFX(31);
        }
    }
}
