using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public int ButtomX;
    public int ButtomY;
    public bool IsWalkable;
    public float CenterX;
    public float CenterY;

    public int gCost;
    public int hCost;
    public int fCost;

    public Node parent;

    public Node(Vector3Int _position,Vector3 _offset,bool _isWalkable)
    {
        ButtomX = _position.x;
        ButtomY = _position.y;

        Vector3 halfCellSize = _offset * .5f;

        CenterX = ButtomX + halfCellSize.x;
        CenterY = ButtomY + halfCellSize.y;

        IsWalkable = _isWalkable;

        gCost = 0;
        hCost = 0;
        fCost = 0;

        parent = null;
    }

    public string GetNodePosition()
    {
        return new string($"({ButtomX},{ButtomY})");
    }
}
