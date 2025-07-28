using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController
{
    private float PanSpeed;
    private CameraBounds CameraBounds;
    private Joystick joyStick;

    public CameraController(float _panSpeed, CameraBounds cameraBounds, Joystick _joystick)
    {
        PanSpeed = _panSpeed;
        CameraBounds = cameraBounds;
        joyStick = _joystick;
    }

    public void Update()
    {
        float hortInput = joyStick.Horizontal;
        float vertInput = joyStick.Vertical;

        if (Mathf.Abs(hortInput) > .1f || Mathf.Abs(vertInput) > .1f)
        {
            Vector3 delta = new Vector3(hortInput * PanSpeed * Time.deltaTime, vertInput * PanSpeed * Time.deltaTime, 0);

            Vector3 newPosition = Camera.main.transform.position + delta;
            Camera.main.transform.position = CameraBounds.GetClampedPosition(newPosition);
        }
    }
  
}
