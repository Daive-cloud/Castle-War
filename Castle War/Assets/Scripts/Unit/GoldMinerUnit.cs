using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GoldMinerUnit : StructureUnit
{
    [Header("Prodection Info")]
    [SerializeField] private Sprite ProductionImage;
    [SerializeField] private Sprite OriginalImage;
    [SerializeField] private float ProductionFrequency;
    private Queue<WorkerUnit> WorkersInMiner = new();
    private bool HasVaildWorker => WorkersInMiner.Count > 0;
    private bool IsTakeMining = false;

    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer >= CheckFrequency)
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

            if (HasVaildWorker && !IsDead)
            {
                sr.sprite = ProductionImage;
                if (!IsTakeMining)
                {
                    StartCoroutine(TakeMiningProcess());
                }
            }
            else
            {
                sr.sprite = OriginalImage;
            }

        }
    }

    public void EnterMiner(WorkerUnit _worker)
    {
        _worker.RecordLastEnteredGoldMiner(this);
        _worker.gameObject.SetActive(false);
        AudioManager.Get().PlaySFX(54);
        WorkersInMiner.Enqueue(_worker);
        BounceEffect();
    }

    private void LevelMiner(WorkerUnit _worker)
    {
        _worker.gameObject.SetActive(true);
        AudioManager.Get().PlaySFX(32);

        if (Random.Range(0, 100) <= 10)
        {
            int crystalAmount = Random.Range(2, 5);
            _worker.IsDiggedCrystal = true;
            _worker.TransportResource(0, 0, 0, crystalAmount * 5);
        }
        else
        {
            int goldAmount = Random.Range(3, 7);
            _worker.IsDiggedCrystal = false;
            _worker.TransportResource(0, 0, goldAmount * 50, 0);
        }

        _worker.UpdateWorkerTask(WorkerTask.Trasporting);
        BounceEffect();
    }

    private IEnumerator TakeMiningProcess()
    {
        IsTakeMining = true;
        float time = Mathf.Clamp(ProductionFrequency -WorkersInMiner.Count * 2,ProductionFrequency * .5f,ProductionFrequency);

//        Debug.Log($"Enter Time : {time}");
        yield return new WaitForSeconds(time);
        var worker = WorkersInMiner.Dequeue();
        LevelMiner(worker);

        IsTakeMining = false;
    }
}
