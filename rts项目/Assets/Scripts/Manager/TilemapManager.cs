
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : SingletonManager<TilemapManager>
{
    public Tilemap WalkableTilemap;
    public Tilemap PlacementTilemap;
    public Tilemap[] UnreachableTilemap;
    public Tilemap BuildingAreaTilemap;
    private PathFinding m_PathFinding;

    private void Start()
    {
        m_PathFinding = new(this);
    }

    public List<Node> FindPath(Vector3 _startPosition, Vector3 _endPosition) => m_PathFinding.FindPath(_startPosition, _endPosition);

    public Node FindNode(Vector3 _position) => m_PathFinding.FindNode(_position);
    public bool CanPlaceBuinding(Vector3Int _position)
    {
        return BuildingAreaTilemap.HasTile(_position) && !IsPlaceOverUnreachbleArea(_position);
    }

    private bool IsPlaceOverUnreachbleArea(Vector3Int _position)
    {
        return IsUnreachableHasTile(_position) || IsPlaceAreaOverObstacle(_position);
    }

    public bool IsPlaceAreaOverObstacle(Vector3Int _position)
    {
        Vector3 tileSize = WalkableTilemap.cellSize;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(_position + tileSize * .5f, tileSize * .9f, 0);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Unit _) || collider.gameObject.tag == "Tree")
            {
                return true;
            }
        }
        return false;
    }

    public bool CanWalkAtTile(Vector3Int _position)
    {
        //        Debug.Log($"CanWalkAtTile : {_position}");
        return WalkableTilemap.HasTile(_position) && !IsUnreachableHasTile(_position) && !IsBlockedByBuilding(_position);
    }

    private bool IsBlockedByBuilding(Vector3Int _tilePosition)
    {
        Vector3 worldPosition = WalkableTilemap.CellToWorld(_tilePosition) + WalkableTilemap.cellSize * .5f;
        int buildingLayerMask = LayerMask.NameToLayer("Building");
        int treeLayerMask = LayerMask.NameToLayer("Tree");

        Collider2D[] colliders = Physics2D.OverlapBoxAll(worldPosition, WalkableTilemap.cellSize * .95f, 0);
        //        Debug.Log($"collision count : {colliders.Length}.");
        foreach (var collider in colliders)
        {
            if (collider == null) continue;

            if (collider.gameObject.layer == buildingLayerMask || collider.gameObject.layer == treeLayerMask)
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateNodesInArea(Vector3Int _startPosition, int _width, int _height) => m_PathFinding.UpdateNodesInArea(_startPosition, _width, _height);

    public void UpdateNodesOverMap() => m_PathFinding.UpdateNodesOverMap();

    public void SetTile(Vector3Int _position)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Resources.Load<Sprite>("Image/PlacementTile");
        PlacementTilemap.SetTile(_position, tile);
        PlacementTilemap.SetTileFlags(_position, TileFlags.None);
        PlacementTilemap.SetColor(_position, Color.red);
    }

    private bool IsUnreachableHasTile(Vector3Int _position)
    {
        foreach (var tile in UnreachableTilemap)
        {
            if (tile.HasTile(_position))
            {
                return true;
            }
        }
        return false;
    }
}
