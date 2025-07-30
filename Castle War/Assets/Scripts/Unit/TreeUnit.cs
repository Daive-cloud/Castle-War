using UnityEngine;
using DG.Tweening;
using System.Collections;
using IUnit;

public class TreeUnit : Unit , IResouceUnit
{
    [SerializeField] private GameObject woodPrefab;
    private Animator anim => GetComponentInChildren<Animator>();
    private WorkerUnit worker;

    public bool HasAssignedWorker => worker != null; // 隐式接口实现

    bool IResouceUnit.IsDead => IsDead;


    public void Shake()
    {
        worker.stats.TakeDamage(stats,10);
        anim.SetTrigger("Shake");
    }

    public override void Death()
    {
        base.Death();
        AudioManager.Get().PlaySFX(30);
        anim.SetTrigger("Death");
        if(worker != null)
            worker.ResetAnimation();
        StartCoroutine(WorkerCollectWood());
        sr.DOFade(0,2f).OnComplete(() => Destroy(gameObject));
    }

    public void AssignWorker(WorkerUnit _worker)
    {
        worker = _worker;
    }

    private IEnumerator WorkerCollectWood()
    {
        Destroy(GetComponent<CapsuleCollider2D>());
        yield return new WaitForFixedUpdate();
        TilemapManager.Get().UpdateNodesOverMap();
        int woodAmount = Random.Range(3, 7);

        for (int i = 0; i < woodAmount; i++)
        {
            float xOffset = Random.Range(-1.5f, 1.5f);
            float yOffset = Random.Range(.1f, .2f);
            var targetPos = transform.position + new Vector3(xOffset, yOffset, 0);
            var newWood = Instantiate(woodPrefab, transform.position, Quaternion.identity);

            Sequence sq = DOTween.Sequence();
            sq.Append(newWood.transform.DOJump(targetPos, 1f, 1, 1f).SetEase(Ease.OutBack));
            if (worker != null)
                sq.Append(newWood.transform.DOMove(worker.transform.position, .5f).SetEase(Ease.Linear).OnComplete(() => Destroy(newWood.gameObject)));
            else
                sq.Append(newWood.GetComponent<SpriteRenderer>().DOFade(0,1f).OnComplete(() => Destroy(newWood.gameObject)));


        }
        
        yield return new WaitForSeconds(1.5f);
        AudioManager.Get().PlaySFX(32);
        if (worker != null)
        {
            worker.TransportResource(woodAmount * 50,0,0);
            worker.UpdateWorkerTask(WorkerTask.Trasporting);
        }
    }

}
