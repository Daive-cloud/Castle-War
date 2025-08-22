using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnemyColor
{
    Red,
    Yellow,
    Purple
}

public class EnemyAgent : MonoBehaviour
{
    public StructureUnit MainCastle => FindObjectsOfType<CastleUnit>().Where(unit => !unit.IsDead && unit.CompareTag(ColorToTag()) && unit.IsCompleted).FirstOrDefault();
    public List<WorkerUnit> Workers;
    public List<HumanoidUnit> ActiveArmy;
    private GameManager m_GameManager;
    private SettingsManager m_SettingsManager;
    private List<Vector3> m_PlacementGrid;
    private Queue<TrainingActionSO> m_TrainingActions;
    private Queue<StructureUnit> m_TrainingBarracks;
    private int ArmyAmount = 10;

    private bool IsTraining = false;
    [Header("EnemyAIStage")]
    public List<EnemyAIStageSO> EnemyAIStages;
    [SerializeField] private float EnemyCheckFrequency;
    [SerializeField] private float EnemyRefillFrequency;
    private EnemyAIStageSO currentStage;
    private StructureUnit currentStageStructure;
    private float EnemyCheckTimer;
    private float EnemyRefillTimer;
    private float limitedCheckFrequency = 60f;
    private float lastCheckTimer;
    private float currentStageTimer;
    private float currentStageDuration;
    public EnemyColor enemyColor = EnemyColor.Red;

    private void Start()
    {

        m_GameManager = GameManager.Get();
        m_SettingsManager = SettingsManager.Get();
        IsEnemyChoosed();

        m_TrainingBarracks = new();
        m_TrainingActions = new();

        RefillActiveUnits();

        GetRandomStage();
        ImplementCurrentStage();
    }

    private void Update()
    {
        if (Time.time - EnemyCheckTimer >= EnemyCheckFrequency)
        {
            EnemyCheckTimer = Time.time;
            //            Debug.Log($"CurrentStageBuilding : {currentStageStructure}");
            // 判断能否进入下一个阶段
            if (IsCurrentStageEnded())
            {
                ImplementCurrentStage();
            }
            if (currentStageStructure != null && !currentStageStructure.IsCompleted && !currentStageStructure.HasAssignedWorker)
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
                ArmyAmount = ActiveArmy.Count;
                ActiveArmy = ActiveArmy.Where(unit => unit != null && !unit.IsDead).ToList();
                foreach (var unit in ActiveArmy)
                {
                    unit.EnemyAIFindTarget();
                }
            }
        }

