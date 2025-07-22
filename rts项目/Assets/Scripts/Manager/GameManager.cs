using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using IUnit;

public class GameManager : SingletonManager<GameManager> 
{
    [Header("Selected Units")]
    public Unit ActiveUnit;
    public List<Unit> SelectedUnits;
    [SerializeField] private float detectedRadius = .3f;
    [SerializeField] private GameObject pointer;

    [Header("UI Parameters")]
    [SerializeField] private ActionBar ActionBar;
    [SerializeField] private PlaceBuildingUI PlaceBuildingUI;
    [SerializeField] private TrainingUnitUI TrainingUnitUI;
    [SerializeField] private TrainingUI TrainingUI;
    [SerializeField] private Image WiningUI;
    [SerializeField] private Image FaliureUI;

    [Header("Resource UI")]
    [SerializeField] private GameObject WoodCollection;
    [SerializeField] private GameObject MeatCollection;
    [SerializeField] private GameObject GoldCollection;
    [Header("Box Renderer")]
    [SerializeField] private LineRenderer BoxRenderer;
    private Vector3 StartPos;
    private bool IsDrawing = false;

    [Header("Registered Target")]
    public List<Unit> RegisteredUnits = new List<Unit>();

    [Header("Camera Options")]
    [SerializeField] private float PanSpeed;
    [SerializeField] private CameraBounds CameraBounds;
    [SerializeField] private Joystick joyStick;

    [Header("Resources Amount")]
    public int WoodAmount;
    public int GoldAmount;
    public int MeatAmount;

    [Header("Point Or Drag")]
    public float DragDuration = .3f;
    public float DragDistance = 4f;
    private Vector2 PointerDownPosition;
    private float PointerDownTime;
    private bool m_IsDrag;

    public UnityAction onResourcesChanged;
    private Vector2 m_MousePosition;
    private LineRenderer ActiveRay;
    private Dictionary<Unit, LineRenderer> ActionRays = new();
    private PlacementProcess m_PlacementProcess;
    private TilemapManager m_TilemapManager;
    private CameraController m_CameraController;
    private Coroutine recordUnWalkableNodes;

    private void Start()
    {
        m_TilemapManager = TilemapManager.Get();
        m_CameraController = new(PanSpeed, CameraBounds, joyStick);
        InitializeBoxRender();
    }
    private void Update()
    {
        m_CameraController.Update();
        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.Update();
            return;
        }

