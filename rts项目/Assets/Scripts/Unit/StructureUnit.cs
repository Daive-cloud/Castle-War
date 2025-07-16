using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StructureUnit : Unit
{
    private Animator anim => GetComponentInChildren<Animator>();

    [Header("Building Effect")]
    [SerializeField] private ParticleSystem BuildingEffect;
    [Header("Sturcture Unit")]
    [SerializeField] private GameObject TowerUnit;

    [Header("Death Info")]
    [SerializeField] private ParticleSystem DeathEffect;
    [SerializeField] private Sprite DeathIcon;

    protected BuildingProcess m_BuildingProcess;

    public bool IsUnderConstruction => m_BuildingProcess != null;
    public bool IsCompleted = false;

    private WorkerUnit ActiveWorker;
    private bool HasAssignedWorker => ActiveWorker != null;

    private float ProcessValue = 0f;

    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer > CheckFrequency)
        {
            CheckTimer = Time.time;

            if (IsUnderConstruction && HasAssignedWorker)
            {
                ProcessValue += .05f;

                if (ProcessValue >= 1f)
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
        if (!HasAssignedWorker)
        {
            ActiveWorker = _worker;
            BuildingEffect.Play();
        }
    }

    public void RemoveWorker()
    {
        if (HasAssignedWorker)
        {
         ActiveWorker = null;
        BuildingEffect.Stop();

        }
    }

    public void UnassignWorker()
    {
        ActiveWorker = null;
    }

    protected void CompleteConstruction()
    {
        ActiveWorker.Target = null;

        ActiveWorker.currentTask = WorkerTask.None;
        UnassignWorker();
        sr.sprite = m_BuildingProcess.BuildingAction.CompletionSprite;
        BuildingEffect.Stop();
        IsCompleted = true;
        m_BuildingProcess = null;

        if (TowerUnit != null)
        {
            TowerUnit.SetActive(true);
        }

        if (anim != null)
        {
            anim.enabled = true;
        }

    }

    public override void Death()
    {
        base.Death();

        if (anim != null)
        {
            anim.enabled = false;
        }

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
        yield return new WaitForSeconds(1f);
        sr.DOFade(0,3f).OnComplete(() => Destroy(gameObject));

    }

}
