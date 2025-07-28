using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProcess 
{
    private BuildingActionSO m_BuildingAction;
    public BuildingActionSO BuildingAction => m_BuildingAction;

    public BuildingProcess(BuildingActionSO _buildingAction,Vector3 _placePosition)
    {
        m_BuildingAction = _buildingAction;

        var go = Object.Instantiate(m_BuildingAction.StructurePrefab);
        go.GetComponentInChildren<SpriteRenderer>().sprite = m_BuildingAction.FoundationSprite;
        go.transform.position = _placePosition;
        go.GetComponent<StructureUnit>().AssignBuildingProcess(this);
    }

    public BuildingProcess(BuildingActionSO _buildingAction, Vector3 _placePosition,out StructureUnit _structure)
    {
        m_BuildingAction = _buildingAction;

        var go = Object.Instantiate(m_BuildingAction.StructurePrefab);
        go.GetComponentInChildren<SpriteRenderer>().sprite = m_BuildingAction.FoundationSprite;
        go.transform.position = _placePosition;
        go.GetComponent<StructureUnit>().AssignBuildingProcess(this);
        _structure = go.GetComponent<StructureUnit>();
    }



}
