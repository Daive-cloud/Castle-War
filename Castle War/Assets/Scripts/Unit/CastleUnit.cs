using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;
using System.Net;
using System.Linq;
using DG.Tweening.Core.Easing;

public class CastleUnit : StructureUnit
{
    [Header("Generate Resource")]
    [SerializeField] private GameObject goldImagePrefab;
    [SerializeField] private int BaseProductionAmount;
    [SerializeField] private int AddProductionAmount;
    [SerializeField] private float ProductionFrequency;
    private float ProductionTimer;
    [SerializeField] private Tile placementTile;

    protected override void Start()
    {
        base.Start();

        if (CompareTag("BlueUnit") && !IsDead)
        {
            SetTile(transform);
        }
        ProductionTimer = Time.time;
    }

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        if (Time.time - ProductionTimer >= ProductionFrequency && CompareTag("BlueUnit") && IsCompleted && !IsDead)
        {
            ProductionTimer = Time.time;
            int amount = BaseProductionAmount + FindCastleCount() * AddProductionAmount;
            GameObject newImage = Instantiate(goldImagePrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
            EventCenter.Instance.EventTrigger("GoldProduction",amount);
            BounceEffect();

            newImage.GetComponentInChildren<TextMeshProUGUI>().text = "+ " + amount.ToString();
            newImage.transform.DOMove(newImage.transform.position + new Vector3(0, 2, 0), .8f).SetEase(Ease.Linear).OnComplete(() => Destroy(newImage.gameObject));

            m_GameManager.GoldAmount += amount;
            m_GameManager.onResourcesChanged?.Invoke();
            AudioManager.Get().PlaySFX(31);
        }
    }

    private void PlaceTile(Transform _castle,Tile _tile)
    {
        var node = TilemapManager.Get().FindNode(_castle.position);
        var pos = new Vector3Int(node.ButtomX, node.ButtomY, 0);
        for (int i = -8; i <= 8; i++)
        {
            for (int j = -8; j <= 8; j++)
            {
                int gridX = pos.x + i;
                int gridY = pos.y + j;
                TilemapManager.Get().BuildingAreaTilemap.SetTile(new Vector3Int(gridX, gridY, 0), _tile);
            }
        }
    }

    public override void Death()
    {
        base.Death();

        ClearTile();
    }

    private void SetTile(Transform _castle) => PlaceTile(_castle,placementTile);

    private void ClearTile()
    {
        PlaceTile(transform, null);

        var castles = FindObjectsOfType<CastleUnit>().Where(unit => unit != null && !unit.IsDead && unit.CompareTag("BlueUnit"));
        foreach (var castle in castles)
        {
            SetTile(castle.transform);
        }
    } 
}