        if (!IsQueueVaild() || IsTraining)
        {
            return;
        }
        StartCoroutine(StartTrainingProcess());
    }

    private void ImplementCurrentStage()
    {
        lastCheckTimer = Time.time;
        currentStageTimer = Time.time;
        EnemyPlaceBuilding(currentStage.BuildingAction);
        SelectTrainingBarrack(out BarrackUnit barrack);
        foreach (var action in currentStage.TrainingActions)
        {
            AddTrainingAction(action,barrack);
        }
        GetRandomStage();
    }

    private void RefillActiveUnits()
    {
        var tag = ColorToTag();
        var allUnits = FindObjectsOfType<HumanoidUnit>().Where(unit => unit != null && !unit.IsDead && unit.CompareTag(tag) && !unit.TryGetComponent(out TowerUnit _)).ToList();
        Workers.Clear();
        ActiveArmy.Clear();

        foreach (var unit in allUnits)
        {
            if (unit.TryGetComponent<WorkerUnit>(out var worker))
            {
                Workers.Add(worker);
            }
        }
        var units = allUnits.Where(unit => !unit.TryGetComponent(out WorkerUnit _)).ToList(); // see you tomorow
        if (units.Count >= ArmyAmount)
        {
            ArmyAmount = units.Count;
            ActiveArmy = units;
        }
    }

    private IEnumerator StartTrainingProcess()
    {
        IsTraining = true;
        var trainingAction = m_TrainingActions.Dequeue();
        float time = trainingAction.TrainingTime;
        float actualTime = HvoUtils.ComputeTrainingTime(FindBarracksCount, time, time / 4) * TrainingParamter(GetEnemyType());

        yield return new WaitForSeconds(actualTime);
        var unit = trainingAction.UnitPrefab;

        StructureUnit barrack = null;
        do
        {
            if (m_TrainingBarracks.Count == 0)
                break;
            barrack = m_TrainingBarracks.Dequeue();
        } while (barrack == null);

        if (barrack != null)
        {
            var targetPos = HvoUtils.MoveToVaildPosition(barrack.transform.position);
            if (targetPos != Vector2.zero)
            {
                var newUnit = Instantiate(unit, barrack.transform.position, Quaternion.identity);
                newUnit.GetComponent<HumanoidUnit>().MoveToDestination(targetPos);
                if (newUnit.TryGetComponent(out BarrelUnit barrel))
                {
                    barrel.SelectedUnit();
                }
            }
           
        }



        // if (newUnit.TryGetComponent(out WorkerUnit worker))
        // {
        //     var task = Random.Range(0, 100) <= 20 ? WorkerTask.Chopping : WorkerTask.Mining;
        // }

        IsTraining = false;
    }

    private bool IsQueueVaild() => m_TrainingBarracks.Count > 0 && m_TrainingActions.Count > 0;

    private void EnemyPlaceBuilding(BuildingActionSO _buildingAction)
    {
        //        Debug.Log($"Main Castle : {MainCastle}");
        if (MainCastle == null || MainCastle.IsDead)
        {
            Debug.Log("Not Found Castle.");
            return;
        }

        m_PlacementGrid = new();

        for (int i = -6; i <= 6; i++)
        {
            for (int j = -6; j <= 6; j++)
            {
                var placePosition = MainCastle.transform.position + new Vector3(i, j, 0);

                if (m_GameManager.CanEnemyPlaceBuilding(_buildingAction, placePosition))
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
        StartCoroutine(CommandWorkersToBuild(structure));
    }

    private IEnumerator CommandWorkersToBuild(StructureUnit structure)
    {
        yield return new WaitForSeconds(1.2f);

        foreach (var unit in Workers)
        {
            unit.AssignTarget(structure);
            unit.UpdateWorkerTask(WorkerTask.Building);
        }
    }

    private void AddTrainingAction(TrainingActionSO _trainingAction,BarrackUnit _barrack)
    {
        if (_barrack == null)
            return;

        m_TrainingActions.Enqueue(_trainingAction);
        m_TrainingBarracks.Enqueue(_barrack);
    }

    private bool SelectTrainingBarrack(out BarrackUnit barrack)
    {
        var barracks = FindObjectsOfType<BarrackUnit>().Where(unit => !unit.IsDead && unit.CompareTag(ColorToTag()) && !unit.IsUnderConstruction).ToList();
        if (barracks.Count == 0)
        {
            barrack = null;
            return false;
        }
        int leastNodeCount = int.MaxValue;
        barrack = barracks[0];
        foreach (var unit in barracks)
        {
            var node = TilemapManager.Get().FindNode(unit.transform.position);
            var pos = node.GetPosition();
            int unwalkableNodeCount = 0;
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    var nodePos = pos + new Vector2(i, j);
                    var currentNode = TilemapManager.Get().FindNode(nodePos);
                    if (!currentNode.IsWalkable)
                    {
                        unwalkableNodeCount++;
                    }
                }
            }
            if (unwalkableNodeCount < leastNodeCount)
            {
                leastNodeCount = unwalkableNodeCount;
                barrack = unit;
            }
        }

        return true;
    }

    private int FindBarracksCount => FindObjectsOfType<BarrackUnit>().Where(unit => unit != null && unit.CompareTag(ColorToTag()) && unit.IsCompleted && !unit.IsDead).ToList().Count;

    private void GetRandomStage()
    {
        currentStage = EnemyAIStages[Random.Range(0, EnemyAIStages.Count - 1)];
        currentStageDuration = currentStage.StageExistTimer;
    }

    private bool ExceedTimer() => Time.time - lastCheckTimer >= limitedCheckFrequency;
    private bool IsCurrentStageTimeEnded() => Time.time - currentStageTimer >= currentStageDuration;

    public float TrainingParamter(EnemyType _type)
    {
        switch (_type)
        {
            case EnemyType.Easy:
                return 1.5f;
            case EnemyType.Medium:
                return 1f;
            case EnemyType.Hard:
                return .5f;
            default:
                return 1f;
        }
    }

    private float StageExistParamter(EnemyType _type)
    {
        switch (_type)
        {
            case EnemyType.Easy:
                return 1f;
            case EnemyType.Medium:
                return .75f;
            case EnemyType.Hard:
                return .5f;
            default:
                return 1f;
        }
    }

    private bool IsCurrentStageEnded() => (currentStageStructure != null && currentStageStructure.IsCompleted && IsCurrentStageTimeEnded()) || ExceedTimer();

    public string ColorToTag()
    {
        return enemyColor.ToString() + "Unit";
    }

    private void IsEnemyChoosed()
    {
        switch (enemyColor)
        {
            case EnemyColor.Red:
                if (m_SettingsManager.enemyTypes[1] == EnemyType.None)
                    gameObject.SetActive(false);
                break;
            case EnemyColor.Yellow:
                if (m_SettingsManager.enemyTypes[2] == EnemyType.None)
                    gameObject.SetActive(false);
                break;
            case EnemyColor.Purple:
                if (m_SettingsManager.enemyTypes[3] == EnemyType.None)
                    gameObject.SetActive(false);
                break;
        }
    }

    private EnemyType GetEnemyType()
    {
        switch (enemyColor)
        {
            case EnemyColor.Red:
                return m_SettingsManager.enemyTypes[1];
            case EnemyColor.Yellow:
                return m_SettingsManager.enemyTypes[2];
            case EnemyColor.Purple:
                return m_SettingsManager.enemyTypes[3];
            default:
                return EnemyType.None;
        }
    }
}
