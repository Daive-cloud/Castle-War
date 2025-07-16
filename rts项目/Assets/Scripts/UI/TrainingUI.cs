using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public enum TrainingUnitType
{
    Warrior,
    Archer,
    Worker,
    Torch,
    Demolisher,
    Barrel
}

public class TrainingUI : MonoBehaviour
{
    [SerializeField] private RectTransform PartForm;
    [Header("UI Elements")]
    [SerializeField] private GameObject WarriorSlot;
    [SerializeField] private GameObject ArcherSlot;
    [SerializeField] private GameObject WorkerSlot;
    [SerializeField] private GameObject TorchSlot;
    [SerializeField] private GameObject DemolisherSlot;
    [SerializeField] private GameObject BarrelSlot;

    private Queue<GameObject> TrainingUnits;
    private Queue<GameObject> TrainingSlots;
    private Queue<float> TrainingTime;
    private Queue<StructureUnit> TrainingBarracks;

    private GameObject newSlot;
    private float trainingTimer;
    private float fixedTimer;
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

        trainingTimer = TrainingTime.Dequeue();
        fixedTimer = 0f;
        var slot = TrainingSlots.Dequeue();

        while (fixedTimer <= trainingTimer)
        {
            yield return null;
            slot.GetComponentInChildren<Slider>().value = (float)fixedTimer / trainingTimer;
            fixedTimer += Time.deltaTime;
        }
        
        Destroy(slot);
        var barrack = TrainingBarracks.Dequeue();
        if(barrack == null || barrack.IsDead)
        {
            yield break;
        }

        var unit = TrainingUnits.Dequeue();
        GameObject newUnit = Instantiate(unit,barrack.transform.position,Quaternion.identity);
        var destination = Destination[Random.Range(0,Destination.Length-1)];
        Vector2 targetPos = barrack.transform.position + destination * 2f;

        if (newUnit.TryGetComponent(out BarrelUnit barrel))
        {
            barrel.SelectedUnit();
            barrel.transform.DOMove(targetPos, 2f).OnComplete(() => barrel.UnselectedUnit());
        }
        else
        {
            newUnit.GetComponent<HumanoidUnit>().MoveToDestination(targetPos);
        }

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
            case TrainingUnitType.Torch:
                newSlot = Instantiate(TorchSlot,PartForm);
                break;
            case TrainingUnitType.Demolisher:
                newSlot = Instantiate(DemolisherSlot,PartForm);
                break;
            case TrainingUnitType.Barrel:
                newSlot = Instantiate(BarrelSlot,PartForm);
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
