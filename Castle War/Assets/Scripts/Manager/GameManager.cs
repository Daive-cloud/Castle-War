using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using IUnit;
using System;

public class GameManager : SingletonManager<GameManager>
{
    #region Paramters
    [Header("Selected Units")]
    public Unit ActiveUnit;
    public List<Unit> SelectedUnits;
    [SerializeField] private float detectedRadius = .3f;
    [SerializeField] private GameObject Pointer;
    [Header("Pool Object")]
    public GameObject damageFont;
    public GameObject arrow;
    public GameObject firecraker;
    public GameObject bigArrow;
    public GameObject healEffect;
    public GameObject magicExplosion;

    [Header("UI Parameters")]
    [SerializeField] private ActionBar ActionBar;
    [SerializeField] private PlaceBuildingUI PlaceBuildingUI;
    [SerializeField] private TrainingUnitUI TrainingUnitUI;
    [SerializeField] private TrainingUI TrainingUI;
    [SerializeField] private Image WiningUI;
    [SerializeField] private Image FaliureUI;
    [SerializeField] private Image OptionsUI;

    [Header("Resource UI")]
    [SerializeField] private GameObject WoodCollection;
    [SerializeField] private TextMeshProUGUI WoodProduction;
    private Queue<(float time, int amount)> producedWoodHistroy = new();
    [SerializeField] private GameObject MeatCollection;
    [SerializeField] private TextMeshProUGUI MeatProduction;
    private Queue<(float time, int amount)> producedMeatHistroy = new();
    [SerializeField] private GameObject GoldCollection;
    [SerializeField] private TextMeshProUGUI GoldProduction;
    private Queue<(float time, int amount)> producedGoldHistroy = new();
    [SerializeField] private GameObject CrystalCollection; 
    private float timeWindow = 60f;
    private float lastUpdateTimer;
    [Header("Box Renderer")]
    private LineRenderer BoxRenderer;
    private Vector3 StartPos;
    public bool IsDrawing = false;

    [Header("Registered Target")]
    public List<Unit> RegisteredUnits = new List<Unit>();

    [Header("Camera Config")]
    [SerializeField] private float PanSpeed;
    [SerializeField] private float ZoomSpeed;
    [SerializeField] private float MinZoom;
    [SerializeField] private float MaxZoom;
    [SerializeField] private CameraBounds CameraBounds;

    [Header("Resources Amount")]
    public int WoodAmount;
    public int GoldAmount;
    public int MeatAmount;
    public int CrystalAmount;

    [Header("Point Or Drag")]
    public float DragDuration = .3f;
    public float DragDistance = 4f;
    private Vector2 PointerDownPosition;
    private float PointerDownTime;
    private bool m_IsDrag;

    public UnityAction onResourcesChanged;
    public UnityAction onSelectionFinished;
    private Vector2 m_MousePosition;
    private LineRenderer ActiveRay;
    private Dictionary<Unit, LineRenderer> ActionRays = new();
    private PlacementProcess m_PlacementProcess;
    private TilemapManager m_TilemapManager;
    private CameraController m_CameraController;
    private Coroutine recordUnWalkableNodes;
    #endregion
    private void Start()
    {
        m_TilemapManager = TilemapManager.Get();
        m_CameraController = new(PanSpeed, ZoomSpeed, MinZoom, MaxZoom, CameraBounds);

        InitializeGame();

        Time.timeScale = 1f;
        lastUpdateTimer = Time.time;
    }
    private void Update()
    {
        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.Update();
            return;
        }
        if (IsDrawing)
        {
            DrawRectangle();
            return;
        }
        if (!HvoUtils.IsPointerOverUIElement())
            m_CameraController.Update();

        HandleClick();
        UpdateMovementRay();

