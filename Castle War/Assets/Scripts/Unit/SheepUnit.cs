
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using IUnit;
public class SheepUnit : HumanoidUnit, IResouceUnit
{
    [SerializeField] private GameObject meatPrefab;
    private float MoveFrequency = 5f;
    private float lastTimeMoved = 0f;

    bool IResouceUnit.IsDead => IsDead;

    public bool HasAssignedWorker => Target != null;

    protected override void UpdateBehaviour()
    {
        if (Time.time - lastTimeMoved >= MoveFrequency)
        {
            lastTimeMoved = Time.time;
            MoveFrequency = Random.Range(5f, 15f);
            RandomMovement();
        }
    }

    public override void Death()
    {
        base.Death();
        if (Target != null)
        {
            (Target as WorkerUnit).ResetAnimation();
            StartCoroutine(WorkerCollectMeat());
        }

    }


    private void RandomMovement()
    {
        List<Vector2> vaildPos = new();
        var node = TilemapManager.Get().FindNode(transform.position);
        var nowPos = new Vector2Int(node.ButtomX, node.ButtomY);
        for (int i = -3; i <= 3; i++)
        {
            for (int j = -3; j <= 3; j++)
            {
                int gridX = nowPos.x + i;
                int gridY = nowPos.y + j;
                var targetPos = new Vector3Int(gridX, gridY, 0);
                if (TilemapManager.Get().CanWalkAtTile(targetPos))
                {
                    vaildPos.Add(new Vector2(targetPos.x, targetPos.y));
                }
            }
        }
        MoveToDestination(vaildPos[Random.Range(0, vaildPos.Count - 1)]);
    }

    public override void PlayDeathSound()
    {
        AudioManager.Get().PlaySFX(34);
    }

    private IEnumerator WorkerCollectMeat()
    {
        int meatAmount = Random.Range(2, 5);
        Target.GetComponent<AI>().ClearPath();

        for (int i = 0; i < meatAmount; i++)
        {
            float xOffset = Random.Range(-1.5f, 1.5f);
            float yOffset = Random.Range(.1f, .2f);
            var targetPos = transform.position + new Vector3(xOffset, yOffset, 0);
            var newMeat = Instantiate(meatPrefab, transform.position, Quaternion.identity);

            Sequence sq = DOTween.Sequence();
            sq.Append(newMeat.transform.DOJump(targetPos, 1f, 1, 1f).SetEase(Ease.OutBack));
            if (Target != null)
                sq.Append(newMeat.transform.DOMove(Target.transform.position, .5f).SetEase(Ease.Linear).OnComplete(() => Destroy(newMeat.gameObject)));
            else
                sq.Append(newMeat.GetComponent<SpriteRenderer>().DOFade(0, 1f).OnComplete(() => Destroy(newMeat.gameObject)));

        }

        yield return new WaitForSeconds(1.5f);
        AudioManager.Get().PlaySFX(32);
        var worker = Target.GetComponent<WorkerUnit>();
        worker.TransportResource(0, meatAmount * 50, 0);
        worker.UpdateWorkerTask(WorkerTask.Trasporting);
    }

    public void AssignWorker(WorkerUnit _worker) => AssignTarget(_worker);

    public void Escape()
    {
        if (Target != null)
        {
            Target.stats.TakeDamage(stats, 10);
            RandomMovement();
        }
    }
}
