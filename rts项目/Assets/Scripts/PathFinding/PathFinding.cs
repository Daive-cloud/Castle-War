using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private Node[,] m_Grid;
    public Node[,] Grid => m_Grid;

    private TilemapManager m_TilemapManager;

    private int m_Width;
    private int m_Height;

    private Vector3Int m_GridOffset;

    private List<Node> OpenList = new List<Node>();
    private HashSet<Node> CloseList = new HashSet<Node>();

    public PathFinding(TilemapManager _tilemapManager)
    {
        m_TilemapManager = _tilemapManager;

        var bounds = m_TilemapManager.WalkableTilemap.cellBounds;

        m_Width = bounds.size.x;
        m_Height = bounds.size.y;

        m_GridOffset = bounds.min;
        m_Grid = new Node[m_Width, m_Height];

        InitializeGrid(m_GridOffset);
    }

    private void InitializeGrid(Vector3Int _offset)
    {
        Vector3 cellSize = m_TilemapManager.WalkableTilemap.cellSize;

        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                Vector3Int leftButtomPosition = new Vector3Int(i + _offset.x, j + _offset.y, 0);
                bool isWalkable = m_TilemapManager.CanWalkAtTile(leftButtomPosition);
                var node = new Node(leftButtomPosition, cellSize, isWalkable);
                m_Grid[i, j] = node;
            }
        }
    }

    public Node FindNode(Vector3 _position) // ������λ��ת�ɽڵ�λ��
    {
        Vector3Int flooredPosition = new Vector3Int(Mathf.FloorToInt(_position.x), Mathf.FloorToInt(_position.y));

        int gridX = flooredPosition.x - m_GridOffset.x;
        int gridY = flooredPosition.y - m_GridOffset.y;

        if (gridX >= 0 && gridY >= 0 && gridX < m_Width && gridY < m_Height)
        {
            return Grid[gridX, gridY];
        }
        return null;
    }

    public List<Node> FindPath(Vector3 _startPosition, Vector3 _endPosition)
    {
        Node startNode = FindNode(_startPosition);
        Node endNode = FindNode(_endPosition);

        if (startNode == null || endNode == null)
        {
            ResetNode();
            return new List<Node>();
        }

        OpenList.Add(startNode);
        Node closestNode = startNode;
        int closestDistance = FindDistanceBetweenTwoNodes(startNode, endNode);

        while (OpenList.Count > 0)
        {
            Node currentNode = FindLowestFCostNode();

            if (currentNode == endNode)
            {
                var path = RetracePath(startNode, endNode);
                ResetNode();
                return path;
            }

            OpenList.Remove(currentNode);
            CloseList.Add(currentNode);

            List<Node> neighbors = FindNodeNeighbors(currentNode);

            foreach (var node in neighbors)
            {
                if (!CloseList.Contains(node))
                {
                    int tentativeGCost = FindDistanceBetweenTwoNodes(currentNode, node) + currentNode.gCost;

                    if (!OpenList.Contains(node) || tentativeGCost < node.gCost)
                    {
                        node.parent = currentNode;
                        node.gCost = tentativeGCost;
                        int distance = FindDistanceBetweenTwoNodes(node, endNode);
                        node.hCost = distance;
                        node.fCost = node.gCost + node.hCost;

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestNode = node;
                        }

                        OpenList.Add(node);
                    }

                }
            }
        }
        var unfinishedPath = RetracePath(startNode, closestNode);
        ResetNode();
        return unfinishedPath;
    }

    public bool CanReachDestination(Vector3 _startPosition, Vector3 _endPosition)
    {
        Node startNode = FindNode(_startPosition);
        Node endNode = FindNode(_endPosition);

        if (startNode == null || endNode == null)
        {
            ResetNode();
            return false;
        }

        OpenList.Add(startNode);

        while (OpenList.Count > 0)
        {
            Node currentNode = FindLowestFCostNode();

            if (currentNode == endNode)
            {
                var path = RetracePath(startNode, endNode);
                ResetNode();
                return true;
            }

            OpenList.Remove(currentNode);
            CloseList.Add(currentNode);

            List<Node> neighbors = FindNodeNeighbors(currentNode);

            foreach (var node in neighbors)
            {
                if (!CloseList.Contains(node))
                {
                    int tentativeGCost = FindDistanceBetweenTwoNodes(currentNode, node) + currentNode.gCost;

                    if (!OpenList.Contains(node) || tentativeGCost < node.gCost)
                    {
                        node.parent = currentNode;
                        node.gCost = tentativeGCost;
                        int distance = FindDistanceBetweenTwoNodes(node, endNode);
                        node.hCost = distance;
                        node.fCost = node.gCost + node.hCost;
                        OpenList.Add(node);
                    }

                }
            }
        }

        return false;
    }

    private void ResetNode() // ���ÿ����б��͹ر��б��е�ֵ
    {
        foreach (var node in OpenList)
        {
            node.hCost = 0;
            node.gCost = 0;
            node.fCost = 0;

            node.parent = null;
        }
        OpenList = new List<Node>();

        foreach (var node in CloseList)
        {
            node.hCost = 0;
            node.gCost = 0;
            node.fCost = 0;

            node.parent = null;
        }
        CloseList = new HashSet<Node>();
    }

    private Node FindLowestFCostNode()
    {
        Node currentNode = OpenList[0];

        foreach (var node in OpenList)
        {
            if (node.fCost < currentNode.fCost || (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost))
            {
                currentNode = node;
            }
        }

        return currentNode;
    }

    private List<Node> RetracePath(Node _startNode, Node _endNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = _endNode;
        while (currentNode != _startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private List<Node> FindNodeNeighbors(Node _node)
    {
        List<Node> neighbors = new List<Node>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                int gridX = _node.ButtomX + i - m_GridOffset.x;
                int gridY = _node.ButtomY + j - m_GridOffset.y;
                if (gridX >= 0 && gridY >= 0 && gridX < m_Width && gridY < m_Height)
                {
                    var node = Grid[gridX, gridY];
                    if (node.IsWalkable)
                        neighbors.Add(node);
                }

            }
        }
        return neighbors;
    }

    private int FindDistanceBetweenTwoNodes(Node _nodeA, Node _nodeB)
    {
        int distanceX = Mathf.Abs(_nodeA.ButtomX - _nodeB.ButtomX);
        int distanceY = Mathf.Abs(_nodeA.ButtomY - _nodeB.ButtomY);

        if (distanceX > distanceY)
        {
            return distanceY * 14 + (distanceX - distanceY) * 10;
        }
        else
        {
            return distanceX * 14 + (distanceY - distanceX) * 10;
        }
    }

    public void UpdateNodesInArea(Vector3Int _startPosition, int _width, int _height)
    {
//        Debug.Log($"start position : {_startPosition}.");
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                int gridX = _startPosition.x + i - m_GridOffset.x;
                int gridY = _startPosition.y + j - m_GridOffset.y;

                //                Debug.Log($"position : ({gridX},{gridY}");
                if (gridX >= 0 && gridY >= 0 && gridX < m_Width && gridY < m_Height) // 这里要判断节点是否在地图范围内，后面的单位已经转移好了
                {
                    Node node = Grid[gridX, gridY];
                    Vector3Int cellPosition = new Vector3Int(node.ButtomX, node.ButtomY, 0);
                    node.IsWalkable = m_TilemapManager.CanWalkAtTile(cellPosition);
                    //                    Debug.Log($"cellPosition : {cellPosition} , IsWalkable : {node.IsWalkable}");
                }
            }
        }
    }

    public void UpdateNodesOverMap()
    {
        Debug.Log("UpdateMapArea.");
        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                Node node = m_Grid[i, j];
                Vector3Int cellPosition = new Vector3Int(node.ButtomX, node.ButtomY, 0);
                node.IsWalkable = m_TilemapManager.CanWalkAtTile(cellPosition);
            }
        }
    }
}
