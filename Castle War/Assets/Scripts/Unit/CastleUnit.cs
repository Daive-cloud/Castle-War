using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;
using System.Net;

public class CastleUnit : StructureUnit
{
    [Header("Generate Resource")]
    [SerializeField] private GameObject goldImagePrefab;
    [SerializeField] private float ProductionFrequency;
    private float Timer;
    [SerializeField] private Tile placementTile;

    protected override void Start()
    {
        base.Start();

        if (CompareTag("BlueUnit") && !IsDead)
        {
            SetTile();
        }
    }

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

    private void PlaceTile(Tile _tile)
    {
        var node = TilemapManager.Get().FindNode(transform.position);
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

    private void SetTile() => PlaceTile(placementTile);

    private void ClearTile() => PlaceTile(null);
}
