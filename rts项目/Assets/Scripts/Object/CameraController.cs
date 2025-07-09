using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController
{
    private float PanSpeed;
    private CameraBounds CameraBounds;

    public CameraController(float _panSpeed, CameraBounds cameraBounds)
    {
        PanSpeed = _panSpeed;
        CameraBounds = cameraBounds;
    }

    public void Update()
    {
        if(Input.GetMouseButton(1))
        {
            Vector2 mouseDeltaPosition = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
            Vector3 newPosition = Camera.main.transform.position + new Vector3(-mouseDeltaPosition.x * PanSpeed * Time.deltaTime,
                                                                                      -mouseDeltaPosition.y * PanSpeed * Time.deltaTime,0);
            Camera.main.transform.position = CameraBounds.GetClampedPosition(newPosition);
        }
    }
  
}
