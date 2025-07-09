using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public enum TrainingUnitType
{
    Warrior,
    Archer,
    Worker
}

public class TrainingUI : MonoBehaviour
{
    [SerializeField] private RectTransform PartForm;
    [Header("UI Elements")]
    [SerializeField] private GameObject WarriorSlot;
    [SerializeField] private GameObject ArcherSlot;
    [SerializeField] private GameObject WorkerSlot;

    private Queue<GameObject> TrainingUnits;
    private Queue<GameObject> TrainingSlots;
    private Queue<float> TrainingTime;
    private Queue<StructureUnit> TrainingBarracks;

    private GameObject newSlot;
    private float TrainingTimer;
    private bool IsTraining = false;

    private Vector3[] Destination = new Vector3[] { Vector3.up,Vector3.left,Vector3.right,Vector3.down};

    private void Start()
    {
        TrainingTime = new();
        TrainingSlots = new();
        TrainingBarracks = new();
        TrainingUnits = new();
    }

    private void Update()
    {
        if(!IsQueueVaild() || IsTraining)
        {
            return;
        }

        StartCoroutine(StartTrainingProcess());
    }

    private IEnumerator StartTrainingProcess()
    {
        IsTraining = true;

        TrainingTimer = TrainingTime.Dequeue();
        yield return new WaitForSeconds(TrainingTimer);

        var slot = TrainingSlots.Dequeue();
        Destroy(slot);
        var barrack = TrainingBarracks.Dequeue();
        if(barrack == null || barrack.IsDead)
        {
            yield break;
        }

        var unit = TrainingUnits.Dequeue();
        GameObject newUnit = Instantiate(unit,barrack.transform.position,Quaternion.identity);
        var destination = Destination[Random.Range(0,Destination.Length-1)];
        newUnit.GetComponent<HumanoidUnit>().MoveToDestination(barrack.transform.position + destination * 1.5f);

        IsTraining = false;
    }

    private bool IsQueueVaild() => TrainingSlots.Count > 0 && TrainingTime.Count > 0 && TrainingBarracks.Count > 0 && TrainingUnits.Count > 0;

    public void RegisterTrainingUnit(TrainingUnitType _unitType,float _trainingTime,StructureUnit _barrack,GameObject _unit)
    {
        switch(_unitType)
        {
            case TrainingUnitType.Warrior:
                newSlot = Instantiate(WarriorSlot,PartForm);
                break;
            case TrainingUnitType.Archer:
                newSlot = Instantiate(ArcherSlot, PartForm);
                break;
            case TrainingUnitType.Worker:
                newSlot = Instantiate(WorkerSlot, PartForm);
                break;
            default:
                return;
        }
        TrainingSlots.Enqueue(newSlot);
        TrainingUnits.Enqueue(_unit);
        TrainingTime.Enqueue(_trainingTime);
        TrainingBarracks.Enqueue(_barrack);
    }
}
