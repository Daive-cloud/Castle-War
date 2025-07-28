using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FindPathType
{
    A_Star,
    Direct
}

public class AI : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Vector3 m_TargetPosition;
    private HumanoidUnit unit => GetComponent<HumanoidUnit>();
    private TilemapManager m_TilemapManager;
    private List<Node> m_CurrentPath;
    private int m_CurrentNodeIndex;

    public FindPathType currentType = FindPathType.A_Star;

    private void Awake()
    {
        m_TilemapManager = TilemapManager.Get();
        m_CurrentPath = new List<Node>();

    }

    private void Start()
    {
        m_TargetPosition = transform.position;
    }

    private void Update()
    {
        switch (currentType)
        {
            case FindPathType.A_Star:
                FindPathAccordingToAStar();
                break;
            case FindPathType.Direct:
                FindPathDirectly();
                break;
            default:
                return;
        }

    }

    private void FindPathAccordingToAStar()
    {
        if (!IsPathVaild() || unit.IsDead)
            return;

        Node newNode = m_CurrentPath[m_CurrentNodeIndex];
        m_TargetPosition = new Vector3(newNode.CenterX, newNode.CenterY);

        var direction = (m_TargetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(m_TargetPosition, transform.position) < .1f)
        {
            m_CurrentNodeIndex++;
            if (m_CurrentNodeIndex >= m_CurrentPath.Count)
            {
                ClearPath();
                return;
            }
            unit.FlipController(new Vector3(m_CurrentPath[m_CurrentNodeIndex].CenterX, m_CurrentPath[m_CurrentNodeIndex].CenterY));
        }
    }

    private void FindPathDirectly()
    {
        if (unit.Target != null)
        {
            m_TargetPosition = unit.Target.transform.position;
        }
        var direction = m_TargetPosition - transform.position;
        // 分解方向，只取 X 或 Y 轴（较大的那个分量）
        float x = direction.x;
        float y = direction.y;
        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            direction = new Vector3(Mathf.Sign(x), 0, 0);
        }
        else
        {
            direction = new Vector3(0, Mathf.Sign(y), 0);
        }
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    public void RegisterDestination(Vector3 _destionation)
    {
        m_TargetPosition = _destionation;
        if (m_CurrentPath.Count > 0)
        {
            Node newNode = m_TilemapManager.FindNode(_destionation);
            if (newNode != null && newNode == m_CurrentPath.Last())
            {
                return;
            }
        }
        ClearPath();

        var path = m_TilemapManager.FindPath(transform.position, _destionation);
        if (path.Count > 0)
        {
            m_CurrentPath = path;
        }
    }

    public void ClearPath()
    {
        m_CurrentPath = new List<Node>();
        m_CurrentNodeIndex = 0;
    }

    private bool IsPathVaild()
    {
        return m_CurrentPath.Count > 0 && m_CurrentNodeIndex < m_CurrentPath.Count;
    }

    public void SwitchFindWayType(FindPathType _type) => currentType = _type;
    
}
