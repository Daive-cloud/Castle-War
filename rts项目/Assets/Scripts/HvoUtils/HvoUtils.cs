using UnityEngine;
using UnityEngine.EventSystems;

public class HvoUtils
{
    private const float a = 0.45f;
    private const float p = 0.85f;
    public static Vector3 GetPlacementPosition() => Input.GetMouseButton(0) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
    public static bool IsPointerOverUIElement()
    {
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
    public static bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    public static bool IsPointerPress() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Stationary : Input.GetMouseButton(0);
    public static bool IsPointerUp() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);
    public static bool IsCancleSelect() => Input.touchCount > 0 ? Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(1);
    public static Vector3 GetPointerPositoin() => Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;

    public static int ComputeTrainingTime(int _barrackCount, float _maxTime,float _minTime)
    {
        int n = Mathf.Clamp(_barrackCount, 1, 12);
        float time = _minTime + (_maxTime - _minTime)/(1 + a * Mathf.Pow(n - 1, p));
        return Mathf.CeilToInt(time);
    }

}
