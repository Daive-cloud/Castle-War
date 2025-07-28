using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HvoUtils
{
    private const float a = 0.45f;
    private const float p = 0.85f;
    public static Vector3 GetPlacementPosition() => Input.GetMouseButton(0) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
    public static bool IsPointerOverUIElement()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = touch.position
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // 如果结果不为空，说明触摸位置在UI上
            return results.Count > 0;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
    public static bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    public static bool IsPointerPress() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Moved : Input.GetMouseButton(0);
    public static bool IsPointerUp() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);
    public static bool IsCancleSelect() => Input.touchCount > 0 ? Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(1);
    public static Vector3 GetPointerPositoin() => Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;

    public static int ComputeTrainingTime(int _barrackCount, float _maxTime, float _minTime)
    {
        int n = Mathf.Clamp(_barrackCount, 1, 12);
        float time = _minTime + (_maxTime - _minTime) / (1 + a * Mathf.Pow(n - 1, p));
        return Mathf.CeilToInt(time);
    }

    public static Vector2 MoveToVaildPosition(Vector2 _originPos)
    {
        List<Vector2> vaildPos = new();

        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                if (i == 0 && j == 0) continue;

                int gridX = Mathf.RoundToInt(_originPos.x + i);
                int gridY = Mathf.RoundToInt(_originPos.y + j);
                Vector3Int targetPos = new Vector3Int(gridX, gridY, 0);
                if (TilemapManager.Get().CanWalkAtTile(targetPos))
                {
                    vaildPos.Add(new Vector2(targetPos.x, targetPos.y));
                }
            }
        }

        return vaildPos[Random.Range(0, vaildPos.Count - 1)];
    }

    public static float GetAccelerateBuildingParemter(Unit _unit,EnemyType _type)
    {
        if (!_unit.CompareTag("BlueUnit"))
        {
             switch (_type)
            {
                case EnemyType.Easy:
                    return .75f;
                case EnemyType.Medium:
                    return 1.25f;
                case EnemyType.Hard:
                    return 2f;
                default:
                    return 1f;
            }
        }
        return 1f;
    }
}
