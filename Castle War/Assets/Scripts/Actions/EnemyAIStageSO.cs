using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIStage",menuName = "Action/EnemyAIStage")]
public class EnemyAIStageSO : ScriptableObject
{
    public BuildingActionSO BuildingAction;
    public List<TrainingActionSO> TrainingActions;
}
