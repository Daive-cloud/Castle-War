using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;

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

    protected override void Start()
    {
        base.Start();
        CheckTimer = Time.time;
    }

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
        if (this.CompareTag("BlueUnit"))
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
        AudioManager.Get().PlaySFX(36);
        yield return new WaitForSeconds(2f);
        Destroy(GetComponent<CapsuleCollider2D>());
        yield return new WaitForFixedUpdate();
        TilemapManager.Get().UpdateNodesOverMap();
        sr.DOFade(0, 1f).OnComplete(() => Destroy(gameObject));
    }

    protected int FindCastleCount() => FindObjectsOfType<CastleUnit>().Where(unit => !unit.IsDead && unit.CompareTag("BlueUnit") && unit.IsCompleted).ToList().Count;

    public virtual void BounceEffect()
    {
        transform.DOKill();
        var originalScale = transform.localScale;
        var originalPos = transform.position;
        float spriteHeight = sr.bounds.size.y;

        // 用 DOValue 做缩放系数动画
        DOTween.To(() => 1f, scaleFactor =>
        {
            // 缩放
            transform.localScale = new Vector3(originalScale.x, originalScale.y * scaleFactor, originalScale.z);

            // 位置补偿（因为pivot在中心，要向下平移一半的高度差）
            float heightDiff = spriteHeight * (1f - scaleFactor) * 0.5f;
            transform.position = originalPos - new Vector3(0, heightDiff, 0);

        }, .9f, .1f)
        .SetEase(Ease.OutQuad)
        .SetLoops(2, LoopType.Yoyo)
        .OnComplete(() =>
        {
            // 保证回到原位置和缩放
            transform.localScale = originalScale;
            transform.position = originalPos;
        });
    }

    protected void OnDestroy()
    {
        transform.DOKill();
    }
}
