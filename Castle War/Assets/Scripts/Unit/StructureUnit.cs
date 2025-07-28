using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

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

    public List<WorkerUnit> RegisterdWorkers = new();
    public bool HasAssignedWorker => RegisterdWorkers.Count > 0;
    public int WorkerCount => RegisterdWorkers.Count;

    protected float ProcessValue = 0f;

    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer > CheckFrequency)
        {
            CheckTimer = Time.time;

            if (IsUnderConstruction && HasAssignedWorker)
            {
                ProcessValue += .01f * WorkerCount;

                if (ProcessValue >= 1f)
                {
                    CompleteConstruction();
                }
            }

            if (!IsCompleted)
            {
                return;
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
            BuildingEffect.Play();
        }
        if (!RegisterdWorkers.Contains(_worker))
        {
            RegisterdWorkers.Add(_worker);
        }

    }

    public void UnassignWorker(WorkerUnit _unit)
    {
        if (RegisterdWorkers.Contains(_unit))
        {
            RegisterdWorkers.Remove(_unit);
            if (!HasAssignedWorker)
            {
                BuildingEffect.Stop();
            }
        }
    }

    public void RemoveWorker()
    {
        RegisterdWorkers.Clear();
    }

    protected void CompleteConstruction()
    {
        foreach (var unit in RegisterdWorkers)
        {
            unit.Target = null;
            unit.UpdateWorkerTask(WorkerTask.None);
        }
        AudioManager.Get().PlaySFX(37);
        RemoveWorker();
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

        if (TowerUnit != null)
        {
            Destroy(TowerUnit);
        }
        StartCoroutine(AfterDeath());
    }

    private IEnumerator AfterDeath()
    {
        Destroy(GetComponent<CapsuleCollider2D>());
        yield return null;
        AudioManager.Get().PlaySFX(36);
        yield return new WaitForFixedUpdate();
        TilemapManager.Get().UpdateNodesOverMap();
        yield return new WaitForSeconds(1f);
        sr.DOFade(0, 3f).OnComplete(() => Destroy(gameObject));

    }

    protected int FindCastleCount() => FindObjectsOfType<CastleUnit>().Where(unit => !unit.IsDead && unit.CompareTag("BlueUnit") && unit.IsCompleted).ToList().Count;

}
