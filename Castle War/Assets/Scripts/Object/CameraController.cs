using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController
{
    private float PanSpeed;
    private float ZoomSpeed;
    private float MinZoom;
    private float MaxZoom;
    private CameraBounds CameraBounds;
    private Vector2 lastPanPosition;
    private Vector3 lastMousePosition;
    private int panFingerId;
    private bool isPanning;
    private bool isDragging;

    public CameraController(float _panSpeed, float _zoomSpeed, float _minZoom, float _maxZoom, CameraBounds cameraBounds)
    {
        PanSpeed = _panSpeed;
        ZoomSpeed = _zoomSpeed;
        MaxZoom = _maxZoom;
        MinZoom = _minZoom;
        CameraBounds = cameraBounds;
    }

    public void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = touch.position;
                panFingerId = touch.fingerId;
                isPanning = true;
            }
            else if (touch.phase == TouchPhase.Moved && isPanning && panFingerId == touch.fingerId)
            {
                Vector2 touchDelta = touch.position - lastPanPosition;
                PanCamera(touchDelta);
                lastPanPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isPanning = false;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1) && isDragging)
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
            PanCamera(new Vector2(deltaMouse.x, deltaMouse.y));
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }

    private void PanCamera(Vector2 _delta)
    {
        Vector3 panMovement = new Vector3(-_delta.x, -_delta.y, 0) * PanSpeed * Time.deltaTime;
        Vector3 newPosition = Camera.main.transform.position + panMovement;
        Camera.main.transform.position = CameraBounds.GetClampedPosition(newPosition);
    }
  
}
