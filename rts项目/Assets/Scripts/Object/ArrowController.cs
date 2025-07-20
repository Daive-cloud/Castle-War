using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] private float FlySpeed;

    private Unit Owner;
    private Unit Target;

    private Vector3 Direction;
    private float DestroyTimer;

    public void RegisterArrow(Unit _owner,Unit _target)
    {
        Owner = _owner;
        Target = _target;
    }

    private void Update()
    {
        if(Target == null || Target.IsDead)
        {
            DestroyTimer += Time.deltaTime;

            if(DestroyTimer >= 2f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += Direction * FlySpeed * Time.deltaTime;
            return;
        }

        Direction = (Target.transform.position - transform.position).normalized;
        transform.position += Direction * FlySpeed * Time.deltaTime;

        if(Vector2.Distance(Target.transform.position,transform.position) < .1f)
        {
            Owner.stats.TakeDamage(Target.GetComponent<UnitStats>());
            AudioManager.Get().PlaySFX(10);
            Destroy(gameObject);
        }

    }
}