        DrawRectangle(); // 处理多个单位
        bool flowControl = HandleClick();
        if (!flowControl)
        {
            return;
        }
        UpdateMovementRay();
    }
    private bool HandleClick()
    {
        if (HvoUtils.IsCancleSelect() && !HvoUtils.IsPointerOverUIElement())
        {
           // Debug.Log("Cancle Selected.");
            ResetSelectedUnits();
            return false;
        }
        if (HvoUtils.IsPointerUp() && !HvoUtils.IsPointerOverUIElement())
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_MousePosition = mousePosition;
            HandleUnitBehaviour(mousePosition);
            // var path = m_TilemapManager.FindPath(ActiveUnit.transform.position, mousePosition);

            // foreach(var node in path)
            // {
            //    m_TilemapManager.SetTile(new Vector3Int(node.ButtomX,node.ButtomY));
            // }
        }

        return true;
    }

    #region Handle Click
    private void HandleUnitBehaviour(Vector2 _mousePosition) // 处理单个单位
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_mousePosition, detectedRadius);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Unit unit))
            {
                SelectNewUnit(unit,_mousePosition);
                return;
            }
        }
        HandleUnitsMovement(_mousePosition);

    }

    private void HandleUnitsMovement(Vector2 _mousePosition)
    {
        if (!IsDrawing)
        {
            if (SelectedUnits.Count > 0)
            {
                float radius = 1f; // 单位间最小半径
                int unitCount = SelectedUnits.Count;
                float angleStep = 360f / unitCount;
                for (int i = 0; i < unitCount; i++)
                {
                    var humanoid = SelectedUnits[i].GetComponent<HumanoidUnit>();
                    humanoid.UnassignTarget();
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                    Vector2 targetPosition = _mousePosition + offset;
                    humanoid.MoveToDestination(targetPosition);
                    GenerateFollowRay(targetPosition, Color.green);
                }
            }
            else
            {
                if (ActiveUnit != null && ActiveUnit.TryGetComponent(out HumanoidUnit humanoid))
                {
                    humanoid.UnassignTarget();
                    humanoid.MoveToDestination(_mousePosition);
                    GenerateFollowRay(_mousePosition, Color.green);
                }
            }
        }
        IsDrawing = false;
    }

    private void SelectNewUnit(Unit _unit,Vector2 _mousePosition)
    {
        //Debug.Log("Select New Unit Part 0.");
        // 处理工人
        bool flowControl = HandleWorkerTask(_unit);
        if (!flowControl)
        {
            return;
        }
        //Debug.Log("Select New Unit Part 1.");
        // 处理攻击的情况
        flowControl = HandleUnitsAttack(_unit, _mousePosition);
        if (!flowControl)
        {
            return;
        }

        if (HasSelectUnits() || ActiveUnit == _unit)
        {
            ResetSelectedUnits();
        }
        // Debug.Log("Select New Unit Part 2.");

        if ((_unit is StructureUnit house && !house.IsCompleted) || _unit.IsDead || IsDrawing)
        {
            return;
        }
        //  Debug.Log("Select New Unit Part 3.");
        ActiveUnit = _unit.CompareTag("BlueUnit") && !_unit.TryGetComponent(out TowerUnit _) ? _unit : null;

        if (ActiveUnit != null)
        {
            ActiveUnit.SelectedUnit();
            if (ActiveUnit.Actions.Count > 0)
            {
                ActionBar.ShowActionBar();
                foreach (var action in ActiveUnit.Actions)
                {
                    ActionBar.RegisterActionButton(action.Icon, () => action.ExecuteAction());
                }
            }
        }
    }

    private bool HandleWorkerTask(Unit _unit)
    {
        if (SelectedUnits.Count > 0)
        {
            var vaildWorkers = SelectedUnits.Where(unit => unit.TryGetComponent(out WorkerUnit worker) && worker.currentTask == WorkerTask.None).ToList();
            if (vaildWorkers.Count == 0)
            {
                return true;
            }

            if (_unit.TryGetComponent(out StructureUnit structure) && structure.IsUnderConstruction)
                {
                    foreach (var unit in vaildWorkers)
                    {
                        unit.GetComponent<WorkerUnit>().AssignTarget(structure);
                        unit.GetComponent<WorkerUnit>().UpdateWorkerTask(WorkerTask.Building);
                    }
                    return false;
                }
                else if (_unit.TryGetComponent(out GoldMinerUnit miner) && !miner.IsDead && miner.IsCompleted)
                {
                    foreach (var unit in vaildWorkers)
                    {
                        unit.GetComponent<WorkerUnit>().AssignTarget(miner);
                        unit.GetComponent<WorkerUnit>().UpdateWorkerTask(WorkerTask.Mining);
                    }
                    return false;
                }
        }
        else
        {
            if (ActiveUnit is WorkerUnit worker && worker.currentTask == WorkerTask.None)
            {
                if (_unit.TryGetComponent(out StructureUnit structure) && structure.IsUnderConstruction)
                {
                    worker.AssignTarget(structure);
                    worker.UpdateWorkerTask(WorkerTask.Building);
                    return false;
                }
                else if (_unit.TryGetComponent(out TreeUnit tree) && !tree.IsDead)
                {
                    tree.AssignWorker(worker);
                    worker.AssignTarget(tree);
                    worker.UpdateWorkerTask(WorkerTask.Chopping);
                    return false;
                }
                else if (_unit.TryGetComponent(out SheepUnit sheep) && !sheep.IsDead)
                {
                    sheep.AssignTarget(worker);
                    worker.AssignTarget(sheep);
                    worker.UpdateWorkerTask(WorkerTask.Killing);
                    return false;
                }
                else if (_unit.TryGetComponent(out GoldMinerUnit miner) && !miner.IsDead && miner.IsCompleted)
                {
                    worker.AssignTarget(miner);
                    worker.UpdateWorkerTask(WorkerTask.Mining);
                    return false;
                }
            }
        }

        return true;
    }

    private bool HandleUnitsAttack(Unit _unit, Vector2 _mousePosition)
    {
        if (!HasSelectUnits())
            return true;

        if (_unit.CompareTag("RedUnit") && !_unit.IsDead)
            {
            Debug.Log("Attack Target");
                GenerateFollowRay(_mousePosition, Color.red);
                if (SelectedUnits.Count > 0)
                {
                    foreach (var unit in SelectedUnits.Where(unit => unit != null && !unit.TryGetComponent(out WorkerUnit _) && !unit.IsDead))
                    {
                        unit.GetComponent<HumanoidUnit>().AssignTarget(_unit);
                    }
                    return false;
                }
                else
                {
                    if (ActiveUnit != null && (!ActiveUnit.TryGetComponent(out WorkerUnit _) || !ActiveUnit.TryGetComponent(out StructureUnit _)))
                    {
                        (ActiveUnit as HumanoidUnit).AssignTarget(_unit);
                        return false;
                    }
                }
            }

        return true;
    }

    private void SelectUnitsInRectangle(Vector3 _startPos, Vector3 _endPos)
    {
        ResetSelectedUnits();
        float minX = Mathf.Min(_startPos.x, _endPos.x);
        float maxX = Mathf.Max(_startPos.x, _endPos.x);
        float minY = Mathf.Min(_startPos.y, _endPos.y);
        float maxY = Mathf.Max(_startPos.y, _startPos.y);

        var vaildUnits = RegisteredUnits.Where(unit => unit.CompareTag("BlueUnit") && !unit.IsDead && unit.TryGetComponent(out HumanoidUnit _) && !unit.TryGetComponent(out TowerUnit _)).ToList();

        foreach (var unit in vaildUnits)
        {
            Vector3 screenPos = unit.transform.position;

            if (screenPos.x >= minX && screenPos.x <= maxX && screenPos.y >= minY && screenPos.y <= maxY)
            {
                //Debug.Log("Add Vaild Unit.");
                SelectedUnits.Add(unit);
                unit.SelectedUnit();
            }
        }
    }

    private void ResetSelectedUnits()
    {
         //Debug.Log("Reset Units.");
        if (SelectedUnits.Count > 0)
        {
            foreach (var unit in SelectedUnits.Where(unit => !unit.IsDead && unit != null))
            {
                unit.UnselectedUnit();
            }
        }

        SelectedUnits.Clear();
        if (ActiveUnit != null)
        {
            ActiveUnit.UnselectedUnit();
            ActiveUnit = null;
        }

        ActionBar.ClearAllActionButtons();
        ActionBar.HideActionBar();

    }

    private bool HasSelectUnits() => ActiveUnit != null || SelectedUnits.Count > 0;

    #endregion

    #region Ray Methods
    private void GenerateFollowRay(Vector2 _mousePosition, Color _color)
    {
        if (SelectedUnits.Count > 0)
        {
            ClearActionRays();
            foreach (var unit in SelectedUnits)
            {
                GameObject go = new GameObject("ActoinRay");
                LineRenderer line = go.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.sortingOrder = 50;
                line.startColor = _color;
                line.endColor = _color;
                line.startWidth = .07f;
                line.endWidth = .07f;

                ActionRays[unit] = line;
                StartCoroutine(HideRayAfterDelay(go));
                Instantiate(pointer, _mousePosition, Quaternion.identity);
            }
        }
        else
        {
            if (ActiveUnit == null || ActiveUnit.IsDead || !ActiveUnit.TryGetComponent(out HumanoidUnit _))
            {
                return;
            }

            GameObject go = new GameObject("MovementRay");
            ActiveRay = go.AddComponent<LineRenderer>();
            ActiveRay.material = new Material(Shader.Find("Sprites/Default"));
            ActiveRay.sortingOrder = 50;
            ActiveRay.startColor = _color;
            ActiveRay.endColor = _color;
            ActiveRay.startWidth = .07f;
            ActiveRay.endWidth = .07f;
            StartCoroutine(HideRayAfterDelay(go));
            Instantiate(pointer, _mousePosition, Quaternion.identity);
        }

    }

    private void DrawRectangle()
    {
        if (HvoUtils.IsPointerOverUIElement())
        {
            return;
        }

        if (HvoUtils.IsPointerDown())
            {
                PointerDownPosition = HvoUtils.GetPointerPositoin();
                PointerDownTime = Time.time;

                StartPos = GetWorldPosition();
                BoxRenderer.enabled = true;
            }

        if (HvoUtils.IsPointerPress())
        {
            Vector3 currentPos = GetWorldPosition();

            Vector3 p0 = StartPos;
            Vector3 p1 = new Vector3(StartPos.x, currentPos.y, 0);
            Vector3 p2 = currentPos;
            Vector3 p3 = new Vector3(currentPos.x, StartPos.y, 0);

            BoxRenderer.SetPosition(0, p0);
            BoxRenderer.SetPosition(1, p1);
            BoxRenderer.SetPosition(2, p2);
            BoxRenderer.SetPosition(3, p3);
            BoxRenderer.SetPosition(4, p0);
        }

        if (HvoUtils.IsPointerUp())
        {
            BoxRenderer.enabled = false;
            Vector3 currentPos = GetWorldPosition();

            if (Mathf.Abs(StartPos.x - currentPos.x) < 1f || Mathf.Abs(StartPos.y - currentPos.y) < 1f)
            {
                return;
            }

            float pointPosition = Vector2.Distance(PointerDownPosition, HvoUtils.GetPointerPositoin());
            float pointDuration = Time.time - PointerDownTime;
            m_IsDrag = pointPosition > DragDistance && pointDuration > DragDuration;
            if (m_IsDrag)
            {
                IsDrawing = true;
            }

            SelectUnitsInRectangle(StartPos, currentPos);
        }
    }

    private Vector3 GetWorldPosition()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        return worldPos;
    }

    private void UpdateMovementRay()
    {
        foreach (var pair in ActionRays)
        {
            Unit unit = pair.Key;
            LineRenderer line = pair.Value;

            if (unit != null && line != null)
            {
                line.positionCount = 2;
                line.SetPosition(0, unit.transform.position);
                line.SetPosition(1, m_MousePosition);
            }
        }
        if (ActiveUnit != null && ActiveRay != null)
        {
            ActiveRay.positionCount = 2;
            ActiveRay.SetPosition(0, ActiveUnit.transform.position);
            ActiveRay.SetPosition(1, m_MousePosition);
        }
    }

    private void ClearActionRays()
    {
        foreach (var pair in ActionRays)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        ActionRays.Clear();
    }

    private IEnumerator HideRayAfterDelay(GameObject _ray)
    {
        yield return new WaitForSeconds(.3f);
        Destroy(_ray);
        ActiveRay = null;
    }

    private void InitializeBoxRender()
    {
        BoxRenderer.positionCount = 5;
        BoxRenderer.material = new Material(Shader.Find("Sprites/Default"));
        BoxRenderer.startColor = Color.yellow;
        BoxRenderer.endColor = Color.yellow;
        BoxRenderer.startWidth = .1f;
        BoxRenderer.endWidth = .1f;
        BoxRenderer.loop = false;
        BoxRenderer.enabled = false;
    }

    #endregion

    #region Place Building Methods
    public void StartBuildingProcess(BuildingActionSO _action)
    {
        m_PlacementProcess = new(_action, m_TilemapManager);

        PlaceBuildingUI.ShowRectangle(_action.GoldCost, _action.WoodCost);
        PlaceBuildingUI.RegisterHooks(() => ConfirmPlacement(_action), CanclePlacement);
    }

    private void ConfirmPlacement(BuildingActionSO _action)
    {
        if (WoodAmount >= _action.WoodCost && GoldAmount >= _action.GoldCost)
        {
            var buildingAction = m_PlacementProcess.BuildingAction;

            if (buildingAction == null)
                return;

            if (m_PlacementProcess.CanPlaceBuilding(out Vector3 placePosition))
            {
                new BuildingProcess(buildingAction, placePosition);

                WoodAmount -= _action.WoodCost;
                GoldAmount -= _action.GoldCost;
                onResourcesChanged?.Invoke();
                AudioManager.Get().PlaySFX(11);
                ClearActionBarUI();
                ClearPlacement();
                if (recordUnWalkableNodes != null)
                {
                    StopCoroutine(recordUnWalkableNodes);
                }
                recordUnWalkableNodes = StartCoroutine(UpdateNodesCoroutine(placePosition, buildingAction));
                ResetSelectedUnits();
            }
        }

    }

    private IEnumerator UpdateNodesCoroutine(Vector3 _placePosition,BuildingActionSO _buildingAction)
    {
        yield return null;

        yield return new WaitForFixedUpdate();

        Vector3Int orientPosition = new Vector3Int(Mathf.FloorToInt(_placePosition.x + _buildingAction.BuildingOffset.x), Mathf.FloorToInt(_placePosition.y + _buildingAction.BuildingOffset.y), 0);
        m_TilemapManager.UpdateNodesInArea(orientPosition,_buildingAction.BuildingSize.x,_buildingAction.BuildingSize.y);
    }

    private void CanclePlacement()
    {
        ClearPlacement();
        ResetSelectedUnits();
        AudioManager.Get().PlaySFX(28);
    }

    public bool CanEnemyPlaceBuilding(BuildingActionSO _buildingAction, Vector3 _placePosition)
    {
        var placementProcess = new PlacementProcess(_buildingAction, m_TilemapManager, true);

        if (placementProcess.CanPlaceBuilding(_placePosition))
        {
            StartCoroutine(UpdateNodesCoroutine(_placePosition,_buildingAction));
            return true;
        }
        return false;
    }

    #endregion

    #region UI Methods
    private void ClearActionBarUI()
    {
        ActionBar.ClearAllActionButtons();
        ActionBar.HideActionBar();
    }
    private void ClearPlacement()
    {
        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.ClearupPlacement();
            m_PlacementProcess = null;
            PlaceBuildingUI.HideRectangle();
        }
    }

    public void CollectResource(ResourceType _type, int _amount, Vector3 _startPos)
    {
        AudioManager.Get().PlaySFX(31);
        GameObject newImage = null;
        string sb = "+ " + _amount.ToString();

        switch (_type)
        {
            case ResourceType.wood:
                newImage = Instantiate(WoodCollection, _startPos, Quaternion.identity);
                WoodAmount += _amount;
                break;
            case ResourceType.meat:
                newImage = Instantiate(MeatCollection, _startPos, Quaternion.identity);
                MeatAmount += _amount;
                break;
            case ResourceType.gold:
                newImage = Instantiate(GoldCollection, _startPos, Quaternion.identity);
                GoldAmount += _amount;
                break;
            default:
                return;
        }
        newImage.GetComponentInChildren<TextMeshProUGUI>().text = sb;
        newImage.transform.DOMove(_startPos + new Vector3(0, 2f, 0), 1.2f).SetEase(Ease.Linear).OnComplete(() => Destroy(newImage.gameObject));

        onResourcesChanged?.Invoke();
    }
    public void CollectWood(int _woodCount, Vector3 _startPos) => CollectResource(ResourceType.wood,_woodCount,_startPos);
    public void CollectMeat(int _meatCount,Vector3 _startPos) => CollectResource(ResourceType.meat,_meatCount,_startPos);
    public void CollectGold(int _goldCount, Vector3 _startPos) => CollectResource(ResourceType.gold,_goldCount,_startPos);

    #endregion

    #region Training Unit Methods
    public void StartTrainingProcess(TrainingActionSO _trainingAction)
    {
        TrainingUnitUI.ShowRectangle(_trainingAction.GoldCost, _trainingAction.MeatCost);
        TrainingUnitUI.RegisterHooks(() => ConfirmTraining(_trainingAction), CancleTraining);
    }

    private void ConfirmTraining(TrainingActionSO _trainingAction)
    {
        if (GoldAmount >= _trainingAction.GoldCost && MeatAmount >= _trainingAction.MeatCost)
        {
            GoldAmount -= _trainingAction.GoldCost;
            MeatAmount -= _trainingAction.MeatCost;
            onResourcesChanged();
        }
        else
        {
            return;
        }
        AudioManager.Get().PlaySFX(27);
        TrainingUI.RegisterTrainingUnit(_trainingAction.UnitType, _trainingAction.TrainingTime, ActiveUnit as StructureUnit, _trainingAction.UnitPrefab);
    }

    public void CancleTraining()
    {
        TrainingUnitUI.HideRectangle();
        AudioManager.Get().PlaySFX(28);
        ClearActionBarUI();
    }

    #endregion

    public void RegisterUnit(Unit _unit)
    {
        RegisteredUnits.Add(_unit);
    }

    public void RemoveUnit(Unit _unit)
    {
        RegisteredUnits.Remove(_unit);

        bool HasActiveBlueBuilding = RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.TryGetComponent(out StructureUnit _) && unit.CompareTag("BlueUnit")).Any();
        bool HasActiveRedBuilding = RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.TryGetComponent(out StructureUnit _) && unit.CompareTag("RedUnit")).Any();

        if (!HasActiveBlueBuilding)
        {
            FaliureUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => !unit.IsDead && unit.CompareTag("BlueUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }

        if (!HasActiveRedBuilding)
        {
            WiningUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => unit.CompareTag("RedUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

}
