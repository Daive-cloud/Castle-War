using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class EnemyAI : MonoBehaviour
{
    public StructureUnit MainCastle;
    public List<WorkerUnit> Workers;
    public List<HumanoidUnit> ActiveArmy;
    private GameManager m_GameManger;
    private List<Vector3> m_PlacementGrid;
    private Queue<TrainingActionSO> m_TrainingActions;
    private Queue<StructureUnit> m_TrainingBarracks;
    private bool IsTraining = false;

    [Header("EnemyAIStage")]
    public List<EnemyAIStageSO> EnemyAIStages;
    [SerializeField] private float EnemyCheckFrequency;
    [SerializeField] private float EnemyRefillFrequency;
    private EnemyAIStageSO currentStage;
    private StructureUnit currentStageStructure;
    private float EnemyCheckTimer;
    private float EnemyRefillTimer;

    private void Start()
    {
        m_GameManger = GameManager.Get();

        m_TrainingBarracks = new();
        m_TrainingActions = new();

        RefillActiveUnits();

        GetRandomStage();
        EnterNextStage();
    }

    private void Update()
    {
        if (Time.time - EnemyCheckTimer >= EnemyCheckFrequency)
        {
            EnemyCheckTimer = Time.time;
            
            // 判断能否进入下一个阶段
            if (currentStageStructure != null && currentStageStructure.IsCompleted)
            {
                EnterNextStage();
            }
            if (!currentStageStructure.HasAssignedWorker)
            {
                CommandWorkersToBuild(currentStageStructure);
            }

            // 填充军队
                if (Time.time - EnemyRefillTimer >= EnemyRefillFrequency)
                {
                    EnemyRefillTimer = Time.time;
                    RefillActiveUnits();
                }

            // 组织现有军队发起进攻
            if (ActiveArmy.Count > 0)
            {
                ActiveArmy = ActiveArmy.Where(unit => unit != null && !unit.IsDead).ToList();
                foreach (var unit in ActiveArmy)
                {
                    unit.FindClosestEnemyWithoutRange();
                }
            }
        }

        if (!IsQueueVaild() || IsTraining)
        {
            return;
        }
        StartCoroutine(StartTrainingProcess());
    }

    private void EnterNextStage()
    {
        EnemyPlaceBuilding(currentStage.BuildingAction);
        foreach (var action in currentStage.TrainingActions)
        {
            EnemyTrainingUnit(action);
        }
        GetRandomStage();
    }

    private void RefillActiveUnits()
    {
        var allUnits = FindObjectsOfType<HumanoidUnit>().Where(unit => unit != null && !unit.IsDead && unit.CompareTag("RedUnit") && !unit.TryGetComponent(out TowerUnit _)).ToList();
        Workers.Clear();
        ActiveArmy.Clear();

        foreach (var unit in allUnits)
        {
            if (unit.TryGetComponent<WorkerUnit>(out var worker) && worker.currentTask == WorkerTask.None)
            {
                Workers.Add(worker);
            }
        }
        ActiveArmy = allUnits.Where(unit => !unit.TryGetComponent(out WorkerUnit _)).ToList();
    }

    private IEnumerator StartTrainingProcess()
    {
        IsTraining = true;
        var trainingAction = m_TrainingActions.Dequeue();
        float time = trainingAction.TrainingTime;
        float actualTime = HvoUtils.ComputeTrainingTime(FindBarracksCount, time, time / 4);
//        Debug.Log($"Actual Training Time : {actualTime}.");

        yield return new WaitForSeconds(actualTime);
        var unit = trainingAction.UnitPrefab;
        var barrack = m_TrainingBarracks.Dequeue();
        GameObject newUnit = Instantiate(unit, barrack.transform.position, Quaternion.identity);

        var targetPos = HvoUtils.MoveToVaildPosition(barrack.transform.position);
        newUnit.GetComponent<HumanoidUnit>().MoveToDestination(targetPos);

        // if (newUnit.TryGetComponent(out WorkerUnit worker))
        // {
        //     var task = Random.Range(0, 100) <= 20 ? WorkerTask.Chopping : WorkerTask.Mining;
        // }

        IsTraining = false;
    }

    private bool IsQueueVaild() => m_TrainingBarracks.Count > 0 && m_TrainingActions.Count > 0;

    private void EnemyPlaceBuilding(BuildingActionSO _buildingAction)
    {
        if (MainCastle == null || MainCastle.IsDead)
        {
            return;
        }

        m_PlacementGrid = new();

        for (int i = -7; i <= 7; i++)
        {
            for (int j = -7; j <= 7; j++)
            {
                var placePosition = MainCastle.transform.position + new Vector3(i, j, 0);

                if (m_GameManger.CanEnemyPlaceBuilding(_buildingAction, placePosition))
                {
                    m_PlacementGrid.Add(placePosition);
                }
            }
        }
        if (m_PlacementGrid.Count == 0)
        {
            return;
        }

        var finalPosition = m_PlacementGrid[Random.Range(0, m_PlacementGrid.Count - 1)];

        new BuildingProcess(_buildingAction, finalPosition, out var structure);
        //      Debug.Log($"Place Sturcture : {structure}");
        currentStageStructure = structure;
        CommandWorkersToBuild(structure);
    }

    private void CommandWorkersToBuild(StructureUnit structure)
    {
        foreach (var unit in Workers)
        {
            unit.AssignTarget(structure);
            unit.UpdateWorkerTask(WorkerTask.Building);
        }
    }

    private void EnemyTrainingUnit(TrainingActionSO _trainingAction)
    {
        var barracks = FindObjectsOfType<BarrackUnit>().Where(unit => !unit.IsDead && unit.CompareTag("RedUnit") && !unit.IsUnderConstruction).ToList();
        if (barracks.Count == 0)
        {
            return;
        }
        var barrack = barracks[Random.Range(0, barracks.Count - 1)];

        m_TrainingActions.Enqueue(_trainingAction);
        m_TrainingBarracks.Enqueue(barrack);
    }

    private int FindBarracksCount => FindObjectsOfType<BarrackUnit>().Where(unit => unit.CompareTag("RedUnit") && unit.IsCompleted && !unit.IsDead).ToList().Count;

    private void GetRandomStage() => currentStage = EnemyAIStages[Random.Range(0,EnemyAIStages.Count - 1)];
    
}
