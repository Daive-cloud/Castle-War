using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementProcess
{
    private GameObject m_PlacementOutline;
    private BuildingActionSO m_BuildingAction;
    private TilemapManager m_TilemapManager;
    private Vector3Int[] m_HighlightedArea;
    private Sprite m_PlacementTile;

    public BuildingActionSO BuildingAction => m_BuildingAction;

    public PlacementProcess(BuildingActionSO _action,TilemapManager _tilemapManager)
    {
        m_PlacementOutline = new GameObject("Placement Outline");
        m_TilemapManager = _tilemapManager;
        m_BuildingAction = _action;
        m_PlacementTile = Resources.Load<Sprite>("Image/PlacementTile");
        m_HighlightedArea = new Vector3Int[] { };
    }

    public PlacementProcess(BuildingActionSO _action,TilemapManager _tilemapManager,bool _IsEnemyAI = true)
    {
        m_TilemapManager = _tilemapManager;
        m_BuildingAction = _action;
        m_HighlightedArea = new Vector3Int[] { };
    }
    public void Update()
    {
        Vector3 worldPositon = HvoUtils.GetPlacementPosition();

        if (HvoUtils.IsPointerOverUIElement())
            return;

        if (m_HighlightedArea != null)
        {
            HighlightArea(m_PlacementOutline.transform.position);
        }

        if (worldPositon != Vector3.zero)
        {
            m_PlacementOutline.transform.position = SnapToGrid(worldPositon);
        }
    }

    private Vector3Int SnapToGrid(Vector3 _worldPosition) => new Vector3Int(Mathf.FloorToInt(_worldPosition.x),Mathf.FloorToInt(_worldPosition.y),0);

    private void HighlightArea(Vector3 _outlinePosition)
    {
        ClearHighlightArea();
        InitiaizeGrid(_outlinePosition);

        foreach(var position in m_HighlightedArea)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = m_PlacementTile;

            m_TilemapManager.PlacementTilemap.SetTile(position,tile); // ������Ƭ
            m_TilemapManager.PlacementTilemap.SetTileFlags(position,TileFlags.None);  // �����޸���Ƭ������

            if(m_TilemapManager.CanPlaceBuinding(position))
            {
                m_TilemapManager.PlacementTilemap.SetColor(position,Color.green);
            }
            else
            {
                m_TilemapManager.PlacementTilemap.SetColor(position, Color.red);
            }
        }

    }

    private void ClearHighlightArea()
    {
        if(m_HighlightedArea != null)
        {
            foreach(var position in m_HighlightedArea)
            {
                m_TilemapManager.PlacementTilemap.SetTile(position,null);
            }
            m_HighlightedArea = null;
        }
    }

    private void InitiaizeGrid(Vector3 _outlinePosition)
    {
        Vector3Int buildingSize = m_BuildingAction.BuildingSize;
        Vector3 leftButtomPosition = _outlinePosition + m_BuildingAction.BuildingOffset;

        m_HighlightedArea = new Vector3Int[buildingSize.x * buildingSize.y];

        for(int x = 0;x < buildingSize.x;x++)
        {
            for(int y = 0;y < buildingSize.y;y++)
            {
                m_HighlightedArea[x + buildingSize.x * y] = new Vector3Int((int)leftButtomPosition.x + x,(int)leftButtomPosition.y + y);
            }
        }
    }

    public bool IsPlacementValid()
    {
        foreach(var position in m_HighlightedArea)
        {
            if(!m_TilemapManager.CanPlaceBuinding(position))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearupPlacement()
    {
        Object.Destroy(m_PlacementOutline);
        ClearHighlightArea();
    }

    public bool CanPlaceBuilding(out Vector3 _placePosition)
    {
        foreach(var position in m_HighlightedArea)
        {
            if(!m_TilemapManager.CanPlaceBuinding(position))
            {
                _placePosition = Vector3.zero;
                return false;
            }
        }
        _placePosition = m_PlacementOutline.transform.position;
        return true;
    }

    public bool CanPlaceBuilding(Vector3 _placePosition)
    {
        InitiaizeGrid(_placePosition);

        foreach(var position in m_HighlightedArea)
        {
            if(m_TilemapManager.IsPlaceAreaOverObstacle(position))
            {
                return false;
            }
        }
        return true;
    }

    
}
