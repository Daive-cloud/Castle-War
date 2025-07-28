using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BarrelUnit : HumanoidUnit
{
    [Header("Flame")]
    [SerializeField] private GameObject flamePrefab;
    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer >= CheckFrequency)
        {
            FindClosestEnemyInRange();
            CheckTimer = Time.time;
            if (HasRegisteredTarget)
            {
                anim.SetTrigger("Select");
                if (CanAttackTarget() && !IsDead)
                {
                    ai.ClearPath();
                    Death();
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
        }
    }

    public override void SelectedUnit()
    {
        base.SelectedUnit();
        anim.SetTrigger("Select");
    }

    public override void UnselectedUnit()
    {
        base.UnselectedUnit();
        anim.SetTrigger("Unselect");
    }

    public void TakeBomb()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ObjectCheckRadius);
        var vaildUnits = colliders.ToList().Where(unit => unit != null && unit != this.GetComponent<CapsuleCollider2D>() && unit.TryGetComponent(out Unit _));
        int damage = stats.Damage.GetValue();
        AudioManager.Get().PlaySFX(8);

        foreach (var unit in vaildUnits)
        {
            if (unit.TryGetComponent(out BarrelUnit barrel) && !barrel.IsDead)
            {
                barrel.Death();
                continue;
            }

            float distance = Vector2.Distance(unit.transform.position, transform.position);
            float value = Mathf.Clamp(distance / ObjectCheckRadius, .1f, .95f);
            int actualDamage = Mathf.CeilToInt((1 - value) * damage);
            stats.TakeDamage(unit.GetComponent<UnitStats>(), actualDamage);
        }

        var node = TilemapManager.Get().FindNode(transform.position);
        // Debug.Log($"Unit Position : {node.GetNodePosition()}");
        Vector2 deathPosition = new Vector2(node.CenterX, node.CenterY);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Random.Range(0, 100) <= 20)
                {
                    float gridX = deathPosition.x + i;
                    float gridY = deathPosition.y + j;

                    Vector2 position = new Vector2(gridX, gridY);

                    Instantiate(flamePrefab, position, Quaternion.identity);
                    AudioManager.Get().PlaySFX(13);
                }
            }
        }
    }

    public override void PlaySelectedSound()
    {
        AudioManager.Get().PlaySFX(26);
    }
}
