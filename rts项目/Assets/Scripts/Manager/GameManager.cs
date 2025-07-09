using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : SingletonManager<GameManager>
{
    public Unit ActiveUnit;
    [SerializeField] private float detectedRadius = .3f;
    [SerializeField] private GameObject pointer;

    [Header("UI Parameters")]
    [SerializeField] private ActionBar ActionBar;
    [SerializeField] private PlaceBuildingUI PlaceBuildingUI;
    [SerializeField] private TrainingUnitUI TrainingUnitUI;
    [SerializeField] private TrainingUI TrainingUI;
    [SerializeField] private Image GameOverUI;

    [Header("Registered Target")]
    public List<Unit> RegisteredUnits = new List<Unit>();

    [Header("Camera Options")]
    [SerializeField] private float PanSpeed;
    [SerializeField] private CameraBounds CameraBounds;

    [Header("Resources Amount")]
    public int WoodAmount;
    public int GoldAmount;
    public UnityAction onResourcesChanged;

    private Vector2 m_MousePosition;

    private LineRenderer ActiveRay;

    private PlacementProcess m_PlacementProcess;
    private TilemapManager m_TilemapManager;

    private CameraController m_CameraController;


    private void Start()
    {
        m_TilemapManager = TilemapManager.Get();
        m_CameraController = new(PanSpeed,CameraBounds);
    }
    private void Update()
    {
        m_CameraController.Update();    
        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.Update();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_MousePosition = mousePosition;
            HandleClick(mousePosition);
            //var path = m_TilemapManager.FindPath(ActiveUnit.transform.position, mousePosition);

            //foreach(var node in path)
            //{
            //    m_TilemapManager.SetTile(new Vector3Int(node.ButtomX,node.ButtomY));
            //}
        }
        UpdateMovementRay();
    }

    #region Handle Click
    private void HandleClick(Vector2 _mousePosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_mousePosition,detectedRadius);

        foreach(var collider in colliders)
        {
            if(collider.TryGetComponent(out Unit unit))
            {
                SelectNewUnit(unit);
                return;
            }
        }
        if(ActiveUnit != null && ActiveUnit.TryGetComponent(out HumanoidUnit _))
        {
            (ActiveUnit as HumanoidUnit).MoveToDestination(_mousePosition);
            InitializeRay();
            Instantiate(pointer,_mousePosition,Quaternion.identity);
        }
    }
    private void SelectNewUnit(Unit _unit)
    {
        if(_unit.TryGetComponent(out StructureUnit structure) && structure.IsUnderConstruction && ActiveUnit is WorkerUnit worker)
        {
            worker.AssignTarget(structure);  
            worker.currentTask = WorkerTask.Building;
            return;
        }
        else if(_unit.TryGetComponent(out TreeUnit tree) && !tree.IsDead && ActiveUnit is WorkerUnit worker_2)
        {
            worker_2.AssignTarget(tree);
            worker_2.currentTask = WorkerTask.Chopping;
            return;
        }
        else if(_unit.CompareTag("RedUnit") && !_unit.IsDead)
        {
            if(ActiveUnit != null && (!ActiveUnit.TryGetComponent(out WorkerUnit _) || !ActiveUnit.TryGetComponent(out StructureUnit _)))
            {
                (ActiveUnit as HumanoidUnit).AssignTarget(_unit);
                return;
            }
        }

        ActiveUnit = _unit.CompareTag("BlueUnit") ? _unit : null;
        ActionBar.ClearAllActionButtons();
        ActionBar.HideActionBar();

        if (ActiveUnit != null && ActiveUnit.Actions.Count > 0)
        {
            ActionBar.ShowActionBar();

            foreach (var action in ActiveUnit.Actions)
            {
                ActionBar.RegisterActionButton(action.Icon, () => action.ExecuteAction());
            }
        }
    }
    #endregion

    #region Ray Methods
    private void InitializeRay()
    {
        GameObject go = new GameObject("MovementRay");
        ActiveRay = go.AddComponent<LineRenderer>();
        ActiveRay.material = new Material(Shader.Find("Sprites/Default"));
        ActiveRay.sortingOrder = 50;
        ActiveRay.startColor = Color.green;
        ActiveRay.endColor = Color.green;
        ActiveRay.startWidth = .1f;
        ActiveRay.endWidth = .1f;
        StartCoroutine(HideRayAfterDelay(go));
    }

    private void UpdateMovementRay()
    {
       if(ActiveUnit != null && ActiveRay != null)
       {
            ActiveRay.positionCount = 2;
            ActiveRay.SetPosition(0,ActiveUnit.transform.position);
            ActiveRay.SetPosition(1,m_MousePosition);
       }
    }

    private IEnumerator HideRayAfterDelay(GameObject _ray)
    {
        yield return new WaitForSeconds(.5f);
        Destroy(_ray);
        ActiveRay = null;
    }

    #endregion

    #region Place Building Methods
    public void StartBuildingProcess(BuildingActionSO _action)
    {
        if(WoodAmount >= _action.WoodCost && GoldAmount >= _action.GoldCost)
        {
            WoodAmount -= _action.WoodCost;
            GoldAmount -= _action.GoldCost;
            onResourcesChanged?.Invoke();
        }
        else
        {
            return;
        }

        m_PlacementProcess = new(_action,m_TilemapManager);

        PlaceBuildingUI.ShowRectangle(_action.GoldCost,_action.WoodCost);
        PlaceBuildingUI.RegisterHooks(ConfirmPlacement,CanclePlacement);
    }

    private void ConfirmPlacement()
    {
        var buildintAction = m_PlacementProcess.BuildingAction;

        if (buildintAction == null)
            return;
        
        if(m_PlacementProcess.CanPlaceBuilding(out Vector3 placePosition))
        {
            new BuildingProcess(buildintAction,placePosition);
           
            ClearActionBarUI();
            ClearPlacement();
        }
    }

    private void CanclePlacement()
    {
        ClearPlacement();
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
        if(m_PlacementProcess != null)
        {
            m_PlacementProcess.ClearupPlacement();
            m_PlacementProcess = null;
            PlaceBuildingUI.HideRectangle();
        }
    }

    #endregion

    #region Training Unit Methods
    public void StartTrainingProcess(TrainingActionSO _trainingAction)
    {
        TrainingUnitUI.ShowRectangle(_trainingAction.TrainingCost);
        TrainingUnitUI.RegisterHooks(() => ConfirmTraining(_trainingAction),CancleTraining);
    }

    private void ConfirmTraining(TrainingActionSO _trainingAction)
    {
        if(GoldAmount >= _trainingAction.TrainingCost)
        {
            GoldAmount -= _trainingAction.TrainingCost;
            onResourcesChanged();
        }
        else
        {
            return;
        }

        TrainingUI.RegisterTrainingUnit(_trainingAction.UnitType,_trainingAction.TrainingTime,ActiveUnit as StructureUnit,_trainingAction.UnitPrefab);
    }

    private void CancleTraining()
    {
        TrainingUnitUI.HideRectangle();
        ClearActionBarUI();
    }

    #endregion

    public bool CanEnemyPlaceBuilding(BuildingActionSO _buildingAction,Vector3 _placePosition)
    {
        var placementProcess = new PlacementProcess(_buildingAction,m_TilemapManager,true);

        if(placementProcess.CanPlaceBuilding(_placePosition))
        {
            return true;
        }
        return false;
    }

    public void RegisterUnit(Unit _unit)
    {
        RegisteredUnits.Add(_unit);
    }

    public void RemoveUnit(Unit _unit)
    {
        RegisteredUnits.Remove(_unit);

        bool HasActiveBlueBuilding = RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.TryGetComponent(out StructureUnit _) && unit.CompareTag("BlueUnit")).Any();
        bool HasActiveRedBuilding = RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.TryGetComponent(out StructureUnit _) && unit.CompareTag("RedUnit")).Any();

        if(!HasActiveBlueBuilding)
        {
            GameOverUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => !unit.IsDead && unit.CompareTag("BlueUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }

        if(!HasActiveRedBuilding)
        {
            GameOverUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => unit.CompareTag("RedUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex -1);
    }




}
