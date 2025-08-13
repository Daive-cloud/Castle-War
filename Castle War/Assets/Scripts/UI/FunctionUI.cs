using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FunctionUI : MonoBehaviour
{
    [SerializeField] private Image tip;
    private GameManager manager;

    void Start()
    {
        manager = GameManager.Get();
        manager.onSelectionFinished += HideTip;
    }

    public void PauseGame()
    {
        manager.PauseGame();
    }

    public void MultiplySelection()
    {
        manager.IsDrawing = true;
        tip.gameObject.SetActive(true);
    }

    public void CancleSelection()
    {
        manager.ResetSelectedUnits();
    }

    public void AllSelection()
    {
        manager.ResetSelectedUnits();
        var vaildUnits = manager.RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.CompareTag("BlueUnit") && unit.TryGetComponent(out HumanoidUnit _) && !unit.TryGetComponent(out WorkerUnit _) && !unit.TryGetComponent(out TowerUnit _)).ToList();
        foreach (var unit in vaildUnits)
        {
            Debug.Log($"unit : {unit}");
            manager.SelectedUnits.Add(unit);
            unit.SelectedUnit();
        }
    }
    public void CommandWorker()
    {
        var freeWorkers = manager.RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.CompareTag("BlueUnit") && unit.TryGetComponent(out WorkerUnit worker) && worker.currentTask == WorkerTask.None).ToList();
        var house = manager.RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.CompareTag("BlueUnit") && unit.TryGetComponent(out StructureUnit structure) && !structure.IsCompleted).FirstOrDefault();
        if (house != null && freeWorkers.Count > 0)
        {
            foreach (var unit in freeWorkers)
            {
                var worker = unit.GetComponent<WorkerUnit>();
                worker.AssignTarget(house);
                worker.UpdateWorkerTask(WorkerTask.Building);
            }
        }
    }

    public void BackToHome()
    {
        var castle = manager.RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.CompareTag("BlueUnit") && unit.TryGetComponent(out CastleUnit _)).FirstOrDefault();
        if (castle != null)
        {
            var position = castle.transform.position;
            Camera.main.transform.position = new Vector3(position.x,position.y,Camera.main.transform.position.z);
        }
    }

    private void HideTip() => tip.gameObject.SetActive(false);


}
