using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    private BoxCollider2D  BoundsCollider => GetComponent<BoxCollider2D>();

    private float minX, maxX, minY, maxY;

    public Vector3 GetClampedPosition(Vector3 _targetPosition)
    {
        Camera cam = Camera.main;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        Bounds bounds = BoundsCollider.bounds;

        minX = bounds.min.x + horzExtent;
        maxX = bounds.max.x - horzExtent;
        minY = bounds.min.y + vertExtent;
        maxY = bounds.max.y - vertExtent;

        float clampedX = Mathf.Clamp(_targetPosition.x,minX,maxX);
        float clampedY = Mathf.Clamp(_targetPosition.y,minY,maxY);

        return new Vector3(clampedX,clampedY,Camera.main.transform.position.z);

    }
}
