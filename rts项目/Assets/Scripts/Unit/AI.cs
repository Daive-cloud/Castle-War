using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Vector3 m_TargetPosition;
    private HumanoidUnit unit => GetComponent<HumanoidUnit>();

    private TilemapManager m_TilemapManager;
    private List<Node> m_CurrentPath;
    private int m_CurrentNodeIndex;

    private void Awake()
    {
        m_TilemapManager = TilemapManager.Get();
        m_CurrentPath  = new List<Node>();
        
    }

    private void Start()
    {
        m_TargetPosition = transform.position;
    }

    private void Update()
    {
        if (!IsPathVaild())
            return;

        Node newNode = m_CurrentPath[m_CurrentNodeIndex];
        m_TargetPosition = new Vector3(newNode.CenterX,newNode.CenterY);
       
        var direction = (m_TargetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if(Vector3.Distance(m_TargetPosition,transform.position) < .1f)
        {
            m_CurrentNodeIndex++;
            if(m_CurrentNodeIndex >= m_CurrentPath.Count)
            {
                ClearPath();
                return;
            }
            unit.FlipController(new Vector3(m_CurrentPath[m_CurrentNodeIndex].CenterX, m_CurrentPath[m_CurrentNodeIndex].CenterY));
        }
    }

    public void RegisterDestination(Vector3 _destionation)
    {
         if(m_CurrentPath.Count > 0)
         {
            Node newNode = m_TilemapManager.FindNode(_destionation);
            if(newNode != null && newNode == m_CurrentPath.Last())
            {
                return;
            }
         }
         ClearPath();

        var path = m_TilemapManager.FindPath(transform.position,_destionation);
        if(path.Count > 0)
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

    
}