        if (Time.time - lastUpdateTimer >= 1f)
        {
            lastUpdateTimer = Time.time;
            UpdateGoldProduction(0);
            UpdateMeatProduction(0);
            UpdateWoodProduction(0);
        }
    }

    private void HandleClick()
    {

        if (HvoUtils.IsPointerOverUIElement())
        {
            // Debug.Log("Cancle Selected.");
            //ResetSelectedUnits();
            return;
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

        return;
    }

    #region Handle Click
    private void HandleUnitBehaviour(Vector2 _mousePosition) // 处理单个单位
    {
        //        Debug.Log("Handle Unit Behaviour");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_mousePosition, detectedRadius);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Unit unit))
            {
                SelectNewUnit(unit, _mousePosition);
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
    }

    private void SelectNewUnit(Unit _unit, Vector2 _mousePosition)
    {
        //       Debug.Log("Select New Unit Part 0.");
        // 处理工人
        bool flowControl = HandleWorkerTask(_unit);
        if (!flowControl)
        {
            return;
        }
        //        Debug.Log("Select New Unit Part 1.");
        // 处理攻击的情况
        flowControl = HandleUnitsAttack(_unit, _mousePosition);
        if (!flowControl)
        {
            return;
        }
        // Debug.Log($"IsDrawing : {IsDrawing}");


        if ((_unit is StructureUnit house && !house.IsCompleted) || _unit.IsDead || IsDrawing || ActiveUnit == _unit)
        {
            return;
        }
        if (ActiveUnit != null || SelectedUnits.Count > 0)
        {
            ResetSelectedUnits();
        }

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
            var vaildWorkers = SelectedUnits.Where(unit => unit != null && unit.TryGetComponent(out WorkerUnit worker) && worker.currentTask == WorkerTask.None).ToList();
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
            if (ActiveUnit is WorkerUnit worker)
            {
                if (_unit.TryGetComponent(out StructureUnit structure) && structure.IsUnderConstruction)
                {
                    worker.AssignTarget(structure);
                    worker.UpdateWorkerTask(WorkerTask.Building);
                    return false;
                }
                else if (_unit.TryGetComponent(out TreeUnit tree) && !tree.IsDead && worker.currentTask == WorkerTask.None)
                {
                    tree.AssignWorker(worker);
                    worker.AssignTarget(tree);
                    worker.UpdateWorkerTask(WorkerTask.Chopping);
                    return false;
                }
                else if (_unit.TryGetComponent(out SheepUnit sheep) && !sheep.IsDead && worker.currentTask == WorkerTask.None)
                {
                    sheep.AssignTarget(worker);
                    worker.AssignTarget(sheep);
                    worker.UpdateWorkerTask(WorkerTask.Killing);
                    return false;
                }
                else if (_unit.TryGetComponent(out GoldMinerUnit miner) && !miner.IsDead && miner.IsCompleted && worker.currentTask == WorkerTask.None)
                {
                    worker.AssignTarget(miner);
                    worker.UpdateWorkerTask(WorkerTask.Mining);
                    return false;
                }
                else if (_unit.TryGetComponent(out CastleUnit castle) && castle.IsCompleted && worker.currentTask == WorkerTask.Trasporting)
                {
                    worker.AssignTarget(castle);
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
                foreach (var unit in SelectedUnits.Where(unit => unit != null && !unit.TryGetComponent(out WorkerUnit _) && !unit.TryGetComponent(out MonkUnit _) && !unit.IsDead))
                {
                    unit.GetComponent<HumanoidUnit>().AssignTarget(_unit);
                }
                return false;
            }
            else
            {
                if (ActiveUnit != null && (!ActiveUnit.TryGetComponent(out WorkerUnit _) || !ActiveUnit.TryGetComponent(out StructureUnit _) || !ActiveUnit.TryGetComponent(out MonkUnit _)))
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
        //        Debug.Log("Reset Units In Rectangle methods");
        float minX = Mathf.Min(_startPos.x, _endPos.x);
        float maxX = Mathf.Max(_startPos.x, _endPos.x);
        float minY = Mathf.Min(_startPos.y, _endPos.y);
        float maxY = Mathf.Max(_startPos.y, _startPos.y);

        var vaildUnits = RegisteredUnits.Where(unit => unit != null && unit.CompareTag("BlueUnit") && !unit.IsDead && unit.TryGetComponent(out HumanoidUnit _) && !unit.TryGetComponent(out TowerUnit _)).ToList();

        foreach (var unit in vaildUnits)
        {
            Vector3 screenPos = unit.transform.position;

            if (screenPos.x >= minX && screenPos.x <= maxX && screenPos.y >= minY && screenPos.y <= maxY)
            {
                if (!SelectedUnits.Contains(unit))
                {
                    SelectedUnits.Add(unit);
                    unit.SelectedUnit();
                }
            }
        }
        onSelectionFinished?.Invoke();
        IsDrawing = false;
    }

    public void ResetSelectedUnits()
    {
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
                var line = GameObjectPool.Get().GetFromPool("ActionRay").GetComponent<LineRenderer>();
                line.startColor = _color;
                line.endColor = _color;

                ActionRays[unit] = line;
                StartCoroutine(HideRayAfterDelay(line.gameObject));
                Instantiate(Pointer, _mousePosition, Quaternion.identity);
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
            Instantiate(Pointer, _mousePosition, Quaternion.identity);
        }

    }

    private void GenerateBoxRenderer()
    {
        var renderer = GameObject.Find("BoxRenderer");
        if (renderer != null)
        {
            Destroy(renderer.gameObject);
        }

        BoxRenderer = new();
        GameObject go = new GameObject("BoxRenderer");
        BoxRenderer = go.AddComponent<LineRenderer>();
        BoxRenderer.material = new Material(Shader.Find("Sprites/Default"));
        BoxRenderer.sortingLayerName = "Unit";
        BoxRenderer.sortingOrder = 100;
        BoxRenderer.startColor = Color.yellow;
        BoxRenderer.endColor = Color.yellow;
        BoxRenderer.startWidth = .08f;
        BoxRenderer.endWidth = .08f;
        BoxRenderer.loop = true;
        BoxRenderer.positionCount = 5;
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
            GenerateBoxRenderer();
        }

        if (HvoUtils.IsPointerPress())
        {
            Vector3 currentPos = GetWorldPosition();

            Vector3 p0 = StartPos;
            Vector3 p1 = new Vector3(StartPos.x, currentPos.y, 0);
            Vector3 p2 = currentPos;
            Vector3 p3 = new Vector3(currentPos.x, StartPos.y, 0);

            if (BoxRenderer != null)
            {
                Vector3[] positions = new Vector3[5];
                positions[0] = p0;
                positions[1] = p1;
                positions[2] = p2;
                positions[3] = p3;
                positions[4] = p0;
                BoxRenderer.SetPositions(positions);
            }
        }

        if (HvoUtils.IsPointerUp())
        {
            //            Debug.Log("Pointer enend");
            if (BoxRenderer != null)
                Destroy(BoxRenderer.gameObject);
            Vector3 currentPos = GetWorldPosition();

            if (Mathf.Abs(StartPos.x - currentPos.x) < 1f || Mathf.Abs(StartPos.y - currentPos.y) < 1f)
            {
                return;
            }

            float pointPosition = Vector2.Distance(PointerDownPosition, HvoUtils.GetPointerPositoin());
            float pointDuration = Time.time - PointerDownTime;
            m_IsDrag = pointPosition > DragDistance && pointDuration > DragDuration;
            //            Debug.Log($"drag distance : {pointDuration} , pointDuraion : {pointDuration} , isDrag : {m_IsDrag}");
            if (m_IsDrag)
            {
                SelectUnitsInRectangle(StartPos, currentPos);
            }

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
                GameObjectPool.Get().ReturnToPool("ActionRay", pair.Value.gameObject);
        }

        ActionRays.Clear();
    }

    private IEnumerator HideRayAfterDelay(GameObject _ray)
    {
        yield return new WaitForSeconds(.3f);
        GameObjectPool.Get().ReturnToPool("ActionRay", _ray);
        ActiveRay = null;
    }

    #endregion

    #region Place Building Methods
    public void StartBuildingProcess(BuildingActionSO _action)
    {
        ClearPlacement();

        m_PlacementProcess = new(_action, m_TilemapManager);
        PlaceBuildingUI.ShowRectangle(_action.GoldCost, _action.WoodCost,_action.CrystalCost);
        PlaceBuildingUI.RegisterHooks(() => ConfirmPlacement(_action), CanclePlacement);
    }

    private void ConfirmPlacement(BuildingActionSO _action)
    {
        if (WoodAmount >= _action.WoodCost && GoldAmount >= _action.GoldCost && CrystalAmount >= _action.CrystalCost)
        {
            var buildingAction = m_PlacementProcess.BuildingAction;

            if (buildingAction == null)
                return;

            if (m_PlacementProcess.CanPlaceBuilding(out Vector3 placePosition))
            {
                new BuildingProcess(buildingAction, placePosition);

                WoodAmount -= _action.WoodCost;
                GoldAmount -= _action.GoldCost;
                CrystalAmount -= _action.CrystalCost;

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

    private IEnumerator UpdateNodesCoroutine(Vector3 _placePosition, BuildingActionSO _buildingAction)
    {
        yield return null;

        yield return new WaitForFixedUpdate();

        Vector3Int orientPosition = new Vector3Int(Mathf.FloorToInt(_placePosition.x + _buildingAction.BuildingOffset.x), Mathf.FloorToInt(_placePosition.y + _buildingAction.BuildingOffset.y), 0);
        m_TilemapManager.UpdateNodesInArea(orientPosition, _buildingAction.BuildingSize.x, _buildingAction.BuildingSize.y);
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
            StartCoroutine(UpdateNodesCoroutine(_placePosition, _buildingAction));
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
        var pool = GameObjectPool.Get();
        string sb = "+ " + _amount.ToString();

        switch (_type)
        {
            case ResourceType.wood:
                newImage = pool.GetFromPool(WoodCollection.name);
                WoodAmount += _amount;
                break;
            case ResourceType.meat:
                newImage = pool.GetFromPool(MeatCollection.name);
                MeatAmount += _amount;
                break;
            case ResourceType.gold:
                newImage = pool.GetFromPool(GoldCollection.name);
                GoldAmount += _amount;
                break;
            case ResourceType.crystal:
                newImage = pool.GetFromPool(CrystalCollection.name);
                CrystalAmount += _amount;
                break;
            default:
                return;
        }
        newImage.transform.position = _startPos;
        newImage.transform.rotation = Quaternion.identity;
        newImage.GetComponentInChildren<TextMeshProUGUI>().text = sb;
        newImage.transform.DOMove(_startPos + new Vector3(0, 2f, 0), 1f).SetEase(Ease.Linear).OnComplete(() => OnPopUpImage(newImage));

        onResourcesChanged?.Invoke();
    }
    public void CollectWood(int _woodCount, Vector3 _startPos) => CollectResource(ResourceType.wood, _woodCount, _startPos);
    public void CollectMeat(int _meatCount, Vector3 _startPos) => CollectResource(ResourceType.meat, _meatCount, _startPos);
    public void CollectGold(int _goldCount, Vector3 _startPos) => CollectResource(ResourceType.gold, _goldCount, _startPos);
    public void CollectCrystal(int _crystalCount, Vector3 _startPos) => CollectResource(ResourceType.crystal,_crystalCount,_startPos);
    private void OnPopUpImage(GameObject _image)
    {
        var name = _image.name.Replace("(Clone)", "");
        GameObjectPool.Get().ReturnToPool(name, _image);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        OptionsUI.gameObject.SetActive(true);
    }

    #endregion

    #region Training Unit Methods
    public void StartTrainingProcess(TrainingActionSO _action)
    {
        StartCoroutine(TrainingProcessWithDelay(_action));
    }

    private IEnumerator TrainingProcessWithDelay(TrainingActionSO _action)
    {
        yield return new WaitForEndOfFrame();
        TrainingUnitUI.ShowRectangle(_action.GoldCost, _action.MeatCost,_action.CrystalCost);
        TrainingUnitUI.RegisterHooks(() => ConfirmTraining(_action), CancleTraining);
    }

    private void ConfirmTraining(TrainingActionSO _trainingAction)
    {
        if (GoldAmount >= _trainingAction.GoldCost && MeatAmount >= _trainingAction.MeatCost && CrystalAmount >= _trainingAction.CrystalCost)
        {
            GoldAmount -= _trainingAction.GoldCost;
            MeatAmount -= _trainingAction.MeatCost;
            CrystalAmount -= _trainingAction.CrystalCost;
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
            SwitchBGM();
            FaliureUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => !unit.IsDead && unit.CompareTag("BlueUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }

        if (!HasActiveRedBuilding)
        {
            SwitchBGM();
            WiningUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => unit.CompareTag("RedUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        FindObjectsOfType<DamageFontUI>().Where(font => font != null && font.gameObject.activeSelf).ToList().ForEach(font => Destroy(font.gameObject));
        FindObjectsOfType<ArrowController>().Where(arrow => arrow != null && arrow.gameObject.activeSelf).ToList().ForEach(arrow => Destroy(arrow.gameObject));
        FindObjectsOfType<GrenadeController>().Where(grenade => grenade != null && grenade.gameObject.activeSelf).ToList().ForEach(grenade => Destroy(grenade.gameObject));
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("选择关卡");
    }

    public void InitializeGame()
    {
        var manager = SettingsManager.Get();
        var pool = GameObjectPool.Get();
        WoodAmount = manager.woodAmount;
        MeatAmount = manager.meatAmount;
        GoldAmount = manager.goldAmount;

        onResourcesChanged?.Invoke();

        SettingsManager.Get().AssignOriginalPositions();

        InitializeRay();
        EventCenter.Instance.AddEventListener<int>("WoodProduction", UpdateWoodProduction);
        EventCenter.Instance.AddEventListener<int>("MeatProduction", UpdateMeatProduction);
        EventCenter.Instance.AddEventListener<int>("GoldProduction", UpdateGoldProduction);

        pool.RegisterPool(damageFont.name, damageFont, 80);
        pool.RegisterPool(arrow.name, arrow, 60);
        pool.RegisterPool(firecraker.name, firecraker, 40);
        pool.RegisterPool(WoodCollection.name, WoodCollection, 40);
        pool.RegisterPool(MeatCollection.name, MeatCollection, 40);
        pool.RegisterPool(GoldCollection.name, GoldCollection, 40);
        pool.RegisterPool(CrystalCollection.name,CrystalCollection,20);
        pool.RegisterPool(bigArrow.name, bigArrow, 60);
        pool.RegisterPool(healEffect.name, healEffect, 30);
        pool.RegisterPool(magicExplosion.name,magicExplosion,25);
    }

    private void SwitchBGM()
    {
        AudioManager.Get().StopPlayBGM(1);
        AudioManager.Get().PlayBGM(0);
    }

    private void InitializeRay()
    {
        GameObject go = new GameObject("ActoinRay");
        LineRenderer line = go.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 50;

        line.startWidth = .07f;
        line.endWidth = .07f;

        GameObjectPool.Get().RegisterPool("ActionRay", line.gameObject, 30);
    }

    #region Update Production Info
    private void UpdateWoodProduction(int _amount)
    {
        float now = Time.time;
        producedWoodHistroy.Enqueue((now, _amount));

        while (producedWoodHistroy.Count > 0 && now - producedWoodHistroy.Peek().time > timeWindow)
        {
            producedWoodHistroy.Dequeue();
        }
        int totalProduced = 0;
        foreach (var record in producedWoodHistroy)
        {
            totalProduced += record.amount;
        }
        WoodProduction.text = "木材产量:" + totalProduced.ToString() + "/分钟";
    }
    private void UpdateMeatProduction(int _amount)
    {
        float now = Time.time;
        producedMeatHistroy.Enqueue((now, _amount));

        while (producedMeatHistroy.Count > 0 && now - producedMeatHistroy.Peek().time > timeWindow)
        {
            producedMeatHistroy.Dequeue();
        }
        int totalProduced = 0;
        foreach (var record in producedMeatHistroy)
        {
            totalProduced += record.amount;
        }
        MeatProduction.text = "兽肉产量:" + totalProduced.ToString() + "/分钟";
    }
    private void UpdateGoldProduction(int _amount)
    {
        float now = Time.time;
        producedGoldHistroy.Enqueue((now, _amount));

        while (producedGoldHistroy.Count > 0 && now - producedGoldHistroy.Peek().time > timeWindow)
        {
            producedGoldHistroy.Dequeue();
        }
        int totalProduced = 0;
        foreach (var record in producedGoldHistroy)
        {
            totalProduced += record.amount;
        }
        GoldProduction.text = "金矿产量:" + totalProduced.ToString() + "/分钟";
    }
    #endregion
}
