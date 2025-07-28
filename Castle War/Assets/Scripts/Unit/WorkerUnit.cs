using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using IUnit;

public enum WorkerTask
{
    None,Building,Chopping,Killing,Mining,Trasporting
}


public class WorkerUnit : HumanoidUnit
{
    public WorkerTask currentTask = WorkerTask.None;
    [Header("Worker Task UI")]
    [SerializeField] private Image TaskUI;
    [SerializeField] private Sprite DefaultIcon;
    [SerializeField] private Sprite BuildIcon;
    [SerializeField] private Sprite ChopIcon;
    [SerializeField] private Sprite KillIcon;
    [SerializeField] private Sprite MiningIcon;
    [SerializeField] private Sprite TransportIcon;
    [Header("ResourceUI")]
    [SerializeField] private Image ResourceUI;
    [SerializeField] private Sprite WoodIcon;
    private int woodCount;
    [SerializeField] private Sprite MeatIcon;
    private int meatCount;
    [SerializeField] private Sprite GoldIcon;
    private int goldCount;
    private GoldMinerUnit lastEnteredMiner;
    private bool IsTransportGold = false;
    private bool IsTransportMeat = false;
    private bool IsTransportWood = false;

    protected override void Start()
    {
        base.Start();
        TaskUI.gameObject.SetActive(false);
        ResourceUI.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        UpdateBehaviour();

        m_Velocity = (transform.position - m_LastPosition).magnitude;
        m_LastPosition = transform.position;

        bool state = m_Velocity > 0;
        if (currentTask != WorkerTask.Trasporting)
            anim.SetBool("Move", state);
        else
        {
            anim.SetBool("Hold", !state);
        }
    }

    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer >= CheckFrequency)
        {
            CheckTimer = Time.time;
            //Debug.Log($"Has Assigned Target : {HasRegisteredTarget}");
            if (HasRegisteredTarget)
                MoveToDestination(Target.transform.position);

            if (IsTargetDetected())
            {
                if (currentTask == WorkerTask.Building)
                {
                    anim.SetBool("Build", true);
                    StartBuildingProcess(Target as StructureUnit);
                }
                else if (currentTask == WorkerTask.Chopping || currentTask == WorkerTask.Killing)
                {
                    anim.SetBool("Chop", true);
                }
                else if (currentTask == WorkerTask.Mining)
                {
                    (Target as GoldMinerUnit).EnterMiner(this);
                }
                else if (currentTask == WorkerTask.Trasporting)
                {
                    FinishTransportResources();
                }

            }

        }
    }

    private void FinishTransportResources()
    {
        HideResourceUI();
        ResetAnimation();
        if (IsTransportWood)
        {
            m_GameManager.CollectWood(woodCount, Target.transform.position + new Vector3(0, 1f, 0));
            FindClosestResource<TreeUnit>();
            if (Target != null)
            {
                UpdateWorkerTask(WorkerTask.Chopping);
            }
        }
        else if (IsTransportMeat)
        {
            m_GameManager.CollectMeat(meatCount, Target.transform.position + new Vector3(0, 1f, 0));
            FindClosestResource<SheepUnit>();
            if (Target != null)
            {
                UpdateWorkerTask(WorkerTask.Killing);
            }
        }
        else if (IsTransportGold)
        {
            m_GameManager.CollectGold(goldCount, Target.transform.position + new Vector3(0, 1f, 0));
            if (lastEnteredMiner != null)
            {
                AssignTarget(lastEnteredMiner);
            }
            UpdateWorkerTask(WorkerTask.Mining);
        }
    }

    public void StartBuildingProcess(StructureUnit _structure)
    {
        _structure.AssignWorker(this);
    }

    public void UsingAxe()
    {
        if (HasRegisteredTarget)
        {
            if (Target.TryGetComponent(out TreeUnit tree))
            {
                AudioManager.Get().PlaySFX(7);
                tree.Shake();
            }
            else if (Target.TryGetComponent(out SheepUnit sheep))
            {
                AudioManager.Get().PlaySFX(33);
                sheep.Escape();
            }
        }
    }
    public void UpdateWorkerTask(WorkerTask _task)
    {
        switch (_task)
        {
            case WorkerTask.None:
                ResetTransportState();
                ResetAnimation();
                TaskUI.sprite = DefaultIcon;
                break;
            case WorkerTask.Building:
                ResetTransportState();
                TaskUI.sprite = BuildIcon;
                break;
            case WorkerTask.Chopping:
                IsTransportWood = true;
                TaskUI.sprite = ChopIcon;
                break;
            case WorkerTask.Killing:
                IsTransportMeat = true;
                TaskUI.sprite = KillIcon;
                break;
            case WorkerTask.Mining:
                IsTransportGold = true;
                TaskUI.sprite = MiningIcon;
                break;
            case WorkerTask.Trasporting:
                ResetAnimation();
                anim.SetBool("Transport", true);
                TaskUI.sprite = TransportIcon;
                break;
            default:
                return;
        }
        currentTask = _task;
    }

    #region Override Methods
    public override void UnassignTarget()
    {
//        Debug.Log("Uassing Target.");
        if (Target != null && Target.TryGetComponent(out StructureUnit _))
        {
            Target.GetComponent<StructureUnit>().UnassignWorker(this);
        }
        if (currentTask != WorkerTask.Trasporting)
            UpdateWorkerTask(WorkerTask.None);
        base.UnassignTarget();
    }

    #region Override Methods
    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(18);
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(25);
    }

    public void PlayBuildingSound()
    {
        AudioManager.Get().PlaySFX(6);
    }

    public override void SelectedUnit()
    {
        base.SelectedUnit();
        TaskUI.gameObject.SetActive(true);
    }

    public override void UnselectedUnit()
    {
        base.UnselectedUnit();
        TaskUI.gameObject.SetActive(false);
    }
    public override void Death()
    {
        base.Death();
        UnassignTarget();
        TaskUI.gameObject.SetActive(false);
        ResourceUI.gameObject.SetActive(false);
    }

    #endregion
    public bool IsTargetDetected()
    {
        if (!HasRegisteredTarget)
            return false;

        var distance = Vector2.Distance(Target.transform.position, transform.position);
        if (Target.TryGetComponent(out StructureUnit structure))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ObjectCheckRadius);
            bool flag = colliders.ToList().Contains(structure.GetComponent<CapsuleCollider2D>());

            if (structure.TryGetComponent(out CastleUnit _))
            {
                return flag;
            }
            else if (structure.TryGetComponent(out GoldMinerUnit _) && currentTask == WorkerTask.Mining)
            {
                if (distance < ObjectCheckRadius * 1.5f)
                {
                    //                    Debug.Log("Switch Find Way");
                    ai.SwitchFindWayType(FindPathType.Direct);
                    if (distance < ObjectCheckRadius * .6f)
                    {
                        ai.SwitchFindWayType(FindPathType.A_Star);
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (structure.IsCompleted)
            {
                UnassignTarget();
                return false;
            }
            return flag;
        }
        else if (Target.TryGetComponent(out TreeUnit tree))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ObjectCheckRadius);
            bool flag = colliders.ToList().Contains(tree.GetComponent<CapsuleCollider2D>());

            if (!flag)
            {
                ai.SwitchFindWayType(FindPathType.Direct);
                return false;
            }
            ai.SwitchFindWayType(FindPathType.A_Star);
            return true;
        }

        return distance < ObjectCheckRadius;
    }
    #endregion

    private void ResetTransportState()
    {
        IsTransportGold = false;
        IsTransportMeat = false;
        IsTransportWood = false;
    }

    public void ResetAnimation()
    {
        anim.SetBool("Hold",false);
        anim.SetBool("Build", false);
        anim.SetBool("Chop", false);
        anim.SetBool("Transport", false);
    }

    public void TransportResource(int _woodCount, int _meatCount, int _goldCount)
    {
        var castle = FindMainCastle();
        if (castle != null)
        {
            AssignTarget(castle);
        }
        ShowResourceIcon();

        woodCount = _woodCount;
        meatCount = _meatCount;
        goldCount = _goldCount;
    }

    private CastleUnit FindMainCastle() => FindObjectsOfType<CastleUnit>().Where(unit => !unit.IsDead && unit.CompareTag(this.tag) && unit.IsCompleted).FirstOrDefault();

    private void FindClosestResource<T>() where T : MonoBehaviour, IResouceUnit
    {
        var resources = FindObjectsOfType<T>().Where(unit => !unit.IsDead && !unit.HasAssignedWorker && unit.TryGetComponent(out T _));
        T closestUnit = null;
        float closestDistance = Mathf.Infinity;
        //  Debug.Log($"Start Distance : {closestDistance}");
        foreach (var unit in resources)
        {
            float distance = Vector2.Distance(transform.position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUnit = unit;
            }
        }

        if (closestUnit != null)
        {
            closestUnit.AssignWorker(this);
            AssignTarget(closestUnit as Unit);
        }
        else
        {
//            Debug.Log("Not Found Target");
            UnassignTarget();
            UpdateWorkerTask(WorkerTask.None);
        }

       // Debug.Log($"Target : {Target}");
    }

    private void ShowResourceIcon()
    {
        ResourceUI.gameObject.SetActive(true);
        switch (currentTask)
        {
            case WorkerTask.Chopping:
                ResourceUI.sprite = WoodIcon;
                break;
            case WorkerTask.Killing:
                ResourceUI.sprite = MeatIcon;
                break;
            case WorkerTask.Mining:
                ResourceUI.sprite = GoldIcon;
                break;
            default:
                return;
        }
    }
    
    private void HideResourceUI() =>  ResourceUI.gameObject.SetActive(false);

    public void RecordLastGoldMiner(GoldMinerUnit _goldMiner) => lastEnteredMiner = _goldMiner;
}
