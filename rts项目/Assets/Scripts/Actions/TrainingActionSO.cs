using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrainingAction",menuName = "Action/TrainingAction")]
public class TrainingActionSO : ActionSO
{
    [SerializeField] private GameObject m_UnitPrefab;
    [SerializeField] private int m_GoldCost;
    [SerializeField] private int m_MeatCost;
    [SerializeField] private float m_TrainingTime;
    [SerializeField] private TrainingUnitType m_UnitType;

    private GameManager m_GameManger;

    public GameObject UnitPrefab => m_UnitPrefab;
    public int GoldCost => m_GoldCost;
    public int MeatCost => m_MeatCost; 
    public float TrainingTime => m_TrainingTime;
    public TrainingUnitType UnitType => m_UnitType;


    public override void ExecuteAction()
    {
        m_GameManger = GameManager.Get();

        m_GameManger.StartTrainingProcess(this);
    }
}
