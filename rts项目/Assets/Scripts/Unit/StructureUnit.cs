using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUnit : Unit
{
    [Header("Building Effect")]
    [SerializeField] private ParticleSystem BuildingEffect;
    [Header("Sturcture Unit")]
    [SerializeField] private GameObject TowerUnit;

    [Header("Death Info")]
    [SerializeField] private ParticleSystem DeathEffect;
    [SerializeField] private Sprite DeathIcon;

    protected BuildingProcess m_BuildingProcess;

    public bool IsUnderConstruction => m_BuildingProcess != null;

    private WorkerUnit ActiveWorker;
    private bool HasAssignedWorker => ActiveWorker != null;

    private float ProcessValue = 0f;

    protected override void UpdateBehaviour()
    {
        if(Time.time - CheckTimer > CheckFrequency)
        {
            CheckTimer = Time.time;

            if(IsUnderConstruction && HasAssignedWorker)
            {
                ProcessValue += .05f;

                if(ProcessValue >= 1f)
                {
                    CompleteConstruction();
                }
            }
        }
    }

    public void AssignBuildingProcess(BuildingProcess _buildingProcess)
    {
        m_BuildingProcess = _buildingProcess;
    }

    public void AssignWorker(WorkerUnit _worker)
    {
        ActiveWorker = _worker;
        BuildingEffect.Play();
    }

    public void UnassignWorker()
    {
        ActiveWorker = null;
    }

    protected void CompleteConstruction()
    {
        ActiveWorker.UnassignTarget();
        ActiveWorker.currentTask = WorkerTask.None;
        UnassignWorker();
        sr.sprite = m_BuildingProcess.BuildingAction.CompletionSprite;
        BuildingEffect.Stop();
        m_BuildingProcess = null;

        if(TowerUnit != null)
        {
            TowerUnit.SetActive(true);
        }

    }

    public override void Death()
    {
        base.Death();

        sr.sprite = DeathIcon;
        DeathEffect.Play();

        if(TowerUnit != null)
        {
            Destroy(TowerUnit);
        }
        StartCoroutine(AfterDeath());
    }

    private IEnumerator AfterDeath()
    {
        yield return null;
        TilemapManager.Get().UpdateNodesOverMap();
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

}
