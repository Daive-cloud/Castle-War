using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class EnemyAI : MonoBehaviour
{
    public StructureUnit MainCastle;
    public WorkerUnit Worker;
    public List<HumanoidUnit> ActiveUnits;

    private GameManager m_GameManger;
    private List<Vector3> m_PlacementGrid;

    private Queue<TrainingActionSO> m_TrainingActions;
    private Queue<StructureUnit> m_TrainingBarracks;

    private bool IsTraining = false;

    [Header("EnemyAIStage")]
    public List<EnemyAIStageSO> EnemyAIStages;
    [SerializeField] private float EnemyCheckFrequency;
    [SerializeField] private float EnemyRefillFrequency;
    private float StageUpdateFruency;

    private float EnemyCheckTimer;
    private float EnemyRefillTimer;
    private float StageUpdateTimer;

    private int CurrentStageIndex = 0;
    

    private void Start()
    {
        m_GameManger = GameManager.Get();

        m_TrainingBarracks = new();
        m_TrainingActions = new();
    }

    private void Update()
    {
        if(Time.time - EnemyCheckTimer >= EnemyCheckFrequency)
        {
            EnemyCheckTimer = Time.time;

            if(Time.time - StageUpdateTimer >= StageUpdateFruency)
            {
                StageUpdateTimer = Time.time;
                if(CurrentStageIndex + 1 >= EnemyAIStages.Count)
                {
                    return;
                }

                var stage = EnemyAIStages[CurrentStageIndex];
                StageUpdateFruency = stage.StageExistWindow;

                EnemyPlaceBuilding(stage.BuildingAction); // 自动放置建筑
                foreach(var action in stage.TrainingActions)
                {
                    EnemyTrainingUnit(action);
                }
                CurrentStageIndex++;
            }
           
            if(Time.time - EnemyRefillTimer >= EnemyRefillFrequency)
            {
                EnemyRefillTimer = Time.time;
                RefillActiveUnits();
            }
            if(ActiveUnits.Count > 0)
            {
                ActiveUnits = ActiveUnits.Where(unit => unit != null && !unit.IsDead).ToList();
                foreach(var unit in ActiveUnits)
                {
                    unit.FindClosestEnemyWithoutRange();
                }
            }
        }

        if(!IsQueueVaild() || IsTraining)
        {
            return;
        }
        StartCoroutine(StartTrainingProcess());
    }

    private void RefillActiveUnits()
    {
        ActiveUnits = FindObjectsOfType<HumanoidUnit>().Where(unit => unit != null && !unit.IsDead && unit.CompareTag("RedUnit") && !unit.TryGetComponent(out WorkerUnit _)).ToList();  
    }

    private IEnumerator StartTrainingProcess()
    {
        IsTraining = true;
        var trainingAction = m_TrainingActions.Dequeue();
        float time = trainingAction.TrainingTime;

        yield return new WaitForSeconds(time);
        var unit = trainingAction.UnitPrefab;
        var barrack = m_TrainingBarracks.Dequeue();
        GameObject newUnit = Instantiate(unit,barrack.transform.position,Quaternion.identity);
        newUnit.GetComponent<HumanoidUnit>().MoveToDestination(barrack.transform.position + Vector3.down * 3);

        IsTraining = false;
    }

    private bool IsQueueVaild() => m_TrainingBarracks.Count > 0 && m_TrainingActions.Count > 0;

    private void EnemyPlaceBuilding(BuildingActionSO _buildingAction)
    {
        m_PlacementGrid = new();

        for(int i=-7;i<=7;i++)
        {
            for(int j=-7;j<=7;j++)
            {
                var placePosition = MainCastle.transform.position + new Vector3(i,j,0);

                if (m_GameManger.CanEnemyPlaceBuilding(_buildingAction, placePosition))
                {
                    m_PlacementGrid.Add(placePosition);
                }
            }
        }
        if(m_PlacementGrid.Count == 0)
        {
            return;
        }

        var finalPosition = m_PlacementGrid[Random.Range(0,m_PlacementGrid.Count -1)];

        new BuildingProcess(_buildingAction,finalPosition,out var structure);
        Worker.AssignTarget(structure);
        Worker.currentTask = WorkerTask.Building;
    }

    private void EnemyTrainingUnit(TrainingActionSO _trainingAction)
    {
        var barracks = FindObjectsOfType<BarrackUnit>().Where(unit => !unit.IsDead && unit.CompareTag("RedUnit") && !unit.IsUnderConstruction).ToList();
        if(barracks.Count == 0)
        {
            return;
        }
        var barrack = barracks[Random.Range(0,barracks.Count -1)];

        m_TrainingActions.Enqueue(_trainingAction);
        m_TrainingBarracks.Enqueue(barrack);
    }
}
