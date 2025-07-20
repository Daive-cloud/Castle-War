using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepUnit : HumanoidUnit
{
    private float MoveFrequency;
    private float lastTimeMoved;

    protected override void UpdateBehaviour()
    {
        if (Time.time - lastTimeMoved >= MoveFrequency)
        {
            lastTimeMoved = Time.time;
            MoveFrequency = Random.Range(5f, 15f);
            RandomMovement();
        }
    }


    private void RandomMovement()
    {
        List<Vector2> vaildPos = new();
        var node = TilemapManager.Get().FindNode(transform.position);
        var nowPos = new Vector2Int(node.ButtomX, node.ButtomY);
        for (int i = -3; i <= 3; i++)
        {
            for (int j = -3; j <= 3; j++)
            {
                int gridX = nowPos.x + i;
                int gridY = nowPos.y + j;
                var targetPos = new Vector3Int(gridX, gridY, 0);
                if (TilemapManager.Get().CanWalkAtTile(targetPos))
                {
                    vaildPos.Add(new Vector2(targetPos.x,targetPos.y));
                }
            }
        }
        MoveToDestination(vaildPos[Random.Range(0,vaildPos.Count -1)]);
    }
}
