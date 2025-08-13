using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIStage",menuName = "Action/EnemyAIStage")]
public class EnemyAIStageSO : ScriptableObject
{
    public float StageExistTimer;
    public BuildingActionSO BuildingAction;
    public List<TrainingActionSO> TrainingActions;
}
