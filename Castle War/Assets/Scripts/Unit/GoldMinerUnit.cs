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

            if (HasVaildWorker)
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
        _worker.RecordLastGoldMiner(this);
        _worker.gameObject.SetActive(false);
        WorkersInMiner.Enqueue(_worker);
    }

    private void LevelMiner(WorkerUnit _worker)
    {
        int goldAmount = Random.Range(3, 7);

        _worker.gameObject.SetActive(true);
        AudioManager.Get().PlaySFX(32);
        
        _worker.TransportResource(0, 0, goldAmount *50);
        _worker.UpdateWorkerTask(WorkerTask.Trasporting);
    }

    private IEnumerator TakeMiningProcess()
    {
        IsTakeMining = true;
        float time = Mathf.Clamp(ProductionFrequency - FindCastleCount(),ProductionFrequency * .5f,ProductionFrequency);

//        Debug.Log($"Enter Time : {time}");
        yield return new WaitForSeconds(time);
        var worker = WorkersInMiner.Dequeue();
        LevelMiner(worker);

        IsTakeMining = false;
    }
}
