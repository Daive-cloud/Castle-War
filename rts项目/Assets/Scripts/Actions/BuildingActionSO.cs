using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingAction",menuName = "Action/BuildingAction")]
public class BuildingActionSO : ActionSO
{
    [SerializeField] private Sprite m_FoundationSprite;
    [SerializeField] private Sprite m_CompletionSprite;
    [SerializeField] private GameObject m_StructurePrefab;
    [SerializeField] private int m_GoldCost;
    [SerializeField] private int m_WoodCost;
    [SerializeField] private Vector3Int m_BuildingSize;
    [SerializeField] private Vector3Int m_BuildingOffset;
    public  Sprite FoundationSprite => m_FoundationSprite;
    public Sprite CompletionSprite => m_CompletionSprite;
    public GameObject StructurePrefab => m_StructurePrefab;
    public int GoldCost => m_GoldCost;
    public int WoodCost => m_WoodCost;
    public Vector3Int BuildingSize => m_BuildingSize;
    public Vector3Int BuildingOffset => m_BuildingOffset;
  
    public override void ExecuteAction()
    {
        var manager = GameManager.Get();

        manager.StartBuildingProcess(this);
    }
}
