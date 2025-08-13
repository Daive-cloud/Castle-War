using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class MagicController : MonoBehaviour
{
    [SerializeField] private float adsorbRadius;
    [SerializeField] private float attackRadius;
    private Unit owner;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, adsorbRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void RegisterMagic(Unit _owner)
    {
        owner = _owner;
    }

    private void TakeMagicDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, adsorbRadius);
        var targets = colliders.Select(unit => unit.GetComponent<Unit>()).
                                    Where(unit => unit != null && !unit.IsDead && unit.tag != owner.tag && !unit.TryGetComponent(out StructureUnit _)&& !unit.TryGetComponent(out TreeUnit _)).ToList();
        foreach (var unit in targets)
        {
            unit.transform.DOMove(transform.position, .5f);
        }

        colliders = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        targets = colliders.Select(unit => unit.GetComponent<Unit>()).Where(unit => unit != null && !unit.IsDead && unit.tag != owner.tag).ToList();
        foreach (var unit in targets)
        {
            owner.stats.TakeDamage(unit.stats);
        }
    }
}
